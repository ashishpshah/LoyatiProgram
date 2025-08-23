using Seed_Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using Seed_Admin.Infra;
using static QRCoder.PayloadGenerator.SwissQrCode;
using static QRCoder.PayloadGenerator;
using Seed_Admin.Infra.Services;

namespace Seed_Admin.Controllers
{
	public class UserController : BaseController<ResponseModel<User>>
	{

		public UserController(IRepositoryWrapper repository) : base(repository) { }

		// GET: Admin/User
		//[CustomAuthorizeAttribute(AccessType_Enum.Read)]
		public ActionResult Index()
		{
			CommonViewModel.ObjList = new List<User>();

			CommonViewModel.ObjList = (from u in _context.Using<User>().GetAll().ToList()
									   join m in _context.Using<UserRoleMapping>().GetAll().ToList() on u.Id equals m.UserId
									   join r in _context.Using<Role>().GetAll().ToList() on m.RoleId equals r.Id
									   join p in _context.Using<Plant>().GetAll().ToList() on m.PlantId equals p.Id
									   where r.Id > 1 && m.UserId != Common.LoggedUser_Id()
									   group new { m, r, p } by new { u.Id, u.UserName, u.IsActive, u.IsDeleted } into g
									   select new User()
									   {
										   Id = g.Key.Id,
										   UserName = g.Key.UserName,
										   User_Role = string.Join(", ", g.OrderByDescending(x => x.m.IsPrimary).Select(x => x.m.PlantId == 0 ? $"{x.r.Name}" : $"{x.p.PlantName} - {x.r.Name}")),
										   IsActive = g.Key.IsActive,
										   IsDeleted = g.Key.IsDeleted
									   }).ToList();

			//CommonViewModel.ObjList = (from x in _context.Using<User>().GetAll().ToList()
			//					   join y in _context.Using<UserRoleMapping>().GetAll().ToList() on x.Id equals y.UserId
			//					   join z in _context.Using<Role>().GetAll().ToList() on y.RoleId equals z.Id
			//					   where y.RoleId > 1 && y.UserId != Common.LoggedUser_Id()
			//					   select new User() { Id = x.Id, UserName = x.UserName, User_Role_Id = z.Id, User_Role = z.Name, IsActive = x.IsActive, IsDeleted = x.IsDeleted }).ToList();

			return View(CommonViewModel);
		}

