using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using Seed_Admin.Infra;
using Seed_Admin.Models;
using System.Collections.Generic;
using System.Data;

namespace Seed_Admin.Controllers
{
	public class LoadingController : BaseController<ResponseModel<Loading>>
	{
		public LoadingController(IRepositoryWrapper repository) : base(repository) { }
		public IActionResult Index()
		{
			CommonViewModel.Obj = new Loading();
			var dt = new DataTable();
			var list = new List<SelectListItem_Custom>();
			dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_Dealer_Combo", null, true);

			if (dt != null && dt.Rows.Count > 0)
			{
				foreach (DataRow dr in dt.Rows)
				{
					list.Add(new SelectListItem_Custom(Convert.ToString(dr["Id"]), Convert.ToString(dr["Dealer_Name"]), "DEALER")
					{
						Value = dr["Id"] != DBNull.Value ? Convert.ToString(dr["Id"]) : "",
						Text = dr["Dealer_Name"] != DBNull.Value ? Convert.ToString(dr["Dealer_Name"]) : ""
					});
				}

			}
			CommonViewModel.SelectListItems = list;
			return View(CommonViewModel);
		}
		[HttpGet]
		public IActionResult GetOrderList(long Id = 0)
		{
			var list = new List<SelectListItem_Custom>();

			List<SqlParameter> oParams = new List<SqlParameter>();

			var dt = new DataTable();

			try
			{

				oParams = new List<SqlParameter>();
				oParams.Add(new SqlParameter("@User_Id", SqlDbType.VarChar) { Value = Id });

				dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_ORDER_Combo", oParams, true);

				if (dt != null && dt.Rows.Count > 0)
					foreach (DataRow dr in dt.Rows)
						list.Add(new SelectListItem_Custom(Convert.ToString(dr["Id"]), Convert.ToString(dr["Order_No"]), "ORDER"));


			}
			catch (Exception ex) { LogService.LogInsert(GetCurrentAction(), "", ex); }

			return Json(list);
		}

		[HttpGet]
		public IActionResult Partial_AddEditForm(long User_ID = 0, long Order_ID = 0)
		{
			if (User_ID <= 0)
			{
				CommonViewModel.IsSuccess = false;
				CommonViewModel.StatusCode = ResponseStatusCode.Error;
				CommonViewModel.Message = "Please select Dealer";

				return Json(CommonViewModel);
			}
			if (Order_ID <= 0)
			{
				CommonViewModel.IsSuccess = false;
				CommonViewModel.StatusCode = ResponseStatusCode.Error;
				CommonViewModel.Message = "Please select order";

				return Json(CommonViewModel);
			}
			CommonViewModel.ObjList = new List<Loading>();

			var dt = new DataTable();



			try
			{

				List<SqlParameter> sqlParameters = new List<SqlParameter>();

				sqlParameters.Add(new SqlParameter("@Order_ID", SqlDbType.BigInt) { Value = Order_ID });
				sqlParameters.Add(new SqlParameter("@Product_ID", SqlDbType.BigInt) { Value = 0 });

				dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_OrderDetail_GET", sqlParameters, true);

				if (dt != null && dt.Rows.Count > 0)
				{
					foreach (DataRow dr in dt.Rows)
					{
						CommonViewModel.ObjList.Add(new Loading()
						{
							Id = dr["Id"] != DBNull.Value ? Convert.ToInt64(dr["Id"]) : 0,
							Order_ID = dr["Order_ID"] != DBNull.Value ? Convert.ToInt64(dr["Order_ID"]) : 0,
							Order_No = dr["Order_No"] != DBNull.Value ? Convert.ToString(dr["Order_No"]) : "",
							Product_ID = dr["Product_ID"] != DBNull.Value ? Convert.ToInt64(dr["Product_ID"]) : 0,
							PackageType_ID = dr["PackageType_ID"] != DBNull.Value ? Convert.ToInt64(dr["PackageType_ID"]) : 0,
							SKUSize_ID = dr["SKUSize_ID"] != DBNull.Value ? Convert.ToInt64(dr["SKUSize_ID"]) : 0,
							Qty = dr["Qty"] != DBNull.Value ? Convert.ToDecimal(dr["Qty"]) : 0,
							Loaded_Qty = dr["Loaded_Qty"] != DBNull.Value ? Convert.ToDecimal(dr["Loaded_Qty"]) : 0,
							Ordered_Qty = dr["Ordered_Qty"] != DBNull.Value ? Convert.ToDecimal(dr["Ordered_Qty"]) : 0,
							Product_Name = dr["Product_Name"] != DBNull.Value ? Convert.ToString(dr["Product_Name"]) : "",
							PackageType_Name = dr["PackageType_Name"] != DBNull.Value ? Convert.ToString(dr["PackageType_Name"]) : "",
							SKUSize_Name = dr["SKUSize_Name"] != DBNull.Value ? Convert.ToString(dr["SKUSize_Name"]) : "",

						});
					}
				}



			}
			catch (Exception ex) { LogService.LogInsert(GetCurrentAction(), "", ex); }



			return PartialView("_Partial_AddEditForm", CommonViewModel);
		}


