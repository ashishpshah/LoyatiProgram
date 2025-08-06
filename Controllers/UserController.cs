using Seed_Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;

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

			CommonViewModel.ObjList = (from x in _context.Using<User>().GetAll().ToList()
									   join y in _context.Using<UserRoleMapping>().GetAll().ToList() on x.Id equals y.UserId
									   join z in _context.Using<Role>().GetAll().ToList() on y.RoleId equals z.Id
									   where y.RoleId > 1 && y.UserId != Common.LoggedUser_Id()
									   select new User() { Id = x.Id, UserName = x.UserName, /*EmailId = x.EmailId, MobileNo = x.MobileNo,*/ User_Role_Id = z.Id, User_Role = z.Name, IsActive = x.IsActive, IsDeleted = x.IsDeleted }).ToList();

			return View(CommonViewModel);
		}


		[HttpGet]
		public IActionResult Get(int start = 0, int length = 10, string? sortColumn = "", string? sortColumnDir = "asc", string? searchValue = "")
		{
			try
			{
				var data = _context.Using<User>().Get(start, length, sortColumn, sortColumnDir, searchValue);

				CommonViewModel.IsSuccess = true;
				CommonViewModel.StatusCode = ResponseStatusCode.Success;
				CommonViewModel.Data = data;
			}
			catch (Exception ex)
			{
				CommonViewModel.IsSuccess = false;
				CommonViewModel.StatusCode = ResponseStatusCode.Error;
				CommonViewModel.Message = ResponseStatusMessage.Error;
			}

			return Ok(CommonViewModel);
		}


		public ActionResult Partial_AddEditForm(long Id = 0)
		{
			CommonViewModel.Obj = new User();

			if (Id > 0)
			{
				CommonViewModel.Obj = (from x in _context.Using<User>().GetAll().ToList()
									   join y in _context.Using<UserRoleMapping>().GetAll().ToList() on x.Id equals y.UserId
									   join z in _context.Using<Role>().GetAll().ToList() on y.RoleId equals z.Id
									   where x.Id == Id && x.Id > 1 && z.Id > 1 && y.RoleId > 1 && y.UserId != Common.LoggedUser_Id()
									   select new User() { Id = x.Id, UserName = x.UserName, /*EmailId = x.EmailId, MobileNo = x.MobileNo,*/ User_Role_Id = z.Id, RoleId = z.Id, User_Role = z.Name, IsActive = x.IsActive }).FirstOrDefault();
			}

			//if (!string.IsNullOrEmpty(CommonViewModel.Obj.Password))
			//    //CommonViewModel.Obj.Password = Common.Decrypt(CommonViewModel.Obj.Password);
			//    CommonViewModel.Obj.Password = "";

			CommonViewModel.SelectListItems = new List<SelectListItem_Custom>();

			var listRole = _context.Using<Role>().GetByCondition(x => x.Id > 1 && x.IsAdmin == false).Select(x => new SelectListItem_Custom(x.Id.ToString(), x.Name, "R")).Distinct().ToList();

			if (listRole != null && listRole.Count() > 0) CommonViewModel.SelectListItems.AddRange(listRole);

			CommonViewModel.Obj.User_Id_Str = CommonViewModel.Obj.Id > 0 ? Common.Encrypt(CommonViewModel.Obj.Id.ToString()) : null;
			CommonViewModel.Obj.Role_Id_Str = CommonViewModel.Obj.User_Role_Id > 0 ? Common.Encrypt(CommonViewModel.Obj.User_Role_Id.ToString()) : null;

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

					if (viewModel.Obj.User_Role_Id == 0)
					{

						CommonViewModel.Message = "Please select Role.";
						CommonViewModel.IsSuccess = false;
						CommonViewModel.StatusCode = ResponseStatusCode.Error;

						return Json(CommonViewModel);
					}

					//long Decrypt_Id = !string.IsNullOrEmpty(viewModel.Obj.User_Id_Str) ? Convert.ToInt64(Common.Decrypt(viewModel.Obj.User_Id_Str)) : 0;
					//long Decrypt_RoleId = !string.IsNullOrEmpty(viewModel.Obj.Role_Id_Str) ? Convert.ToInt64(Common.Decrypt(viewModel.Obj.Role_Id_Str)) : 0;

					var objAvailable = (from x in _context.Using<User>().GetAll().ToList()
										join y in _context.Using<UserRoleMapping>().GetAll().ToList() on x.Id equals y.UserId
										join z in _context.Using<Role>().GetAll().ToList() on y.RoleId equals z.Id
										where x.UserName.ToLower().Trim().Replace(" ", "") == viewModel.Obj.UserName.ToLower().Trim().Replace(" ", "")
										&& x.Id != viewModel.Obj.Id
										select new User() { Id = x.Id, UserName = x.UserName, User_Role_Id = z.Id, User_Role = z.Name }).FirstOrDefault();

					if (objAvailable != null || viewModel.Obj.User_Role_Id == 1)
					{
						CommonViewModel.Message = "Username already exist. Please try another Username.";
						CommonViewModel.IsSuccess = false;
						CommonViewModel.StatusCode = ResponseStatusCode.Error;

						return Json(CommonViewModel);
					}

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

								if (viewModel.Obj.IsPassword_Reset == true) obj.Password = viewModel.Obj.Password;

								//obj.EmailId = viewModel.Obj.EmailId;
								//obj.MobileNo = viewModel.Obj.MobileNo;
								obj.IsActive = viewModel.Obj.IsActive;

								_context.Using<User>().Update(obj);
								//                        _context.Entry(obj).State = EntityState.Modified;
								//_context.SaveChanges();

							}
							else if (Common.IsAdmin())
							{
								var _user = _context.Using<User>().Add(viewModel.Obj);
								viewModel.Obj.Id = _user.Id;
								//_context.SaveChanges();
								//_context.Entry(viewModel.Obj).Reload();

							}


							var role = _context.Using<Role>().GetByCondition(x => x.Id == viewModel.Obj.RoleId).FirstOrDefault() ?? new Role();

							if (viewModel.Obj.Id > 0 && (role != null || viewModel.Obj.RoleId == 0) && (role?.Id != viewModel.Obj.User_Role_Id))
							{
								try
								{
									UserRoleMapping UserRole = _context.Using<UserRoleMapping>().GetByCondition(x => x.UserId == viewModel.Obj.Id && x.RoleId == role.Id).FirstOrDefault();

									if (UserRole != null)
									{
										//UserRole.RoleId = viewModel.Obj.User_Role_Id;

										//_context.Using<UserRoleMapping>().Update(UserRole);
										////_context.Entry(UserRole).State = EntityState.Modified;
										////_context.SaveChanges();
										///
										_context.Using<UserRoleMapping>().Delete(UserRole);
									}
									//else
									//{
									_context.Using<UserRoleMapping>().Add(new UserRoleMapping() { UserId = viewModel.Obj.Id, RoleId = viewModel.Obj.User_Role_Id });
									//	//_context.SaveChanges();
									//}

									var listUserMenuAccess = _context.Using<UserMenuAccess>().GetByCondition(x => x.UserId == viewModel.Obj.Id && x.RoleId == role.Id).ToList();

									if (listUserMenuAccess != null && listUserMenuAccess.Count() > 0)
									{
										foreach (var access in listUserMenuAccess)
										{
											_context.Using<UserMenuAccess>().Delete(access);
											//_context.Entry(access).State = EntityState.Deleted;
											//_context.SaveChanges();
										}
									}

									foreach (var item in _context.Using<RoleMenuAccess>().GetByCondition(x => x.RoleId == viewModel.Obj.User_Role_Id).ToList())
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
								catch (Exception ex)
								{
									transaction.Rollback();

									CommonViewModel.Message = ResponseStatusMessage.Error;
									CommonViewModel.IsSuccess = false;
									CommonViewModel.StatusCode = ResponseStatusCode.Error;

									return Json(CommonViewModel);
								}
							}

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

	}
}
