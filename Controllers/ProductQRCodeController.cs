using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using Seed_Admin.Controllers;
using Seed_Admin.Infra;
using Seed_Admin.Models;
using System.Collections.Generic;
using System.Data;

namespace Seed_Admin.Areas.Admin.Controllers
{

	public class ProductQRCodeController : BaseController<ResponseModel<ProductQrCode>>
	{
		public ProductQRCodeController(IRepositoryWrapper repository) : base(repository) { }

		// GET: Admin/LoyaltyPoint
		public ActionResult Index()
		{
			return View(CommonViewModel);
		}

		[HttpGet]
		public IActionResult GetData(JqueryDatatableParam param)
		{
			var result = new PagedResult();

			if (Common.IsAdmin())
				try
				{
					long batchId = Convert.ToInt64((string)HttpContext.Request.Query["BatchId"] ?? "0");

					var listQrcode = _context.Using<ProductQrCode>().GetByCondition(x => batchId > 0 ? x.BatchId == batchId : true).ToList();

					var dictProduct = _context.Using<Product>().GetAll().ToDictionary(x => x.Id);
					var dictPackageType = _context.Using<PackageType>().GetAll().ToDictionary(x => x.Id);
					var dictSKUSize = _context.Using<SKUSize>().GetAll().ToDictionary(x => x.Id);

					var listQrcode_Filtered = listQrcode.GroupBy(x => new { x.BatchId, x.RequestId, x.ProductId, x.PackageType_ID, x.SKUSize_ID, x.CreatedDate })
									.Select(g => new ProductQrCode()
									{
										BatchId = g.Key.BatchId,
										RequestId = g.Key.RequestId,
										Points = g.Count(),
										//Items = g.ToList()
										QrCode = (g.ToList().OrderBy(x => x.DecimalValue).FirstOrDefault()?.QrCode ?? "") + " - " + (g.ToList().OrderBy(x => x.DecimalValue).LastOrDefault()?.QrCode ?? ""),
										ProductId = g.Key.ProductId,
										PackageType_ID = g.Key.PackageType_ID,
										SKUSize_ID = g.Key.SKUSize_ID,
										CreatedDate = g.Key.CreatedDate
									}).ToList();

					var list = listQrcode_Filtered.Select(x =>
					{
						dictProduct.TryGetValue(x.ProductId, out var lp);
						dictPackageType.TryGetValue(x.PackageType_ID, out var lpt);
						dictSKUSize.TryGetValue(x.SKUSize_ID, out var ls);
						return new ProductQrCode
						{
							Id = x.Id,
							BatchId = x.BatchId,
							RequestId = x.RequestId,
							//QrCode_Base64 = x.QrCode_Base64,
							QrCode = x.QrCode,
							Points = x.Points,
							IsScanned = x.IsScanned,

							ProductId = lp?.Id ?? 0,
							Product_Text = lp?.Name ?? "",
							PackageType_ID = lpt?.Id ?? 0,
							PackageType_Text = lpt?.PackageTypeName ?? "",
							SKUSize_ID = ls?.Id ?? 0,
							SKUSize_Text = ls?.SKUSizeName ?? "",
							CreatedDate_Text = x.CreatedDate?.ToString("dd/MM/yyyy HH:mm:ss").Replace("-", "/") ?? "",
							//LastModifiedDate_Text = x.LastModifiedDate?.ToString("dd/MM/yyyy HH:mm:ss").Replace("-", "/") ?? "",
							CreatedDate_Ticks = x.CreatedDate?.Ticks ?? 0,
							//LastModifiedDate_Ticks = x.LastModifiedDate?.Ticks ?? 0
						};
					}).ToList();

					var recordsTotal = list.Count();
					IEnumerable<ProductQrCode> query = list;

					// Filter (Search)
					if (!string.IsNullOrWhiteSpace(param.sSearch))
					{
						query = query.Where(x =>
							(x.QrCode?.Contains(param.sSearch, StringComparison.OrdinalIgnoreCase) ?? false) ||
							(x.Product_Text?.Contains(param.sSearch, StringComparison.OrdinalIgnoreCase) ?? false) ||
							(x.PackageType_Text?.Contains(param.sSearch, StringComparison.OrdinalIgnoreCase) ?? false) ||
							(x.SKUSize_Text?.Contains(param.sSearch, StringComparison.OrdinalIgnoreCase) ?? false) ||
							x.Points.ToString().Contains(param.sSearch) ||
							x.CreatedDate_Text.Contains(param.sSearch)
						);
					}

					// Sort
					string sortColumn = HttpContext.Request.Query.ContainsKey("iSortCol_0") && HttpContext.Request.Query.ContainsKey($"mDataProp_{HttpContext.Request.Query["iSortCol_0"]}") ? Convert.ToString(HttpContext.Request.Query[$"mDataProp_{HttpContext.Request.Query["iSortCol_0"]}"]) : "";

					query = sortColumn?.ToLower() switch
					{
						"requestid" => Convert.ToString(HttpContext.Request.Query["sSortDir_0"]).ToLower() == "asc" ? query.OrderBy(x => x.RequestId) : query.OrderByDescending(x => x.RequestId),
						"qrcode" => Convert.ToString(HttpContext.Request.Query["sSortDir_0"]).ToLower() == "asc" ? query.OrderBy(x => x.QrCode) : query.OrderByDescending(x => x.QrCode),
						"points" => Convert.ToString(HttpContext.Request.Query["sSortDir_0"]).ToLower() == "asc" ? query.OrderBy(x => x.Points) : query.OrderByDescending(x => x.Points),
						"generatedate_text" => Convert.ToString(HttpContext.Request.Query["sSortDir_0"]).ToLower() == "asc" ? query.OrderBy(x => x.CreatedDate?.Ticks) : query.OrderByDescending(x => x.CreatedDate?.Ticks),
						"lastmodifieddate_text" => Convert.ToString(HttpContext.Request.Query["sSortDir_0"]).ToLower() == "asc" ? query.OrderBy(x => x.LastModifiedDate?.Ticks) : query.OrderByDescending(x => x.LastModifiedDate?.Ticks),

						_ => query.OrderByDescending(x => batchId > 0 ? x.LastModifiedDate?.Ticks : x.RequestId)
					};

					// Pagination
					var pagedData = param.iDisplayLength > -1 ? query.Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList() : query.ToList();

					return Json(new { param.sEcho, iTotalRecords = pagedData.Count(), iTotalDisplayRecords = recordsTotal, aaData = pagedData }, new System.Text.Json.JsonSerializerOptions());
				}
				catch (Exception ex) { }

			return Json(new { param.sEcho, iTotalRecords = 0, iTotalDisplayRecords = 0, aaData = new JArray() }, new System.Text.Json.JsonSerializerOptions());
		}

