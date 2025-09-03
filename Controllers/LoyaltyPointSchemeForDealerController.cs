using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Seed_Admin.Infra;
using System.Data;

namespace Seed_Admin.Controllers
{
	public class LoyaltyPointSchemeForDealerController : BaseController<ResponseModel<LoyaltyPointScheme>>
	{
		public LoyaltyPointSchemeForDealerController(IRepositoryWrapper repository) : base(repository) { }

		public IActionResult Index()
		{
			try
			{
				var dt = new DataTable();

				CommonViewModel.ObjList = new List<LoyaltyPointScheme>();

				List<SqlParameter> sqlParameters = new List<SqlParameter>();
				sqlParameters.Add(new SqlParameter("@Id", SqlDbType.BigInt) { Value = 0 });

				dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_LoyaltyPointScheme_GET", sqlParameters, true);

				if (dt != null && dt.Rows.Count > 0)
				{
					foreach (DataRow dr in dt.AsEnumerable().Where(r => r.Field<string>("SchemeFor") == "DEALER"))
					{
						CommonViewModel.ObjList.Add(new LoyaltyPointScheme()
						{
							Id = dr["Id"] != DBNull.Value ? Convert.ToInt64(dr["Id"]) : 0,
							SchemeName = dr["SchemeName"] != DBNull.Value ? Convert.ToString(dr["SchemeName"]) : "",
							ProductID = dr["ProductID"] != DBNull.Value ? Convert.ToInt64(dr["ProductID"]) : 0,
							PackageType_ID = dr["PackageType_ID"] != DBNull.Value ? Convert.ToInt64(dr["PackageType_ID"]) : 0,
							SKUSize_ID = dr["SKUSize_ID"] != DBNull.Value ? Convert.ToInt64(dr["SKUSize_ID"]) : 0,
							//MinPurchaseQty = dr["MinPurchaseQty"] != DBNull.Value ? Convert.ToInt32(dr["MinPurchaseQty"]) : 0,
							MaxPurchaseQty = dr["MaxPurchaseQty"] != DBNull.Value ? Convert.ToInt32(dr["MaxPurchaseQty"]) : 0,
							LoyaltyPoints = dr["LoyaltyPoints"] != DBNull.Value ? Convert.ToInt32(dr["LoyaltyPoints"]) : 0,
							EffectiveStartDate = dr["EffectiveStartDate"] != DBNull.Value ? Convert.ToDateTime(dr["EffectiveStartDate"]) : nullDateTime,
							EffectiveEndDate = dr["EffectiveEndDate"] != DBNull.Value ? Convert.ToDateTime(dr["EffectiveEndDate"]) : nullDateTime,
							SchemeFor = dr["SchemeFor"] != DBNull.Value ? Convert.ToString(dr["SchemeFor"]) : "",
							SchemeFor_Text = dr["SchemeFor_Text"] != DBNull.Value ? Convert.ToString(dr["SchemeFor_Text"]) : "",
							Product_Name = dr["Product_Name"] != DBNull.Value ? Convert.ToString(dr["Product_Name"]) : "",
							PackageType_Name = dr["PackageType_Name"] != DBNull.Value ? Convert.ToString(dr["PackageType_Name"]) : "",
							SKUSize_Name = dr["SKUSize_Name"] != DBNull.Value ? Convert.ToString(dr["SKUSize_Name"]) : "",

						});
					}
				}
			}
			catch (Exception ex) { LogService.LogInsert(GetCurrentAction(), "", ex); }

			return View(CommonViewModel);
		}

