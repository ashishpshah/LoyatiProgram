using Seed_Admin.Controllers;
using Seed_Admin.Infra;
using Microsoft.AspNetCore.Mvc;

namespace Seed_Admin.Areas.Admin.Controllers
{

	public class LoyaltyPointController : BaseController<ResponseModel>
	{
		public LoyaltyPointController(IRepositoryWrapper repository) : base(repository) { }

		// GET: Admin/LoyaltyPoint
		public ActionResult Index()
		{
			var list = new List<LoyaltyPointsQrcode>();

			if (Common.IsAdmin())
				list = _context.Using<LoyaltyPointsQrcode>().GetAll().Distinct().ToList();

			return View(CommonViewModel);
		}


		[HttpGet]
		public IActionResult GetData(int start = 0, int length = 10, string? sortColumn = "", string? sortColumnDir = "asc", string? searchValue = "")
		{
			try
			{
				var data = _context.Using<LoyaltyPointsQrcode>().Get(start, length, sortColumn, sortColumnDir, searchValue);

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


		////[CustomAuthorizeAttribute(AccessType_Enum.Read)]
		//public ActionResult Partial_AddEditForm(long Id = 0)
		//{
		//	CommonViewModel.Obj = new LoyaltyPoint();

		//	if (Common.IsAdmin() && Id > 1)
		//		CommonViewModel.Obj = _context.Using<LoyaltyPoint>().GetByCondition(x => x.Id == Id).FirstOrDefault();

		//	var listMenu = _context.Using<Menu>().GetAll().ToList();

		//	foreach (var item in listMenu.Where(x => x.ParentId > 0).ToList())
		//		item.ParentMenuName = listMenu.Where(x => x.Id == item.ParentId).Select(x => x.Name).FirstOrDefault();

		//	CommonViewModel.SelectListItems = new List<SelectListItem_Custom>();

		//	//var list = _context.Using<LoyaltyPointMenuAccess>().GetByCondition(x => x.LoyaltyPointId == CommonViewModel.Obj.Id).ToList();

		//	//if (list != null && list.Count() > 0)
		//	//{
		//	//	string[] selected = (from x in list
		//	//						 join y in listMenu on x.MenuId equals y.Id
		//	//						 where !y.Name.ToLower().Contains("menu") && !y.Name.ToLower().Contains("menu")
		//	//						 select Convert.ToString(x.MenuId + "_" + y.ParentId)).ToArray();

		//	//	if (selected != null && selected.Length > 0)
		//	//		CommonViewModel.Obj.CreatedDate_Text = string.Join(",", selected) + ",";
		//	//}

		//	if (CommonViewModel.SelectListItems == null) CommonViewModel.SelectListItems = new List<SelectListItem_Custom>();

		//	return PartialView("_Partial_AddEditForm", CommonViewModel);
		//}

		//[HttpPost]
		////[CustomAuthorizeAttribute(AccessType_Enum.Write)]
		//public ActionResult Save(LoyaltyPoint viewModel)
		//{
		//	try
		//	{
		//		viewModel = viewModel != null && !Common.IsSuperAdmin() && viewModel.Id == Common.LoggedUser_LoyaltyPointId() ? null : viewModel;

		//		if (viewModel != null)
		//		{
		//			#region Validation

		//			if (!Common.IsAdmin())
		//			{
		//				CommonViewModel.IsSuccess = false;
		//				CommonViewModel.StatusCode = ResponseStatusCode.Error;
		//				CommonViewModel.Message = ResponseStatusMessage.UnAuthorize;

		//				return Json(CommonViewModel);
		//			}

		//			if (string.IsNullOrEmpty(viewModel.Name))
		//			{
		//				CommonViewModel.IsSuccess = false;
		//				CommonViewModel.StatusCode = ResponseStatusCode.Error;
		//				CommonViewModel.Message = "Please enter LoyaltyPoint name.";

		//				return Json(CommonViewModel);
		//			}

		//			if (_context.Using<LoyaltyPoint>().Any(x => x.Name.ToLower().Replace(" ", "") == viewModel.Name.ToLower().Replace(" ", "") && x.Id != viewModel.Id) || viewModel.Id == 1)
		//			{
		//				CommonViewModel.IsSuccess = false;
		//				CommonViewModel.StatusCode = ResponseStatusCode.Error;
		//				CommonViewModel.Message = "LoyaltyPoint already exist. Please try another LoyaltyPoint.";

		//				return Json(CommonViewModel);
		//			}

		//			#endregion

		//			#region Database-Transaction

		//			using (var transaction = _context.BeginTransaction())
		//			{
		//				try
		//				{
		//					LoyaltyPoint obj = _context.Using<LoyaltyPoint>().GetByCondition(x => x.Id > 1 && x.Id == viewModel.Id).FirstOrDefault();

		//					if (viewModel != null && !(viewModel.DisplayOrder > 0))
		//						viewModel.DisplayOrder = (_context.Using<LoyaltyPoint>().GetAll().ToList().Max(x => x.DisplayOrder) ?? 0) + 1;

		//					if (Common.IsAdmin() && obj != null)
		//					{
		//						obj.Name = viewModel.Name;
		//						obj.DisplayOrder = viewModel.DisplayOrder;
		//						obj.IsAdmin = Common.IsSuperAdmin() ? viewModel.IsAdmin : false;
		//						obj.IsActive = viewModel.IsActive;

		//						_context.Using<LoyaltyPoint>().Update(obj);
		//						//_context.Entry(obj).State = EntityState.Modified;
		//						//_context.SaveChanges();
		//					}
		//					else if (Common.IsAdmin())
		//					{
		//						viewModel.IsAdmin = Common.IsSuperAdmin() ? viewModel.IsAdmin : false;

		//						var _viewModel = _context.Using<LoyaltyPoint>().Add(viewModel);
		//						viewModel.Id = _viewModel.Id;
		//						//_context.SaveChanges();
		//						//_context.Entry(viewModel).Reload();
		//					}

		//					try
		//					{
		//						var listLoyaltyPointMenuAccesses = _context.Using<LoyaltyPointMenuAccess>().GetByCondition(x => x.LoyaltyPointId == viewModel.Id).ToList();

		//						if (listLoyaltyPointMenuAccesses != null && listLoyaltyPointMenuAccesses.Count() > 0)
		//						{
		//							foreach (var access in listLoyaltyPointMenuAccesses)
		//							{
		//								_context.Using<LoyaltyPointMenuAccess>().Delete(access);
		//								//_context.Entry(access).State = EntityState.Deleted;
		//								//_context.SaveChanges();
		//							}
		//						}

		//						if (!string.IsNullOrEmpty(viewModel.CreatedDate_Text))
		//						{
		//							var listMenu = _context.Using<Menu>().GetAll().ToList();

		//							List<(long MenuId, long ParentMenuId)> menuPairs = viewModel.CreatedDate_Text.Split(',').Select(pair => pair.Split('_'))
		//											.Select(parts => (MenuId: long.Parse(parts[0]), ParentMenuId: long.Parse(parts[1]))).ToList();

		//							// Get all unique IDs from MenuId and ParentMenuId
		//							var allMenuIds = menuPairs.SelectMany(p => new[] { p.MenuId, p.ParentMenuId }).Distinct().ToList();

		//							// Now get actual Menu objects by matching IDs
		//							var selectedMenus = listMenu.Where(x => x.IsActive == true && allMenuIds.Contains(x.Id)).ToList();

		//							//var list = viewModel.CreatedDate_Text.Split(',');

		//							//foreach (var item in list.Where(x => !string.IsNullOrEmpty(x)))
		//							foreach (var menu in selectedMenus)
		//							{
		//								try
		//								{
		//									var LoyaltyPointMenuAccess = new LoyaltyPointMenuAccess()
		//									{
		//										//MenuId = Convert.ToInt64(item.Split('_')[0]),
		//										MenuId = menu.Id,
		//										LoyaltyPointId = viewModel.Id,
		//										IsCreate = true,
		//										IsUpdate = true,
		//										IsRead = true,
		//										IsDelete = true,
		//										IsActive = true,
		//										IsDeleted = false,
		//										IsSetDefault = true
		//									};

		//									_context.Using<LoyaltyPointMenuAccess>().Add(LoyaltyPointMenuAccess);
		//									//_context.SaveChanges();
		//								}
		//								catch (Exception ex) { continue; }
		//							}


		//							try
		//							{
		//								var listUserMenuAccesses = _context.Using<UserMenuAccess>().GetByCondition(x => x.LoyaltyPointId == viewModel.Id).ToList();

		//								foreach (var access in listUserMenuAccesses) _context.Using<UserMenuAccess>().Delete(access);
		//								//_context.Entry(access).State = EntityState.Deleted;
		//								//_context.SaveChanges();

		//								foreach (var menu in selectedMenus.OrderBy(x => x.Id).ToList())
		//								{
		//									var listLoyaltyPointUserAccess = (from x in _context.Using<UserLoyaltyPointMapping>().GetAll().ToList()
		//															  where x.LoyaltyPointId == viewModel.Id && x.UserId > 0 && x.IsActive == true && x.IsDeleted == false
		//															  select new UserMenuAccess() { UserId = x.UserId, LoyaltyPointId = viewModel.Id, MenuId = menu.Id, IsCreate = true, IsUpdate = true, IsRead = true, IsDelete = true, CreatedBy = 1 }).ToList();

		//									foreach (var item_ in listLoyaltyPointUserAccess) _context.Using<UserMenuAccess>().Add(item_);
		//									//_context.SaveChanges();
		//								}
		//							}
		//							catch (Exception ex) { }

		//						}

		//					}
		//					catch (Exception ex) { }

		//					CommonViewModel.IsConfirm = true;
		//					CommonViewModel.IsSuccess = true;
		//					CommonViewModel.StatusCode = ResponseStatusCode.Success;
		//					CommonViewModel.Message = ResponseStatusMessage.Success;
		//					CommonViewModel.RedirectURL = Url.Action("Index", "LoyaltyPoint");

		//					transaction.Commit();

		//					return Json(CommonViewModel);
		//				}
		//				catch (Exception ex) { transaction.Rollback(); }
		//			}

		//			#endregion
		//		}
		//	}
		//	catch (Exception ex) { }

		//	CommonViewModel.Message = ResponseStatusMessage.Error;
		//	CommonViewModel.IsSuccess = false;
		//	CommonViewModel.StatusCode = ResponseStatusCode.Error;

		//	return Json(CommonViewModel);
		//}

		[HttpPost]
		//[CustomAuthorizeAttribute(AccessType_Enum.Delete)]
		public ActionResult DeleteConfirmed(long Id)
		{
			try
			{
				if (!Common.IsAdmin())
				{
					CommonViewModel.IsSuccess = false;
					CommonViewModel.StatusCode = ResponseStatusCode.Error;
					CommonViewModel.Message = ResponseStatusMessage.UnAuthorize;

					return Json(CommonViewModel);
				}

				if (Common.IsAdmin() && _context.Using<LoyaltyPoint>().Any(x => x.Id == Id))
				{
					var obj = _context.Using<LoyaltyPoint>().GetByCondition(x => x.Id == Id).FirstOrDefault();

					_context.Using<LoyaltyPoint>().Update(obj);
					//_context.Entry(obj).State = EntityState.Deleted;
					//_context.SaveChanges();

					CommonViewModel.IsConfirm = true;
					CommonViewModel.IsSuccess = true;
					CommonViewModel.StatusCode = ResponseStatusCode.Success;
					CommonViewModel.Message = ResponseStatusMessage.Delete;

					CommonViewModel.RedirectURL = Url.Action("Index", "LoyaltyPoint");

					return Json(CommonViewModel);
				}
			}
			catch (Exception ex) { }

			CommonViewModel.IsSuccess = false;
			CommonViewModel.StatusCode = ResponseStatusCode.Error;
			CommonViewModel.Message = ResponseStatusMessage.Unable_Delete;

			return Json(CommonViewModel);
		}

	}
}