		//[CustomAuthorizeAttribute(AccessType_Enum.Read)]
		public ActionResult Partial_AddEditForm()
		{
			if (!Common.IsAdmin())
			{
				CommonViewModel.IsSuccess = false;
				CommonViewModel.StatusCode = ResponseStatusCode.Error;
				CommonViewModel.Message = ResponseStatusMessage.UnAuthorize;

				return Json(CommonViewModel);
			}



			var dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_Product_Combo", null, true);

			if (dt != null && dt.Rows.Count > 0)
				foreach (DataRow dr in dt.Rows)
					CommonViewModel.SelectListItems.Add(new SelectListItem_Custom(dr["Id"] != DBNull.Value ? Convert.ToString(dr["Id"]) : "0", dr["Name"] != DBNull.Value ? Convert.ToString(dr["Name"]) : "", "Product"));

			return PartialView("_Partial_AddEditForm", CommonViewModel);
		}

		[HttpPost]
		//[CustomAuthorizeAttribute(AccessType_Enum.Write)]
		public ActionResult Save(long ProductId, long PackageType_ID, long SKUSize_ID, int NoOfQRCode, int MinPointRange, int MaxPointRange)
		{
			try
			{
				#region Validation

				if (!Common.IsAdmin())
				{
					CommonViewModel.IsSuccess = false;
					CommonViewModel.StatusCode = ResponseStatusCode.Error;
					CommonViewModel.Message = ResponseStatusMessage.UnAuthorize;

					return Json(CommonViewModel);
				}
				if (ProductId <= 0)
				{
					CommonViewModel.IsSuccess = false;
					CommonViewModel.StatusCode = ResponseStatusCode.Error;
					CommonViewModel.Message = "Please select Product.";

					return Json(CommonViewModel);
				}
				if (PackageType_ID <= 0)
				{
					CommonViewModel.IsSuccess = false;
					CommonViewModel.StatusCode = ResponseStatusCode.Error;
					CommonViewModel.Message = "Please select Package type.";

					return Json(CommonViewModel);
				}
				if (SKUSize_ID <= 0)
				{
					CommonViewModel.IsSuccess = false;
					CommonViewModel.StatusCode = ResponseStatusCode.Error;
					CommonViewModel.Message = "Please select SKU Size.";

					return Json(CommonViewModel);
				}


				if (NoOfQRCode <= 0)
				{
					CommonViewModel.IsSuccess = false;
					CommonViewModel.StatusCode = ResponseStatusCode.Error;
					CommonViewModel.Message = "Please enter No. Of QR Code.";

					return Json(CommonViewModel);
				}

				#endregion

				var (IsSuccess, response, Id) = (false, ResponseStatusMessage.Error, 0M);

				List<SqlParameter> oParams = new List<SqlParameter>();

				oParams.Add(new SqlParameter("@ProductId", SqlDbType.BigInt) { Value = ProductId });
				oParams.Add(new SqlParameter("@PackageType_ID", SqlDbType.BigInt) { Value = PackageType_ID });
				oParams.Add(new SqlParameter("@SKUSize_ID", SqlDbType.BigInt) { Value = SKUSize_ID });
				oParams.Add(new SqlParameter("@NoOfQRCode", SqlDbType.Int) { Value = NoOfQRCode });
				oParams.Add(new SqlParameter("@MinValue", SqlDbType.Int) { Value = MinPointRange });
				oParams.Add(new SqlParameter("@MaxValue", SqlDbType.Int) { Value = MaxPointRange });

				oParams.Add(new SqlParameter("@Operated_By", SqlDbType.BigInt) { Value = AppHttpContextAccessor.GetSession(SessionKey.KEY_USER_ID) });

				(IsSuccess, response, Id) = DataContext_Command.ExecuteStoredProcedure("SP_Product_QR_Code_Generate", oParams, true);

				CommonViewModel.IsConfirm = true;
				CommonViewModel.IsSuccess = IsSuccess;
				CommonViewModel.StatusCode = IsSuccess ? ResponseStatusCode.Success : ResponseStatusCode.Error;
				CommonViewModel.Message = response;

				return Json(CommonViewModel);

			}
			catch (Exception ex) { }

			CommonViewModel.Message = ResponseStatusMessage.Error;
			CommonViewModel.IsSuccess = false;
			CommonViewModel.StatusCode = ResponseStatusCode.Error;

			return Json(CommonViewModel);
		}

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