		public ActionResult Partial_AddEditForm(long Id = 0)
		{
			CommonViewModel.Obj = new User();

			if (Id > 0)
			{
				//CommonViewModel.Obj = (from x in _context.Using<User>().GetAll().ToList()
				//					   join y in _context.Using<UserRoleMapping>().GetAll().ToList() on x.Id equals y.UserId
				//					   join z in _context.Using<Role>().GetAll().ToList() on y.RoleId equals z.Id
				//					   where x.Id == Id && x.Id > 1 && z.Id > 1 && y.RoleId > 1 && y.UserId != Common.LoggedUser_Id()
				//					   select new User() { Id = x.Id, UserName = x.UserName, /*EmailId = x.EmailId, MobileNo = x.MobileNo,*/ User_Role_Id = z.Id, RoleId = z.Id, User_Role = z.Name, IsActive = x.IsActive }).FirstOrDefault();
				CommonViewModel.Obj = (from u in _context.Using<User>().GetAll().ToList()
									   join m in _context.Using<UserRoleMapping>().GetAll().ToList() on u.Id equals m.UserId
									   where u.Id == Id && m.RoleId > 1 && m.UserId != Common.LoggedUser_Id()
									   group new { m } by new { u.Id, u.UserName, u.Email, u.ContactNo, u.Designation, u.Department, u.IsActive, u.IsDeleted } into g
									   select new User()
									   {
										   Id = g.Key.Id,
										   UserName = g.Key.UserName,
										   Email = g.Key.Email,
										   ContactNo = g.Key.ContactNo,
										   Designation = g.Key.Designation,
										   Department = g.Key.Department,
										   UserRoleMappings = g.Select(x => x.m).ToList(),
										   IsActive = g.Key.IsActive,
										   IsDeleted = g.Key.IsDeleted,
										   Plant_Role = string.Join(",", g.Select(x => x.m.PlantId + "|" + x.m.RoleId).ToArray()),
										   Default_Plant = g.Select(x => x.m).OrderByDescending(x => x.IsPrimary).Select(x => x.PlantId).FirstOrDefault()
									   }).FirstOrDefault();
			}

			//if (!string.IsNullOrEmpty(CommonViewModel.Obj.Password))
			//    //CommonViewModel.Obj.Password = Common.Decrypt(CommonViewModel.Obj.Password);
			//    CommonViewModel.Obj.Password = "";

			CommonViewModel.SelectListItems = new List<SelectListItem_Custom>();

			var listRole = _context.Using<Role>().GetByCondition(x => x.Id > 1 && (Common.IsSuperAdmin() ? true : x.IsAdmin == false))
				.Select(x => new SelectListItem_Custom(x.Id.ToString(), x.Name, x.IsActive.ToString(), "R")).Distinct().ToList();

			if (listRole != null && listRole.Count() > 0) CommonViewModel.SelectListItems.AddRange(listRole);

			var listPlant = _context.Using<Plant>().GetAll()
				.Select(x => new SelectListItem_Custom(x.Id.ToString(), x.PlantName, x.IsActive.ToString(), "P")).Distinct().ToList();

			if (listPlant != null && listPlant.Count() > 0)
			{
				listPlant.Insert(0, new SelectListItem_Custom("0", "All Plant", "P"));
				CommonViewModel.SelectListItems.AddRange(listPlant);
			}

			if (Id <= 0)
			{
				CommonViewModel.Obj = new User()
				{
					UserRoleMappings = new List<UserRoleMapping>() { new UserRoleMapping() { PlantId = Common.Get_Session_Int(SessionKey.KEY_USER_PLANT_ID), RoleId = Convert.ToInt64(listRole != null && listRole.Count() > 0 ? listRole.Select(x => x.Value).FirstOrDefault() : "0") } },
					IsActive = true,
					Plant_Role = Common.Get_Session_Int(SessionKey.KEY_USER_PLANT_ID) + "|" + (listRole != null && listRole.Count() > 0 ? listRole.Select(x => x.Value).FirstOrDefault() : "0"),
					Default_Plant = Common.Get_Session_Int(SessionKey.KEY_USER_PLANT_ID)
				};
			}

			return PartialView("_Partial_AddEditForm", CommonViewModel);
		}