		[HttpGet]
		public IActionResult GetOrderProductDetails(long OrderId = 0, long ProductId = 0)
		{
			var obj = new Loading();

			try
			{

				List<SqlParameter> sqlParameters = new List<SqlParameter>();

				sqlParameters.Add(new SqlParameter("@Order_ID", SqlDbType.BigInt) { Value = OrderId });
				sqlParameters.Add(new SqlParameter("@Product_ID", SqlDbType.BigInt) { Value = ProductId });

				var ds = DataContext_Command.ExecuteStoredProcedure_DataSet("SP_OrderDetail_GET", sqlParameters);

				if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
				{
					int rowIndex = ds.Tables[0].Rows.IndexOf(ds.Tables[0].AsEnumerable().FirstOrDefault(r => r.Field<long>("Product_ID") == ProductId));

					obj = new Loading()
					{
						Id = ds.Tables[0].Rows[rowIndex]["Id"] != DBNull.Value ? Convert.ToInt64(ds.Tables[0].Rows[rowIndex]["Id"]) : 0,
						Order_ID = ds.Tables[0].Rows[rowIndex]["Order_ID"] != DBNull.Value ? Convert.ToInt64(ds.Tables[0].Rows[rowIndex]["Order_ID"]) : 0,
						Order_No = ds.Tables[0].Rows[rowIndex]["Order_No"] != DBNull.Value ? Convert.ToString(ds.Tables[0].Rows[rowIndex]["Order_No"]) : "",
						Product_ID = ds.Tables[0].Rows[rowIndex]["Product_ID"] != DBNull.Value ? Convert.ToInt64(ds.Tables[0].Rows[rowIndex]["Product_ID"]) : 0,
						PackageType_ID = ds.Tables[0].Rows[rowIndex]["PackageType_ID"] != DBNull.Value ? Convert.ToInt64(ds.Tables[0].Rows[rowIndex]["PackageType_ID"]) : 0,
						SKUSize_ID = ds.Tables[0].Rows[rowIndex]["SKUSize_ID"] != DBNull.Value ? Convert.ToInt64(ds.Tables[0].Rows[rowIndex]["SKUSize_ID"]) : 0,
						Qty = ds.Tables[0].Rows[rowIndex]["Qty"] != DBNull.Value ? Convert.ToDecimal(ds.Tables[0].Rows[rowIndex]["Qty"]) : 0,
						Loaded_Qty = ds.Tables[0].Rows[rowIndex]["Loaded_Qty"] != DBNull.Value ? Convert.ToDecimal(ds.Tables[0].Rows[rowIndex]["Loaded_Qty"]) : 0,
						Ordered_Qty = ds.Tables[0].Rows[rowIndex]["Ordered_Qty"] != DBNull.Value ? Convert.ToDecimal(ds.Tables[0].Rows[rowIndex]["Ordered_Qty"]) : 0,
						Product_Name = ds.Tables[0].Rows[rowIndex]["Product_Name"] != DBNull.Value ? Convert.ToString(ds.Tables[0].Rows[rowIndex]["Product_Name"]) : "",
						PackageType_Name = ds.Tables[0].Rows[rowIndex]["PackageType_Name"] != DBNull.Value ? Convert.ToString(ds.Tables[0].Rows[rowIndex]["PackageType_Name"]) : "",
						SKUSize_Name = ds.Tables[0].Rows[rowIndex]["SKUSize_Name"] != DBNull.Value ? Convert.ToString(ds.Tables[0].Rows[rowIndex]["SKUSize_Name"]) : "",

					};
				}

				if (ds != null && ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0 && obj != null && ProductId > 0)
				{
					obj.listQRCode = new List<ProductQrCode>();

					foreach (DataRow row in ds.Tables[1].AsEnumerable().Where(r => r.Field<long>("Product_ID") == ProductId))
					{
						//int srNo = row["SrNo"] != DBNull.Value ? Convert.ToInt32(row["SrNo"]) : 0;
						//long qrCodeId = row["Product_QR_Code_Id"] != DBNull.Value ? Convert.ToInt64(row["Product_QR_Code_Id"]) : 0;
						//string qrCode = row["QRCode"] != DBNull.Value ? row["QRCode"].ToString() : string.Empty;
						//string status = row["Status"] != DBNull.Value ? row["Status"].ToString() : string.Empty;

						//obj.listQRCode.Add((srNo, qrCodeId, qrCode, status));
						obj.listQRCode.Add(new ProductQrCode()
						{
							SrNo = row["SrNo"] != DBNull.Value ? Convert.ToInt32(row["SrNo"]) : 0,
							Id = row["Id"] != DBNull.Value ? Convert.ToInt64(row["Id"]) : 0,
							QrCode = row["QR_Code"] != DBNull.Value ? row["QR_Code"].ToString() : string.Empty,
							Status = row["Status"] != DBNull.Value ? row["Status"].ToString() : string.Empty,
							Reason = row["Reason"] != DBNull.Value ? row["Reason"].ToString() : string.Empty
						});
					}
				}
			}
			catch (Exception ex) { }

			return Json(obj);
		}