				if (Common.IsAdmin() && _context.Using<LoyaltyPointsQrcode>().Any(x => x.Id == Id && x.IsScanned == false))
				{
					var obj = _context.Using<LoyaltyPointsQrcode>().GetByCondition(x => x.Id == Id && x.IsScanned == false).FirstOrDefault();

					_context.Using<LoyaltyPointsQrcode>().Update(obj);
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
		[HttpGet]
		public IActionResult GetProductDetails(string Type = "", string Product = "", long ParentId = 0)
		{
			var list = new List<SelectListItem_Custom>();

			List<SqlParameter> oParams = new List<SqlParameter>();

			var dt = new DataTable();

			try
			{
				if (string.IsNullOrEmpty(Type))
				{
					dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_Product_Combo", null, true);

					if (dt != null && dt.Rows.Count > 0)
						foreach (DataRow dr in dt.Rows)
							list.Add(new SelectListItem_Custom(Convert.ToString(dr["Id"]), Convert.ToString(dr["Name"]), "Product"));
				}
				else if (!string.IsNullOrEmpty(Type) && Type == "PACKTYPE")
				{
					oParams = new List<SqlParameter>();
					oParams.Add(new SqlParameter("@Product", SqlDbType.VarChar) { Value = Product });

					dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_PackageType_Combo", oParams, true);

					if (dt != null && dt.Rows.Count > 0)
						foreach (DataRow dr in dt.Rows)
							list.Add(new SelectListItem_Custom(Convert.ToString(dr["Id"]), Convert.ToString(dr["PackageTypeName"]), "PackageType"));
				}
				else if (!string.IsNullOrEmpty(Type) && Type == "SKUSIZE")
				{
					oParams = new List<SqlParameter>();
					oParams.Add(new SqlParameter("@PackageType_ID", SqlDbType.BigInt) { Value = ParentId });

					dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_SKUSize_Combo", oParams, true);

					if (dt != null && dt.Rows.Count > 0)
						foreach (DataRow dr in dt.Rows)
							list.Add(new SelectListItem_Custom(Convert.ToString(dr["Id"]), Convert.ToString(dr["SKUSizeName"]), "SKUSize"));
				}

			}
			catch (Exception ex) { LogService.LogInsert(GetCurrentAction(), "", ex); }

			return Json(list);
		}

	}
}