		[HttpPost]
		public ActionResult Save(ResponseModel<User> viewModel)
		{
			try
			{
				if (viewModel != null && viewModel.Obj != null)
				{
					if (!string.IsNullOrEmpty(viewModel.Obj.Date_Text)) { try { viewModel.Obj.Date = DateTime.ParseExact(viewModel.Obj.Date_Text, "yyyy-MM-dd", CultureInfo.InvariantCulture); } catch { } }

					#region Validation

					if (string.IsNullOrEmpty(viewModel.Obj.UserName))
					{

						CommonViewModel.Message = "Please enter Username.";
						CommonViewModel.IsSuccess = false;
						CommonViewModel.StatusCode = ResponseStatusCode.Error;

						return Json(CommonViewModel);
					}

					if (_context.Using<User>().Any(x => x.UserName.ToLower() == viewModel.Obj.UserName.ToLower() && x.Id != viewModel.Obj.Id))
					{

						CommonViewModel.Message = "Username already exist. Please try another Username.";
						CommonViewModel.IsSuccess = false;
						CommonViewModel.StatusCode = ResponseStatusCode.Error;

						return Json(CommonViewModel);
					}

					if (string.IsNullOrEmpty(viewModel.Obj.Password) && viewModel.Obj.Id == 0)
					{

						CommonViewModel.Message = "Please enter Password.";
						CommonViewModel.IsSuccess = false;
						CommonViewModel.StatusCode = ResponseStatusCode.Error;

						return Json(CommonViewModel);
					}


					if (viewModel.Obj.ContactNo != null && !ValidateField.IsValidMobileNo(viewModel.Obj.ContactNo))
					{
						CommonViewModel.IsSuccess = false;
						CommonViewModel.Message = "Please enter valid Contact No.";
						CommonViewModel.StatusCode = ResponseStatusCode.Error;

						return Json(CommonViewModel);
					}

					if (viewModel.Obj.Email != null && !ValidateField.IsValidEmail(viewModel.Obj.Email))
					{
						CommonViewModel.IsSuccess = false;
						CommonViewModel.Message = "Please enter valid Email Address";
						CommonViewModel.StatusCode = ResponseStatusCode.Error;

						return Json(CommonViewModel);
					}

					if (string.IsNullOrEmpty(viewModel.Obj.Plant_Role))
					{

						CommonViewModel.Message = "Please select Role.";
						CommonViewModel.IsSuccess = false;
						CommonViewModel.StatusCode = ResponseStatusCode.Error;

						return Json(CommonViewModel);
					}

					//var objAvailable = (from x in _context.Using<User>().GetAll().ToList()
					//					join y in _context.Using<UserRoleMapping>().GetAll().ToList() on x.Id equals y.UserId
					//					join z in _context.Using<Role>().GetAll().ToList() on y.RoleId equals z.Id
					//					where x.UserName.ToLower().Trim().Replace(" ", "") == viewModel.Obj.UserName.ToLower().Trim().Replace(" ", "")
					//					&& x.Id != viewModel.Obj.Id
					//					select new User() { Id = x.Id, UserName = x.UserName, User_Role_Id = z.Id, User_Role = z.Name }).FirstOrDefault();

					//if (objAvailable != null || viewModel.Obj.User_Role_Id == 1)
					//{
					//	CommonViewModel.Message = "Username already exist. Please try another Username.";
					//	CommonViewModel.IsSuccess = false;
					//	CommonViewModel.StatusCode = ResponseStatusCode.Error;

					//	return Json(CommonViewModel);
					//}

					#endregion


					#region Database-Transaction

					using (var transaction = _context.BeginTransaction())
					{
						try
						{
							if (viewModel.Obj.IsPassword_Reset == true) viewModel.Obj.Password = "12345";

							if (!string.IsNullOrEmpty(viewModel.Obj.Password)) viewModel.Obj.Password = Common.Encrypt(viewModel.Obj.Password);

							//User obj = _context.Using<User>().Where(x => x.UserName.ToLower().Replace(" ", "") == viewModel.Obj.UserName.ToLower().Replace(" ", "")).FirstOrDefault();
							User obj = _context.Using<User>().GetByCondition(x => x.Id == viewModel.Obj.Id).FirstOrDefault();

							if (obj != null && Common.IsAdmin())
							{
								obj.UserName = viewModel.Obj.UserName;
								obj.Email = viewModel.Obj.Email;
								obj.ContactNo = viewModel.Obj.ContactNo;
								obj.Designation = viewModel.Obj.Designation;
								obj.Department = viewModel.Obj.Department;

								if (viewModel.Obj.IsPassword_Reset == true) obj.Password = viewModel.Obj.Password;

								obj.IsActive = viewModel.Obj.IsActive;

								_context.Using<User>().Update(obj);

							}
							else if (Common.IsAdmin())
							{
								var _user = _context.Using<User>().Add(viewModel.Obj);
								viewModel.Obj.Id = _user.Id;

							}

							List<UserRoleMapping> UserRole = _context.Using<UserRoleMapping>().GetByCondition(x => x.UserId == viewModel.Obj.Id && x.RoleId > 1).ToList();

							if (UserRole != null) foreach (var item in UserRole) _context.Using<UserRoleMapping>().Delete(item);

							var listUserMenuAccess = _context.Using<UserMenuAccess>().GetByCondition(x => x.UserId == viewModel.Obj.Id && x.RoleId > 1).ToList();

							if (listUserMenuAccess != null && listUserMenuAccess.Count() > 0) foreach (var access in listUserMenuAccess) _context.Using<UserMenuAccess>().Delete(access);

							var plantRoles = viewModel.Obj.Plant_Role.Split(',', StringSplitOptions.RemoveEmptyEntries)
											.Select(x =>
											{
												var parts = x.Split('|');
												return new { PlantId = long.Parse(parts[0]), RoleId = long.Parse(parts[1]) };
											}).ToList();

							if (plantRoles != null)
								foreach (var _item in plantRoles)
								{
									_context.Using<UserRoleMapping>().Add(new UserRoleMapping() { UserId = viewModel.Obj.Id, RoleId = _item.RoleId, PlantId = _item.PlantId, IsPrimary = viewModel.Obj.Default_Plant == _item.PlantId });

									foreach (var item in _context.Using<RoleMenuAccess>().GetByCondition(x => x.RoleId == _item.RoleId).ToList())
									{
										var userMenuAccess = new UserMenuAccess()
										{
											MenuId = item.MenuId,
											UserId = viewModel.Obj.Id,
											RoleId = viewModel.Obj.User_Role_Id,
											IsCreate = item.IsCreate,
											IsUpdate = item.IsUpdate,
											IsRead = item.IsRead,
											IsDelete = item.IsDelete,
											IsActive = item.IsActive,
											IsDeleted = item.IsDelete,
											IsSetDefault = true
										};

										_context.Using<UserMenuAccess>().Add(userMenuAccess);
										//_context.SaveChanges();
									}

								}

							//var role = _context.Using<Role>().GetByCondition(x => x.Id == viewModel.Obj.RoleId).FirstOrDefault() ?? new Role();

							//if (viewModel.Obj.Id > 0 && (role != null || viewModel.Obj.RoleId == 0) && (role?.Id != viewModel.Obj.User_Role_Id))
							//{
							//	try
							//	{
							//		UserRoleMapping UserRole = _context.Using<UserRoleMapping>().GetByCondition(x => x.UserId == viewModel.Obj.Id && x.RoleId == role.Id).FirstOrDefault();

							//		if (UserRole != null)
							//		{
							//			_context.Using<UserRoleMapping>().Delete(UserRole);
							//		}

							//		_context.Using<UserRoleMapping>().Add(new UserRoleMapping() { UserId = viewModel.Obj.Id, RoleId = viewModel.Obj.User_Role_Id });

							//		var listUserMenuAccess = _context.Using<UserMenuAccess>().GetByCondition(x => x.UserId == viewModel.Obj.Id && x.RoleId == role.Id).ToList();

							//		if (listUserMenuAccess != null && listUserMenuAccess.Count() > 0)
							//		{
							//			foreach (var access in listUserMenuAccess)
							//			{
							//				_context.Using<UserMenuAccess>().Delete(access);
							//			}
							//		}

							//		foreach (var item in _context.Using<RoleMenuAccess>().GetByCondition(x => x.RoleId == viewModel.Obj.User_Role_Id).ToList())
							//		{
							//			var userMenuAccess = new UserMenuAccess()
							//			{
							//				MenuId = item.MenuId,
							//				UserId = viewModel.Obj.Id,
							//				RoleId = viewModel.Obj.User_Role_Id,
							//				IsCreate = item.IsCreate,
							//				IsUpdate = item.IsUpdate,
							//				IsRead = item.IsRead,
							//				IsDelete = item.IsDelete,
							//				IsActive = item.IsActive,
							//				IsDeleted = item.IsDelete,
							//				IsSetDefault = true
							//			};

							//			_context.Using<UserMenuAccess>().Add(userMenuAccess);
							//			//_context.SaveChanges();
							//		}

							//	}
							//	catch (Exception ex)
							//	{
							//		transaction.Rollback();

							//		CommonViewModel.Message = ResponseStatusMessage.Error;
							//		CommonViewModel.IsSuccess = false;
							//		CommonViewModel.StatusCode = ResponseStatusCode.Error;

							//		return Json(CommonViewModel);
							//	}
							//}

							CommonViewModel.IsConfirm = true;
							CommonViewModel.IsSuccess = true;
							CommonViewModel.StatusCode = ResponseStatusCode.Success;
							CommonViewModel.Message = "Record saved successfully ! ";
							CommonViewModel.RedirectURL = Url.Action("Index", "User");

							transaction.Commit();

							return Json(CommonViewModel);
						}
						catch (Exception ex) { transaction.Rollback(); }
					}

					#endregion
				}
			}
			catch (Exception ex) { }


			CommonViewModel.Message = ResponseStatusMessage.Error;
			CommonViewModel.IsSuccess = false;
			CommonViewModel.StatusCode = ResponseStatusCode.Error;

			return Json(CommonViewModel);
		}

