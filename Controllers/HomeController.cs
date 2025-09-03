using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Seed_Admin.Infra;
using Seed_Admin.Models;
using System.Data;
using System.Globalization;
using System.Net;

namespace Seed_Admin.Controllers
{
	public class HomeController : BaseController<ResponseModel<LoginViewModel>>
	{
		public HomeController(IRepositoryWrapper repository) : base(repository) { }

		public IActionResult Index()
		{
			if (!Common.IsUserLogged())
				return RedirectToAction("Login", "Home", new { Area = "" });


			if (Common.IsDealer())
			{
				var listOrderDetail = new List<Order_Detail>();

				List<SqlParameter> sqlParameters = new List<SqlParameter>();
				sqlParameters.Add(new SqlParameter("@Id", SqlDbType.BigInt) { Value = -1 });
				sqlParameters.Add(new SqlParameter("@User_Id", SqlDbType.BigInt) { Value = Common.LoggedUser_Id() });

				var ds = DataContext_Command.ExecuteStoredProcedure_DataSet("SP_ORDERS_GET", sqlParameters);

				if (ds != null && ds.Tables.Count > 1 && ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
				{
					foreach (DataRow dr in ds.Tables[1].Rows)
					{
						listOrderDetail.Add(new Order_Detail()
						{
							Id = dr["Id"] != DBNull.Value ? Convert.ToInt64(dr["Id"]) : 0,
							Product_ID = dr["Product_ID"] != DBNull.Value ? Convert.ToInt64(dr["Product_ID"]) : 0,
							PackageType_ID = dr["PackageType_ID"] != DBNull.Value ? Convert.ToInt64(dr["PackageType_ID"]) : 0,
							SKUSize_ID = dr["SKUSize_ID"] != DBNull.Value ? Convert.ToInt64(dr["SKUSize_ID"]) : 0,
							Qty = dr["Qty"] != DBNull.Value ? Convert.ToDecimal(dr["Qty"]) : 0,
							Product_Name = dr["Product_Name"] != DBNull.Value ? Convert.ToString(dr["Product_Name"]) : "",
							PackageType_Name = dr["PackageType_Name"] != DBNull.Value ? Convert.ToString(dr["PackageType_Name"]) : "",
							SKUSize_Name = dr["SKUSize_Name"] != DBNull.Value ? Convert.ToString(dr["SKUSize_Name"]) : "",
						});
					}
				}

				var dictLoyaltyPoint = _context.Using<LoyaltyPoint>().GetByCondition(x => x.UserId == Common.LoggedUser_Id()).ToList().ToDictionary(x => x.LoyaltyPointSchemeId);

				var listLoyaltyPointScheme = _context.Using<LoyaltyPointScheme>().GetByCondition(x => dictLoyaltyPoint.Keys.Contains(x.Id)).ToList();

				var ids = listLoyaltyPointScheme.Select(x => x.ProductID).Distinct().ToList();
				var dictProduct = _context.Using<Product>().GetByCondition(x => ids.Contains(x.Id)).ToDictionary(x => x.Id);
				var dictOrder = listOrderDetail.Where(x => ids.Contains(x.Product_ID)).GroupBy(x => x.Product_ID).ToDictionary(g => g.Key, g => (int)g.Sum(x => x.Qty));

				var list = listLoyaltyPointScheme.Select(x =>
				{
					dictLoyaltyPoint.TryGetValue(x.Id, out var lp);
					dictProduct.TryGetValue(x.ProductID, out var product);
					dictOrder.TryGetValue(x.ProductID, out var totalQty);

					return new LoyaltyPointViewModel
					{
						Points = lp?.Points ?? 0,
						QrCode = x.SchemeName ?? "",
						ProductName = product?.Name ?? "",
						Qty = x?.MaxPurchaseQty ?? 0,
						OrderQty = totalQty,

						UserId = lp?.UserId ?? 0,
						ClaimedDate_Ticks = x?.EffectiveStartDate?.Ticks ?? 0,
						ExpiryDate_Ticks = x?.EffectiveEndDate?.Ticks ?? 0,
						GenerateDate_Ticks = lp?.CreatedDate?.Ticks ?? 0,
						ClaimedDate_Text = x?.EffectiveStartDate?.ToString("dd/MM/yyyy HH:mm:ss").Replace("-", "/") ?? "",
						ExpiryDate_Text = x?.EffectiveEndDate?.ToString("dd/MM/yyyy").Replace("-", "/") ?? "",
						GenerateDate_Text = lp?.CreatedDate?.ToString("dd/MM/yyyy HH:mm:ss").Replace("-", "/") ?? ""
					};
				}).ToList();

				CommonViewModel.Data = list.Distinct().ToList();
			}


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
				if (!string.IsNullOrEmpty(viewModel.UserName) && viewModel.UserName.Length > 0 && _context.Using<User>().Any(x => x.UserName == viewModel.UserName))
				{
					viewModel.Password = Common.Encrypt(viewModel.Password);

					var obj = _context.Using<User>().GetByCondition(x => x.UserName == viewModel.UserName && x.Password == viewModel.Password).FirstOrDefault();

					if (obj != null && obj.IsActive == true && obj.IsDeleted == false)
					{
						var userRole = _context.Using<UserRoleMapping>().GetByCondition(x => x.UserId == obj.Id).OrderByDescending(x => x.IsPrimary).FirstOrDefault();

						obj.RoleId = userRole != null ? userRole.RoleId : 0;
						obj.PlantId = userRole != null ? userRole.PlantId : 0;

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
						Common.Set_Session_Int(SessionKey.KEY_USER_PLANT_ID, obj.PlantId);

						Common.Set_Session(SessionKey.KEY_USER_NAME, obj.UserName);
						Common.Set_Session(SessionKey.KEY_USER_ROLE, role.Name);
						Common.Set_Session_Int(SessionKey.KEY_IS_ADMIN, (role.IsAdmin || obj.RoleId == 1 ? 1 : 0));
						Common.Set_Session_Int(SessionKey.KEY_IS_SUPER_USER, (obj.RoleId == 1 ? 1 : 0));
						Common.Set_Session_Int(SessionKey.KEY_IS_Dealer, (role.Name.ToLower().Contains("dealer") ? 1 : 0));
						Common.Set_Session_Int(SessionKey.KEY_IS_Distributor, (role.Name.ToLower().Contains("distributor") ? 1 : 0));
						Common.Set_Session_Int(SessionKey.KEY_IS_Farmer, (role.Name.ToLower().Contains("farmer") ? 1 : 0));

						List<Plant> plants = (from u in _context.Using<User>().GetAll().ToList()
											  join m in _context.Using<UserRoleMapping>().GetAll().ToList() on u.Id equals m.UserId
											  join p in _context.Using<Plant>().GetAll().ToList() on m.PlantId equals p.Id
											  where (role != null && role.Id == 1) ? true : u.Id == obj.Id
											  select p).Distinct().ToList();

						Common.Configure_UserPlantAccess(plants);

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

		[HttpPost]
		public IActionResult Change_Plant(long id)
		{
			try
			{
				if (id > 0)
				{
					var userId = Common.Get_Session_Int(SessionKey.KEY_USER_ID);

					var userRole = _context.Using<UserRoleMapping>().GetByCondition(x => x.UserId == userId && (x.PlantId == id || x.RoleId == 1)).OrderByDescending(x => x.IsPrimary).FirstOrDefault();

					userRole.PlantId = userRole.RoleId > 1 ? userRole.PlantId : id;

					List<UserMenuAccess> listMenuAccess = new List<UserMenuAccess>();
					List<UserMenuAccess> listMenuPermission = new List<UserMenuAccess>();

					Role role = _context.Using<Role>().GetByCondition(x => x.Id == userRole.RoleId).FirstOrDefault();

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
										  where x.UserId == userId && x.RoleId == userRole.RoleId
										  && y.IsActive == true && y.IsDeleted == false && x.IsActive == true && x.IsDeleted == false && y.Name != "Menu" && y.IsAdmin == true
										  && x.IsRead == true
										  select new UserMenuAccess() { Id = y.Id, ParentMenuId = y.ParentId, Area = y.Area, Controller = y.Controller, Url = y.Url, MenuName = y.Name, DisplayOrder = y.DisplayOrder, IsActive = x.IsActive, IsDeleted = x.IsDeleted }).ToList();
					}
					else if (role != null && !role.IsAdmin && role.IsActive && !role.IsDeleted)
					{
						listMenuAccess = (from x in _context.Using<UserMenuAccess>().GetAll().ToList()
										  join y in _context.Using<Menu>().GetAll().ToList() on x.MenuId equals y.Id
										  where x.UserId == userId && x.RoleId == userRole.RoleId
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
											  where x.UserId == userId && y.IsActive == true && y.IsDeleted == false && x.IsActive == true && x.IsDeleted == false
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

					Common.Set_Session_Int(SessionKey.KEY_USER_ROLE_ID, userRole.RoleId);
					Common.Set_Session_Int(SessionKey.KEY_USER_PLANT_ID, userRole.PlantId);

					Common.Set_Session_Int(SessionKey.KEY_IS_ADMIN, (role.IsAdmin || userRole.RoleId == 1 ? 1 : 0));
					Common.Set_Session_Int(SessionKey.KEY_IS_SUPER_USER, (userRole.RoleId == 1 ? 1 : 0));


					List<Plant> plants = (from u in _context.Using<User>().GetAll().ToList()
										  join m in _context.Using<UserRoleMapping>().GetAll().ToList() on u.Id equals m.UserId
										  join p in _context.Using<Plant>().GetAll().ToList() on m.PlantId equals p.Id
										  where (role != null && role.Id == 1) ? true : u.Id == userId
										  select p).Distinct().ToList();

					Common.Configure_UserPlantAccess(plants);

					CommonViewModel.IsSuccess = true;
					CommonViewModel.StatusCode = ResponseStatusCode.Success;
					CommonViewModel.Message = ResponseStatusMessage.Success;

					CommonViewModel.RedirectURL = Url.Content("~/") /*+ "Admin/"*/ + this.ControllerContext.RouteData.Values["Controller"].ToString() + "/Index";

					return Json(CommonViewModel);
				}

			}
			catch (Exception ex)
			{
				LogService.LogInsert(GetCurrentAction(), "", ex);

				CommonViewModel.IsConfirm = true;
				CommonViewModel.IsSuccess = false;
				CommonViewModel.StatusCode = ResponseStatusCode.Error;
				CommonViewModel.Message = ResponseStatusMessage.Error;
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