		[HttpGet]
		//[CustomAuthorizeAttribute(AccessType_Enum.Write)]
		public ActionResult Check_QR_Code(string qr_code, long OrderId, long ProductId)
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

				if (OrderId <= 0)
				{
					CommonViewModel.IsSuccess = false;
					CommonViewModel.StatusCode = ResponseStatusCode.Error;
					CommonViewModel.Message = "Please select valid Order detail.";

					return Json(CommonViewModel);
				}

				if (ProductId <= 0)
				{
					CommonViewModel.IsSuccess = false;
					CommonViewModel.StatusCode = ResponseStatusCode.Error;
					CommonViewModel.Message = "Please select valid Product.";

					return Json(CommonViewModel);
				}

				if (string.IsNullOrEmpty(qr_code) || qr_code.Length < 15)
				{
					CommonViewModel.IsSuccess = false;
					CommonViewModel.StatusCode = ResponseStatusCode.Error;
					CommonViewModel.Message = "Please enter valid QR Code.";

					return Json(CommonViewModel);
				}

				#endregion

				var (IsSuccess, response, Id, ds) = (false, ResponseStatusMessage.Error, 0M, (DataSet)null);

				List<SqlParameter> oParams = new List<SqlParameter>();

				oParams.Add(new SqlParameter("@BatchId", SqlDbType.BigInt) { Value = 0 });
				oParams.Add(new SqlParameter("@OrderId", SqlDbType.BigInt) { Value = OrderId });
				oParams.Add(new SqlParameter("@ProductId", SqlDbType.BigInt) { Value = ProductId });
				oParams.Add(new SqlParameter("@QRCode", SqlDbType.NVarChar) { Value = qr_code });

				oParams.Add(new SqlParameter("@Operated_By", SqlDbType.BigInt) { Value = AppHttpContextAccessor.GetSession(SessionKey.KEY_USER_ID) });

				(IsSuccess, response, Id, ds) = DataContext_Command.ExecuteStoredProcedure_Dataset("SP_Check_QR_Code", oParams, true);

				CommonViewModel.IsConfirm = true;
				CommonViewModel.IsSuccess = IsSuccess;
				CommonViewModel.StatusCode = IsSuccess ? ResponseStatusCode.Success : ResponseStatusCode.Error;
				CommonViewModel.Message = response;

				if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
				{
					var listQRCode = new List<ProductQrCode>();

					foreach (DataRow row in ds.Tables[0].Rows)
					{
						//int srNo = row["SrNo"] != DBNull.Value ? Convert.ToInt32(row["SrNo"]) : 0;
						//long qrCodeId = row["Product_QR_Code_Id"] != DBNull.Value ? Convert.ToInt64(row["Product_QR_Code_Id"]) : 0;
						//string qrCode = row["QR_Code"] != DBNull.Value ? row["QR_Code"].ToString() : string.Empty;
						//string status = row["Status"] != DBNull.Value ? row["Status"].ToString() : string.Empty;
						//string reason = row["Reason"] != DBNull.Value ? row["Reason"].ToString() : string.Empty;

						listQRCode.Add(new ProductQrCode()
						{
							SrNo = row["SrNo"] != DBNull.Value ? Convert.ToInt32(row["SrNo"]) : 0,
							Id = row["Id"] != DBNull.Value ? Convert.ToInt64(row["Id"]) : 0,
							QrCode = row["QR_Code"] != DBNull.Value ? row["QR_Code"].ToString() : string.Empty,
							Status = row["Status"] != DBNull.Value ? row["Status"].ToString() : string.Empty,
							Reason = row["Reason"] != DBNull.Value ? row["Reason"].ToString() : string.Empty
						});
					}

					CommonViewModel.Data = listQRCode;
				}