		public static bool IsValidEmail(string email)
		{
			if (string.IsNullOrWhiteSpace(email))
				return false;

			try
			{
				// Normalize the domain
				email = Regex.Replace(email, @"(@)(.+)$", DomainMapper, RegexOptions.None, TimeSpan.FromMilliseconds(200));

				// Examines the domain part of the email and normalizes it.
				string DomainMapper(Match match)
				{
					// Use IdnMapping class to convert Unicode domain names.
					var idn = new IdnMapping();

					// Pull out and process domain name (throws ArgumentException on invalid)
					string domainName = idn.GetAscii(match.Groups[2].Value);

					return match.Groups[1].Value + domainName;
				}
			}
			catch (RegexMatchTimeoutException e)
			{
				return false;
			}
			catch (ArgumentException e)
			{
				return false;
			}

			try
			{
				if (email.Contains(","))
				{
					foreach (var item in email.Split(','))
					{
						if (Regex.IsMatch(item,
						@"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z",
						RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250))) continue;
						else { break; return false; }
					}
					return true;
				}
				else
					return Regex.IsMatch(email,
						@"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z",
						RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
			}
			catch (RegexMatchTimeoutException)
			{
				return false;
			}
		}

		[HttpPost]
		//[CustomAuthorizeAttribute(AccessType_Enum.Delete)]
		public ActionResult DeleteConfirmed(long Id)
		{
			try
			{
				if (_context.Using<User>().GetAll().ToList().Any(x => x.Id == Id))
				{
					var UserRole = _context.Using<UserRoleMapping>().GetByCondition(x => x.UserId == Id).ToList();

					if (UserRole != null)
						foreach (var obj in UserRole)
						{
							_context.Using<UserRoleMapping>().Delete(obj);
							//_context.Entry(obj).State = EntityState.Deleted;
							//_context.SaveChanges();
						}

					var UserMenu = _context.Using<UserMenuAccess>().GetByCondition(x => x.UserId == Id).ToList();

					if (UserMenu != null)
						foreach (var obj in UserMenu)
						{
							_context.Using<UserMenuAccess>().Delete(obj);
							//_context.Entry(obj).State = EntityState.Deleted;
							//_context.SaveChanges();
						}

					var user = _context.Using<User>().GetByCondition(x => x.Id == Id).FirstOrDefault();

					if (user != null)
					{
						_context.Using<User>().Delete(user);
						//_context.Entry(user).State = EntityState.Deleted;
						//_context.SaveChanges();
					}



					CommonViewModel.IsConfirm = true;
					CommonViewModel.IsSuccess = true;
					CommonViewModel.StatusCode = ResponseStatusCode.Success;
					CommonViewModel.Message = "Data deleted successfully ! ";
					CommonViewModel.RedirectURL = Url.Action("Index", "User");

					return Json(CommonViewModel);
				}

			}
			catch (Exception ex)
			{ }


			CommonViewModel.Message = "Unable to delete User.";
			CommonViewModel.IsSuccess = false;
			CommonViewModel.StatusCode = ResponseStatusCode.Error;

			return Json(CommonViewModel);
		}


