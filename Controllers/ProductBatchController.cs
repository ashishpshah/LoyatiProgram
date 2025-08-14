using Seed_Admin.Controllers;
using Seed_Admin.Infra;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Seed_Admin.Areas.Admin.Controllers
{

	public class ProductBatchController : BaseController<ResponseModel<ProductBatch>>
	{
		public ProductBatchController(IRepositoryWrapper repository) : base(repository) { }

		// GET: Admin/ProductBatch
		public ActionResult Index()
		{
			if (Common.IsSuperAdmin() || Common.IsAdmin()) CommonViewModel.ObjList = _context.Using<ProductBatch>().GetAll().ToList();

			for (int i = 0; i < CommonViewModel.ObjList.Count(); i++)
			{
				CommonViewModel.ObjList[i].MfgDate_Text = CommonViewModel.ObjList[i].MfgDate.ToString(Common.DateTimeFormat_ddMMyyyy_HHmm);
				CommonViewModel.ObjList[i].ExpiryDate_Text = CommonViewModel.ObjList[i].ExpiryDate.ToString(Common.DateTimeFormat_ddMMyyyy_HHmm);
				CommonViewModel.ObjList[i].CreatedDate_Text = CommonViewModel.ObjList[i].CreatedDate?.ToString(Common.DateTimeFormat_ddMMyyyy_HHmm);
			}

			return View(CommonViewModel);
		}

		//[CustomAuthorizeAttribute(AccessType_Enum.Read)]
		public ActionResult Partial_AddEditForm(long Id = 0, bool IsProductQRView = false)
		{
			if (!(Common.IsSuperAdmin() || Common.IsAdmin()))
			{
				CommonViewModel.IsSuccess = false;
				CommonViewModel.StatusCode = ResponseStatusCode.Error;
				CommonViewModel.Message = ResponseStatusMessage.UnAuthorize;

				return Json(CommonViewModel);
			}

			long currentCount = _context.Using<ProductBatch>().GetByCondition(x => x.CreatedDate.Value.Year == DateTime.Now.Year).Count();

			CommonViewModel.Obj = new ProductBatch()
			{
				BatchNo = DateTime.Now.ToString("yyyyMMdd") + (currentCount + 1).ToString().PadLeft(10, '0'),
				MfgDate = DateTime.Now.Date,
				ExpiryDate = DateTime.Now.Date.AddDays(365).AddMinutes(-1)
			};

			if (Id > 0) CommonViewModel.Obj = _context.Using<ProductBatch>().GetByCondition(x => x.Id == Id).FirstOrDefault();

			CommonViewModel.Obj.MfgDate_Text = CommonViewModel.Obj.MfgDate.ToString(Common.DateTimeFormat_ddMMyyyy).Replace("-","/");
			CommonViewModel.Obj.ExpiryDate_Text = CommonViewModel.Obj.ExpiryDate.ToString(Common.DateTimeFormat_ddMMyyyy).Replace("-", "/");
			CommonViewModel.Obj.CreatedDate_Text = CommonViewModel.Obj.CreatedDate?.ToString(Common.DateTimeFormat_ddMMyyyy_HHmm).Replace("-", "/");

			List<SqlParameter> sqlParameters = new List<SqlParameter>();
			sqlParameters.Add(new SqlParameter("@Id", SqlDbType.BigInt) { Value = 0 });

			var dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_Product_GET", sqlParameters, true);

			if (dt != null && dt.Rows.Count > 0)
				foreach (DataRow dr in dt.Rows)
					CommonViewModel.SelectListItems.Add(new SelectListItem_Custom(dr["Id"] != DBNull.Value ? Convert.ToString(dr["Id"]) : "0", dr["Name"] != DBNull.Value ? Convert.ToString(dr["Name"]) : ""));

			return PartialView("_Partial_AddEditForm", CommonViewModel);
		}

		[HttpPost]
		//[CustomAuthorizeAttribute(AccessType_Enum.Write)]
		public ActionResult Save(ProductBatch viewModel)
		{
			if (viewModel != null)
			{
				#region Validation

				if (!(Common.IsSuperAdmin() || Common.IsAdmin()))
				{
					CommonViewModel.IsSuccess = false;
					CommonViewModel.StatusCode = ResponseStatusCode.Error;
					CommonViewModel.Message = ResponseStatusMessage.UnAuthorize;

					return Json(CommonViewModel);
				}

				if (string.IsNullOrEmpty(viewModel.BatchNo))
				{
					CommonViewModel.IsSuccess = false;
					CommonViewModel.StatusCode = ResponseStatusCode.Error;
					CommonViewModel.Message = "Please enter Batch No.";

					return Json(CommonViewModel);
				}

				if (viewModel.ProductId <= 0)
				{
					CommonViewModel.IsSuccess = false;
					CommonViewModel.StatusCode = ResponseStatusCode.Error;
					CommonViewModel.Message = "Please select Product.";

					return Json(CommonViewModel);
				}

				if (string.IsNullOrEmpty(viewModel.MfgDate_Text))
				{
					CommonViewModel.IsSuccess = false;
					CommonViewModel.StatusCode = ResponseStatusCode.Error;
					CommonViewModel.Message = "Please enter Manufacture Date.";

					return Json(CommonViewModel);
				}

				if (string.IsNullOrEmpty(viewModel.ExpiryDate_Text))
				{
					CommonViewModel.IsSuccess = false;
					CommonViewModel.StatusCode = ResponseStatusCode.Error;
					CommonViewModel.Message = "Please enter Expiry Date.";

					return Json(CommonViewModel);
				}

				if (!string.IsNullOrEmpty(viewModel.MfgDate_Text)) { try { viewModel.MfgDate = DateTime.ParseExact(viewModel.MfgDate_Text.Replace("-", "/"), Common.DateTimeFormat_ddMMyyyy, CultureInfo.InvariantCulture); } catch { } }

				if (!string.IsNullOrEmpty(viewModel.ExpiryDate_Text)) { try { viewModel.ExpiryDate = DateTime.ParseExact(viewModel.ExpiryDate_Text.Replace("-", "/"), Common.DateTimeFormat_ddMMyyyy, CultureInfo.InvariantCulture); } catch { } }

				#endregion

				#region Database-Transaction

				using (var transaction = _context.BeginTransaction())
				{
					try
					{
						ProductBatch obj = _context.Using<ProductBatch>().GetByCondition(x => x.Id == viewModel.Id).FirstOrDefault();

						if (obj != null)
						{
							obj.IsActive = true;
							obj.IsDeleted = obj.IsActive ? false : obj.IsDeleted;

							_context.Using<ProductBatch>().Update(obj);
						}
						else
						{
							long currentCount = _context.Using<ProductBatch>().GetByCondition(x => x.CreatedDate.Value.Year == DateTime.Now.Year).Count();

							viewModel.BatchNo = DateTime.Now.ToString("yyyyMMdd") + (currentCount + 1).ToString().PadLeft(10, '0');

							var _viewModel = _context.Using<ProductBatch>().Add(viewModel);
							viewModel.Id = _viewModel.Id;
						}

						CommonViewModel.IsConfirm = true;
						CommonViewModel.IsSuccess = true;
						CommonViewModel.StatusCode = ResponseStatusCode.Success;
						CommonViewModel.Message = ResponseStatusMessage.Success;
						CommonViewModel.Data1 = viewModel.Id;

						transaction.Commit();

						return Json(CommonViewModel);
					}
					catch (Exception ex) { transaction.Rollback(); }
				}

				#endregion
			}

			CommonViewModel.IsSuccess = false;
			CommonViewModel.Message = ResponseStatusMessage.Error;
			CommonViewModel.StatusCode = ResponseStatusCode.Error;

			return Json(CommonViewModel);
		}

		[HttpPost]
		//[CustomAuthorizeAttribute(AccessType_Enum.Delete)]
		public ActionResult DeleteConfirmed(long Id)
		{
			try
			{
				if (!(Common.IsSuperAdmin() || Common.IsAdmin()))
				{
					CommonViewModel.IsSuccess = false;
					CommonViewModel.StatusCode = ResponseStatusCode.Error;
					CommonViewModel.Message = ResponseStatusMessage.UnAuthorize;

					return Json(CommonViewModel);
				}
				else if (_context.Using<ProductBatch>().Any(x => x.Id == Id))
				{
					var obj = _context.Using<ProductBatch>().GetByCondition(x => x.Id == Id).FirstOrDefault();
					obj.IsActive = false;
					obj.IsDeleted = true;
					_context.Using<ProductBatch>().Update(obj);

					CommonViewModel.IsConfirm = true;
					CommonViewModel.IsSuccess = true;
					CommonViewModel.StatusCode = ResponseStatusCode.Success;
					CommonViewModel.Message = ResponseStatusMessage.Delete;

					CommonViewModel.RedirectURL = Url.Action("Index", "ProductBatch");

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