		[HttpGet]
		public IActionResult Partial_AddEditForm(long Id = 0)
		{
			var obj = new LoyaltyPointScheme();

			var dt = new DataTable();

			var list = new List<SelectListItem_Custom>();

			try
			{

				List<SqlParameter> sqlParameters = new List<SqlParameter>();
				if (Id > 0)
				{
					sqlParameters.Add(new SqlParameter("@Id", SqlDbType.BigInt) { Value = Id });

					dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_LoyaltyPointScheme_GET", sqlParameters, true);

					if (dt != null && dt.Rows.Count > 0) dt = dt.AsEnumerable().Where(r => r.Field<string>("SchemeFor") == "DEALER").CopyToDataTable();

					if (dt != null && dt.Rows.Count > 0)
					{
						obj = new LoyaltyPointScheme()
						{
							Id = dt.Rows[0]["Id"] != DBNull.Value ? Convert.ToInt64(dt.Rows[0]["Id"]) : 0,
							SchemeName = dt.Rows[0]["SchemeName"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["SchemeName"]) : "",
							ProductID = dt.Rows[0]["ProductID"] != DBNull.Value ? Convert.ToInt64(dt.Rows[0]["ProductID"]) : 0,
							PackageType_ID = dt.Rows[0]["PackageType_ID"] != DBNull.Value ? Convert.ToInt64(dt.Rows[0]["PackageType_ID"]) : 0,
							SKUSize_ID = dt.Rows[0]["SKUSize_ID"] != DBNull.Value ? Convert.ToInt64(dt.Rows[0]["SKUSize_ID"]) : 0,
							//MinPurchaseQty = dt.Rows[0]["MinPurchaseQty"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["MinPurchaseQty"]) : 0,
							MaxPurchaseQty = dt.Rows[0]["MaxPurchaseQty"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["MaxPurchaseQty"]) : 0,
							LoyaltyPoints = dt.Rows[0]["LoyaltyPoints"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["LoyaltyPoints"]) : 0,
							EffectiveStartDate = dt.Rows[0]["EffectiveStartDate"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["EffectiveStartDate"]) : nullDateTime,
							EffectiveEndDate = dt.Rows[0]["EffectiveEndDate"] != DBNull.Value ? Convert.ToDateTime(dt.Rows[0]["EffectiveEndDate"]) : nullDateTime,
							SchemeFor = dt.Rows[0]["SchemeFor"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["SchemeFor"]) : "",
							SchemeFor_Text = dt.Rows[0]["SchemeFor_Text"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["SchemeFor_Text"]) : "",
							Product_Name = dt.Rows[0]["Product_Name"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["Product_Name"]) : "",
							PackageType_Name = dt.Rows[0]["PackageType_Name"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["PackageType_Name"]) : "",
							SKUSize_Name = dt.Rows[0]["SKUSize_Name"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["SKUSize_Name"]) : "",

						};
					}
					sqlParameters = new List<SqlParameter>();
					sqlParameters.Add(new SqlParameter("@Product", SqlDbType.VarChar) { Value = obj.Product_Name });
					dt = new DataTable();
					dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_PackageType_Combo", sqlParameters, true);

					if (dt != null && dt.Rows.Count > 0)
					{
						foreach (DataRow drpkg in dt.Rows)
						{
							list.Add(new SelectListItem_Custom(Convert.ToString(drpkg["Id"]), Convert.ToString(drpkg["PackageTypeName"]), "PackageType")
							{
								Value = drpkg["Id"] != DBNull.Value ? Convert.ToString(drpkg["Id"]) : "",
								Text = drpkg["PackageTypeName"] != DBNull.Value ? Convert.ToString(drpkg["PackageTypeName"]) : ""
							});
						}

					}

					sqlParameters = new List<SqlParameter>();
					sqlParameters.Add(new SqlParameter("@PackageType_ID", SqlDbType.BigInt) { Value = obj.PackageType_ID });
					dt = new DataTable();
					dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_SKUSize_Combo", sqlParameters, true);

					if (dt != null && dt.Rows.Count > 0)
					{
						foreach (DataRow drpkg in dt.Rows)
						{
							list.Add(new SelectListItem_Custom(Convert.ToString(drpkg["Id"]), Convert.ToString(drpkg["SKUSizeName"]), "SKUSize")
							{
								Value = drpkg["Id"] != DBNull.Value ? Convert.ToString(drpkg["Id"]) : "",
								Text = drpkg["SKUSizeName"] != DBNull.Value ? Convert.ToString(drpkg["SKUSizeName"]) : ""
							});
						}

					}


				}




				dt = new DataTable();
				dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_Product_Combo", null, true);

				if (dt != null && dt.Rows.Count > 0)
				{
					foreach (DataRow dr in dt.Rows)
					{
						list.Add(new SelectListItem_Custom(Convert.ToString(dr["Id"]), Convert.ToString(dr["Name"]), "Product")
						{
							Value = dr["Id"] != DBNull.Value ? Convert.ToString(dr["Id"]) : "",
							Text = dr["Name"] != DBNull.Value ? Convert.ToString(dr["Name"]) : ""
						});
					}

				}



			}
			catch (Exception ex) { LogService.LogInsert(GetCurrentAction(), "", ex); }

			CommonViewModel.SelectListItems = list;
			CommonViewModel.Obj = obj;

			return PartialView("_Partial_AddEditForm", CommonViewModel);
		}
		[HttpPost]
		public JsonResult Save(LoyaltyPointScheme viewModel)
		{
			try
			{

				if (string.IsNullOrEmpty(viewModel.SchemeName))
				{
					CommonViewModel.IsSuccess = false;
					CommonViewModel.StatusCode = ResponseStatusCode.Error;
					CommonViewModel.Message = "Please enter Scheme Name.";

					return Json(CommonViewModel);
				}

				if (viewModel.ProductID == 0)
				{
					CommonViewModel.IsSuccess = false;
					CommonViewModel.StatusCode = ResponseStatusCode.Error;
					CommonViewModel.Message = "Please select Product.";

					return Json(CommonViewModel);
				}

				if (viewModel.MaxPurchaseQty == 0)
				{
					CommonViewModel.IsSuccess = false;
					CommonViewModel.StatusCode = ResponseStatusCode.Error;
					CommonViewModel.Message = "Please enter quanity .";

					return Json(CommonViewModel);
				}
				if (viewModel.LoyaltyPoints == 0)
				{
					CommonViewModel.IsSuccess = false;
					CommonViewModel.StatusCode = ResponseStatusCode.Error;
					CommonViewModel.Message = "Please enter loyalty points .";

					return Json(CommonViewModel);
				}
				if (viewModel.EffectiveStartDate == null && viewModel.EffectiveEndDate == null)
				{
					CommonViewModel.IsSuccess = false;
					CommonViewModel.StatusCode = ResponseStatusCode.Error;
					CommonViewModel.Message = "Please select atleast one date.";

					return Json(CommonViewModel);
				}


				var (IsSuccess, response, Id) = (false, ResponseStatusMessage.Error, 0M);



				List<SqlParameter> oParams = new List<SqlParameter>();

				oParams.Add(new SqlParameter("@Id", SqlDbType.BigInt) { Value = viewModel.Id });
				oParams.Add(new SqlParameter("@SchemeName", SqlDbType.VarChar) { Value = viewModel.SchemeName });
				oParams.Add(new SqlParameter("@ProductID", SqlDbType.BigInt) { Value = viewModel.ProductID });
				oParams.Add(new SqlParameter("@PackageType_ID", SqlDbType.BigInt) { Value = viewModel.PackageType_ID });
				oParams.Add(new SqlParameter("@SKUSize_ID", SqlDbType.BigInt) { Value = viewModel.SKUSize_ID });
				oParams.Add(new SqlParameter("@MinPurchaseQty", SqlDbType.Int) { Value = null });
				oParams.Add(new SqlParameter("@MaxPurchaseQty", SqlDbType.Int) { Value = viewModel.MaxPurchaseQty });
				oParams.Add(new SqlParameter("@LoyaltyPoints", SqlDbType.Int) { Value = viewModel.LoyaltyPoints });
				oParams.Add(new SqlParameter("@EffectiveStartDate", SqlDbType.DateTime) { Value = viewModel.EffectiveStartDate ?? null });
				oParams.Add(new SqlParameter("@EffectiveEndDate", SqlDbType.DateTime) { Value = viewModel.EffectiveEndDate ?? null });
				oParams.Add(new SqlParameter("@SchemeFor", SqlDbType.VarChar) { Value = "DEALER" });
				oParams.Add(new SqlParameter("@Operated_By", SqlDbType.BigInt) { Value = AppHttpContextAccessor.GetSession(SessionKey.KEY_USER_ID) });
				oParams.Add(new SqlParameter("@Action", SqlDbType.VarChar) { Value = viewModel.Id == 0 ? "INSERT" : "UPDATE" });

				(IsSuccess, response, Id) = DataContext_Command.ExecuteStoredProcedure("SP_LoyaltyPointScheme_SAVE", oParams, true);

				CommonViewModel.IsConfirm = true;
				CommonViewModel.IsSuccess = IsSuccess;
				CommonViewModel.StatusCode = IsSuccess ? ResponseStatusCode.Success : ResponseStatusCode.Error;
				CommonViewModel.Message = response;

				CommonViewModel.RedirectURL = IsSuccess ? Url.Content("~/") + GetCurrentControllerUrl() + "/Index" : "";

				return Json(CommonViewModel);
			}
			catch (Exception ex)
			{
				LogService.LogInsert(GetCurrentAction(), "", ex);

				CommonViewModel.IsSuccess = false;
				CommonViewModel.StatusCode = ResponseStatusCode.Error;
				CommonViewModel.Message = ResponseStatusMessage.Error + " | " + ex.Message;
			}
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
							list.Add(new SelectListItem_Custom(Convert.ToString(dr["Id"]), Convert.ToString(dr["PackageTypeName"]), "PACKTYPE"));
				}
				else if (!string.IsNullOrEmpty(Type) && Type == "SKUSIZE")
				{
					oParams = new List<SqlParameter>();
					oParams.Add(new SqlParameter("@PackageType_ID", SqlDbType.BigInt) { Value = ParentId });

					dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_SKUSize_Combo", oParams, true);

					if (dt != null && dt.Rows.Count > 0)
						foreach (DataRow dr in dt.Rows)
							list.Add(new SelectListItem_Custom(Convert.ToString(dr["Id"]), Convert.ToString(dr["SKUSizeName"]), "SKUSIZE"));
				}

			}
			catch (Exception ex) { LogService.LogInsert(GetCurrentAction(), "", ex); }

			return Json(list);
		}
	}
}
