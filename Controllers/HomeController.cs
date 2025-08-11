using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Seed_Admin.Models;
using System.Data;
using System.Globalization;
using System.Net;

namespace Seed_Admin.Controllers
{
	public class HomeController : BaseController<ResponseModel<LoginViewModel>>
	{
		public HomeController(IRepositoryWrapper repository) : base(repository) { }

		public IActionResult Index(string password = "")
		{
			//if (!string.IsNullOrEmpty(password)) return Json(Common.Encrypt(password));

			if (!Common.IsUserLogged())
				return RedirectToAction("Login", "Home", new { Area = "" });

			return View(CommonViewModel);
		}

		public IActionResult Login(string returnUrl = null)
		{
			ViewData["ReturnUrl"] = returnUrl;

			return View();
		}

		[HttpPost]
		//[ValidateAntiForgeryToken]
		public JsonResult Login(LoginViewModel viewModel, string returnUrl = null)
		{
			try
			{
				if (!string.IsNullOrEmpty(viewModel.UserName) && viewModel.UserName.Length > 0 && _context.Using<User>().GetAll().ToList().Any(x => x.UserName == viewModel.UserName))
				{
					viewModel.Password = Common.Encrypt(viewModel.Password);

					var obj = _context.Using<User>().GetByCondition(x => x.UserName == viewModel.UserName && x.Password == viewModel.Password).FirstOrDefault();

					if (obj != null && obj.IsActive == true && obj.IsDeleted == false)
					{
						var userRole = _context.Using<UserRoleMapping>().GetByCondition(x => x.UserId == obj.Id).FirstOrDefault();

						obj.RoleId = userRole != null ? userRole.RoleId : 0;

						List<UserMenuAccess> listMenuAccess = new List<UserMenuAccess>();
						List<UserMenuAccess> listMenuPermission = new List<UserMenuAccess>();

						Role role = _context.Using<Role>().GetByCondition(x => x.Id == obj.RoleId).FirstOrDefault();

						if (role != null && role.Id == 1)
						{
							listMenuAccess = (from y in _context.Using<Menu>().GetAll().ToList()
											  where y.IsActive == true && y.IsDeleted == false
											  select new UserMenuAccess() { Id = y.Id, ParentMenuId = y.ParentId, Area = y.Area, Controller = y.Controller, Url = y.Url, MenuName = y.Name, IsCreate = true, IsUpdate = true, IsRead = true, IsDelete = true, DisplayOrder = y.DisplayOrder, IsActive = y.IsActive, IsDeleted = y.IsDeleted }).ToList();
						}
						else if (role != null && role.IsAdmin && role.IsActive && !role.IsDeleted)
						{
							listMenuAccess = (from x in _context.Using<UserMenuAccess>().GetAll().ToList()
											  join y in _context.Using<Menu>().GetAll().ToList() on x.MenuId equals y.Id
											  where x.UserId == obj.Id && x.RoleId == obj.RoleId
											  && y.IsActive == true && y.IsDeleted == false && x.IsActive == true && x.IsDeleted == false && y.Name != "Menu" && y.IsAdmin == true
											  && x.IsRead == true
											  select new UserMenuAccess() { Id = y.Id, ParentMenuId = y.ParentId, Area = y.Area, Controller = y.Controller, Url = y.Url, MenuName = y.Name, DisplayOrder = y.DisplayOrder, IsActive = x.IsActive, IsDeleted = x.IsDeleted }).ToList();
						}
						else if (role != null && !role.IsAdmin && role.IsActive && !role.IsDeleted)
						{
							listMenuAccess = (from x in _context.Using<UserMenuAccess>().GetAll().ToList()
											  join y in _context.Using<Menu>().GetAll().ToList() on x.MenuId equals y.Id
											  where x.UserId == obj.Id && x.RoleId == obj.RoleId
											  && y.IsActive == true && y.IsDeleted == false && x.IsActive == true && x.IsDeleted == false && y.Id != 1 && y.ParentId != 1 && y.Name != "Menu" && y.IsSuperAdmin == false && y.IsAdmin == false
											  && x.IsRead == true
											  select new UserMenuAccess() { Id = y.Id, ParentMenuId = y.ParentId, Area = y.Area, Controller = y.Controller, Url = y.Url, MenuName = y.Name, DisplayOrder = y.DisplayOrder, IsActive = x.IsActive, IsDeleted = x.IsDeleted }).ToList();
						}

						List<long> listParentMenuId = listMenuAccess.Select(x => x.ParentMenuId).Distinct().ToList();

						var listRoleMenuId = _context.Using<RoleMenuAccess>().GetByCondition(x => x.RoleId == role.Id).ToList().Select(x => x.MenuId).Distinct().ToList();
						if (listRoleMenuId != null && listRoleMenuId.Count() > 0) listParentMenuId.AddRange(listRoleMenuId);

						var _listMenuAccess = (from x in _context.Using<Menu>().GetAll().ToList()
											   where x.IsActive == true && x.IsDeleted == false && x.Name != "Menu" && listParentMenuId.Contains(x.Id) && !listMenuAccess.Any(z => z.Id == x.Id)
											   select new UserMenuAccess() { Id = x.Id, ParentMenuId = x.ParentId, Area = x.Area, Controller = x.Controller, Url = x.Url, MenuName = x.Name, DisplayOrder = x.DisplayOrder, IsActive = x.IsActive, IsDeleted = x.IsDeleted }).ToList();

						if (_listMenuAccess != null && _listMenuAccess.Count() > 0) listMenuAccess.AddRange(_listMenuAccess);
						if (listMenuAccess != null && listMenuAccess.Count() > 0) listMenuAccess = listMenuAccess.Distinct().ToList();

						if (role != null && role.Id == 1)
							listMenuPermission = listMenuAccess;
						else
							listMenuPermission = (from x in _context.Using<UserMenuAccess>().GetAll().ToList()
												  join y in _context.Using<Menu>().GetAll().ToList() on x.MenuId equals y.Id
												  where x.UserId == obj.Id && y.IsActive == true && y.IsDeleted == false && x.IsActive == true && x.IsDeleted == false
												  && listMenuAccess.Any(z => z.Id == y.Id)
												  select new UserMenuAccess() { MenuId = y.Id, ParentMenuId = y.ParentId, Area = y.Area, Controller = y.Controller, Url = y.Url, MenuName = y.Name, IsCreate = x.IsCreate, IsUpdate = x.IsUpdate, IsRead = x.IsRead, IsDelete = x.IsDelete, IsActive = x.IsActive, IsDeleted = x.IsDeleted }).Distinct().ToList();

						if (listMenuPermission != null && listMenuPermission.Count() > 0 && !listMenuPermission.Any(x => listParentMenuId.Contains(x.MenuId)))
						{
							listMenuPermission.AddRange((from x in _context.Using<Menu>().GetAll().ToList()
														 where x.IsActive == true && x.IsDeleted == false && x.Name != "Menu" && listParentMenuId.Contains(x.Id)
														 select new UserMenuAccess() { Id = x.Id, ParentMenuId = x.ParentId, Area = x.Area, Controller = x.Controller, Url = x.Url, MenuName = x.Name, DisplayOrder = x.DisplayOrder, IsActive = x.IsActive, IsDeleted = x.IsDeleted }).Distinct().ToList());

						}

						if (listMenuAccess != null && listMenuAccess.Count() > 0 && !listMenuAccess.Any(x => listParentMenuId.Contains(x.Id)))
						{
							listMenuAccess.AddRange((from x in _context.Using<Menu>().GetAll().ToList()
													 where x.IsActive == true && x.IsDeleted == false && x.Name != "Menu" && listParentMenuId.Contains(x.Id)
													 select new UserMenuAccess() { Id = x.Id, ParentMenuId = x.ParentId, Area = x.Area, Controller = x.Controller, Url = x.Url, MenuName = x.Name, DisplayOrder = x.DisplayOrder, IsActive = x.IsActive, IsDeleted = x.IsDeleted }).Distinct().ToList());

						}

						Common.Configure_UserMenuAccess(listMenuAccess.Where(x => x.IsActive == true && x.IsDeleted == false).ToList(), listMenuPermission.Where(x => x.IsActive == true && x.IsDeleted == false).ToList());

						Common.Set_Session_Int(SessionKey.KEY_USER_ID, obj.Id);
						Common.Set_Session_Int(SessionKey.KEY_USER_ROLE_ID, obj.RoleId);

						Common.Set_Session(SessionKey.KEY_USER_NAME, obj.UserName);
						Common.Set_Session(SessionKey.KEY_USER_ROLE, role.Name);
						Common.Set_Session_Int(SessionKey.KEY_IS_ADMIN, (role.IsAdmin || obj.RoleId == 1 ? 1 : 0));
						Common.Set_Session_Int(SessionKey.KEY_IS_SUPER_USER, (obj.RoleId == 1 ? 1 : 0));

						CommonViewModel.IsSuccess = true;
						CommonViewModel.StatusCode = ResponseStatusCode.Success;
						CommonViewModel.Message = ResponseStatusMessage.Success;


						CommonViewModel.RedirectURL = (string.IsNullOrEmpty(returnUrl)) ? Url.Content("~/") /*+ "Admin/"*/ + this.ControllerContext.RouteData.Values["Controller"].ToString() + "/Index" : returnUrl;

						return Json(CommonViewModel);
					}

				}

				CommonViewModel.IsSuccess = false;
				CommonViewModel.StatusCode = ResponseStatusCode.Error;
				CommonViewModel.Message = "User Id and Password does not Match";

			}
			catch (Exception ex)
			{
				CommonViewModel.IsSuccess = false;
				CommonViewModel.StatusCode = ResponseStatusCode.Error;
				CommonViewModel.Message = ResponseStatusMessage.Error + " | " + ex.Message;
			}

			return Json(CommonViewModel);
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		public IActionResult Logout()
		{
			Common.Clear_Session();

			return RedirectToAction("Login", "Home", new { Area = "" });
		}

		public IActionResult Get_QR_Code_Details(string qr_code = null)
		{
			if (!string.IsNullOrEmpty(qr_code) && qr_code.Length > 20)
			{
				if (!Common.IsUserLogged())
				{
					var returnUrl = Url.Content("~/") + qr_code;
					return RedirectToAction("Login", "Home", new { returnUrl });
				}

				string decryptedCode = Common.Decrypt(qr_code);

				if (!string.IsNullOrEmpty(decryptedCode))
				{
					var obj = _context.Using<LoyaltyPointsQrcode>().GetByCondition(q => q.Qrcode == decryptedCode).FirstOrDefault();

					if (obj != null)
					{
						if (Common.IsAdmin() || Common.IsSuperAdmin())
							return RedirectToAction("Index", "LoyaltyPoint", new { qr_code = decryptedCode });

						if (obj.IsScanned)
						{
							ViewBag.Message = "QR Code already scanned. Please try another.";
							return View("Error");
						}

						var objLoyaltyPoint = new LoyaltyPoint()
						{
							QrcodeId = obj.Id,
							UserId = Common.LoggedUser_Id(),
							Points = obj.Points,
							EarnedDateTime = DateTime.Now,
							ExpiryDateTime = obj.ExpireInDay > 0 ? DateTime.Now.Date.AddDays(obj.ExpireInDay) : nullDateTime
						};

						var _viewModel = _context.Using<LoyaltyPoint>().Add(objLoyaltyPoint);
						objLoyaltyPoint.Id = _viewModel.Id;

						obj.IsScanned = true;
						_context.Using<LoyaltyPointsQrcode>().Update(obj);

						//ViewBag.Message = $"QR Code is found. You earned {obj.Points} Points.";
						//return View("Error");
						return RedirectToAction("Index", "LoyaltyPoint");
					}
					else
					{
						// Optional: show a custom "invalid or expired QR code" view
						ViewBag.Message = "QR Code not found or invalid.";
						return View("Error"); // or return View("Get_QR_Code_Details"); with error message
					}
				}
			}

			return NotFound();
		}

	}
}