				return Json(CommonViewModel);

			}
			catch (Exception ex) { }

			CommonViewModel.Message = ResponseStatusMessage.Error;
			CommonViewModel.IsSuccess = false;
			CommonViewModel.StatusCode = ResponseStatusCode.Error;

			return Json(CommonViewModel);
		}

		[HttpGet]
		//[CustomAuthorizeAttribute(AccessType_Enum.Write)]
		public ActionResult Delete_QR_Code(string qr_code, long QRCodeId, long OrderId, long ProductId)
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

				if (QRCodeId <= 0)
				{
					CommonViewModel.IsSuccess = false;
					CommonViewModel.StatusCode = ResponseStatusCode.Error;
					CommonViewModel.Message = "Please select valid QR Code.";

					return Json(CommonViewModel);
				}
				
				if (OrderId <= 0)
				{
					CommonViewModel.IsSuccess = false;
					CommonViewModel.StatusCode = ResponseStatusCode.Error;
					CommonViewModel.Message = "Please select valid Order detail.";

					return Json(CommonViewModel);
				}

				if (ProductId <= 0)
				{
					CommonViewModel.IsSuccess = false;
					CommonViewModel.StatusCode = ResponseStatusCode.Error;
					CommonViewModel.Message = "Please select valid Product.";

					return Json(CommonViewModel);
				}

				#endregion

				var (IsSuccess, response, Id, ds) = (false, ResponseStatusMessage.Error, 0M, (DataSet)null);

				List<SqlParameter> oParams = new List<SqlParameter>();

				oParams.Add(new SqlParameter("@BatchId", SqlDbType.BigInt) { Value = 0 });
				oParams.Add(new SqlParameter("@OrderId", SqlDbType.BigInt) { Value = OrderId });
				oParams.Add(new SqlParameter("@ProductId", SqlDbType.BigInt) { Value = ProductId });
				oParams.Add(new SqlParameter("@QRCode", SqlDbType.NVarChar) { Value = qr_code });
				oParams.Add(new SqlParameter("@OrderLoadingId", SqlDbType.BigInt) { Value = QRCodeId });

				oParams.Add(new SqlParameter("@Operated_By", SqlDbType.BigInt) { Value = AppHttpContextAccessor.GetSession(SessionKey.KEY_USER_ID) });

				(IsSuccess, response, Id, ds) = DataContext_Command.ExecuteStoredProcedure_Dataset("SP_Delete_QR_Code", oParams, true);

				CommonViewModel.IsConfirm = true;
				CommonViewModel.IsSuccess = IsSuccess;
				CommonViewModel.StatusCode = IsSuccess ? ResponseStatusCode.Success : ResponseStatusCode.Error;
				CommonViewModel.Message = response;

				if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
				{
					var listQRCode = new List<ProductQrCode>();

					foreach (DataRow row in ds.Tables[0].Rows)
					{
						listQRCode.Add(new ProductQrCode()
						{
							SrNo = row["SrNo"] != DBNull.Value ? Convert.ToInt32(row["SrNo"]) : 0,
							Id = row["Id"] != DBNull.Value ? Convert.ToInt64(row["Id"]) : 0,
							QrCode = row["QR_Code"] != DBNull.Value ? row["QR_Code"].ToString() : string.Empty,
							Status = row["Status"] != DBNull.Value ? row["Status"].ToString() : string.Empty,
							Reason = row["Reason"] != DBNull.Value ? row["Reason"].ToString() : string.Empty
						});
					}

					CommonViewModel.Data = listQRCode;
				}

				return Json(CommonViewModel);

			}
			catch (Exception ex) { }

			CommonViewModel.Message = ResponseStatusMessage.Error;
			CommonViewModel.IsSuccess = false;
			CommonViewModel.StatusCode = ResponseStatusCode.Error;

			return Json(CommonViewModel);
		}

	}
}