		[HttpPost]
		public JsonResult GetRoleByPlant(long plant_Id = 0)
		{
			var list = new List<SelectListItem_Custom>();
			try
			{
				list = _context.Using<Role>().GetByCondition(x => x.Id > 1 && (Common.IsSuperAdmin() ? true : x.IsAdmin == false))
					.Select(x => new SelectListItem_Custom(x.Id.ToString(), x.Name, x.IsActive.ToString(), "R")).Distinct().ToList();

			}
			catch (Exception ex) { LogService.LogInsert(GetCurrentAction(), "", ex); }

			return Json(list);
		}


		[HttpGet]
		public IActionResult Show_Menu(long RoleId = 0)
		{
			if (RoleId > 0)
				try
				{
					var dt = new DataTable();


					if (dt != null && dt.Rows.Count > 0)
					{
						var SelectedMenu = dt.Rows[0]["MENUS_NAME"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["MENUS_NAME"]) : "";

						var strHTML = "<div class=\"row col-12 w-100\"><table id=\"table_Common\" class=\"table table-bordered table-striped w-100 table_Common\"><thead><tr><th width=\"5%\">Sr. No.</th><th>Name</th></tr></thead><tbody>#</tbody></table></div>";

						var rows = "";

						var i = 0;
						if (!string.IsNullOrEmpty(SelectedMenu) && SelectedMenu.Contains("||"))
							foreach (string item in SelectedMenu.Split("||"))
								rows = rows + $"<tr><td>{++i}</td><td>{item}</td></tr>";
						else if (!string.IsNullOrEmpty(SelectedMenu))
							rows = rows + $"<tr><td>{++i}</td><td>{SelectedMenu}</td></tr>";

						strHTML = strHTML.Replace("#", rows);

						return Json(strHTML);
					}
				}
				catch (Exception ex) { LogService.LogInsert(GetCurrentAction(), "", ex); }


			return Json(null);
		}

	}
}
