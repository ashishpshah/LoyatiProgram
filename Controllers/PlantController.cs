using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Seed_Admin.Infra;
using Seed_Admin.Models;
using System.Data;

namespace Seed_Admin.Controllers
{
	public class PlantController : BaseController<ResponseModel<Plant>>
	{
		public PlantController(IRepositoryWrapper repository) : base(repository) { }

		public IActionResult Index()
		{
			try
			{
				var dt = new DataTable();

				CommonViewModel.ObjList = new List<Plant>();

				List<SqlParameter> sqlParameters = new List<SqlParameter>();
				sqlParameters.Add(new SqlParameter("@Id", SqlDbType.BigInt) { Value = 0 });

				dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_Plant_GET", sqlParameters, true);

				if (dt != null && dt.Rows.Count > 0)
				{
					foreach (DataRow dr in dt.Rows)
					{
						CommonViewModel.ObjList.Add(new Plant()
						{
							Id = dr["Id"] != DBNull.Value ? Convert.ToInt64(dr["Id"]) : 0,
							PlantCode = dr["PlantCode"] != DBNull.Value ? Convert.ToString(dr["PlantCode"]) : "",
							PlantName = dr["PlantName"] != DBNull.Value ? Convert.ToString(dr["PlantName"]) : "",
							AddressLine1 = dr["AddressLine1"] != DBNull.Value ? Convert.ToString(dr["AddressLine1"]) : "",
							AddressLine2 = dr["AddressLine2"] != DBNull.Value ? Convert.ToString(dr["AddressLine2"]) : "",
							Country_Id = dr["Country_Id"] != DBNull.Value ? Convert.ToInt64(dr["Country_Id"]) : 0,
							State_Id = dr["State_Id"] != DBNull.Value ? Convert.ToInt64(dr["State_Id"]) : 0,
							District_Id = dr["District_Id"] != DBNull.Value ? Convert.ToInt64(dr["District_Id"]) : 0,
							Taluka_Id = dr["Taluka_Id"] != DBNull.Value ? Convert.ToInt64(dr["Taluka_Id"]) : 0,
							City_Id = dr["City_Id"] != DBNull.Value ? Convert.ToInt64(dr["City_Id"]) : 0,
							CountryName = dr["CountryName"] != DBNull.Value ? Convert.ToString(dr["CountryName"]) : "",
							StateName = dr["StateName"] != DBNull.Value ? Convert.ToString(dr["StateName"]) : "",
							DistrictName = dr["DistrictName"] != DBNull.Value ? Convert.ToString(dr["DistrictName"]) : "",
							TalukaName = dr["TalukaName"] != DBNull.Value ? Convert.ToString(dr["TalukaName"]) : "",
							VillageName = dr["VillageName"] != DBNull.Value ? Convert.ToString(dr["VillageName"]) : "",
							PinCode = dr["Pincode"] != DBNull.Value ? Convert.ToInt32(dr["Pincode"]) : 0


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
			var obj = new Plant();

			var dt = new DataTable();

			var list = new List<SelectListItem_Custom>();

			try
			{

				List<SqlParameter> sqlParameters = new List<SqlParameter>();
				if (Id > 0)
				{
					sqlParameters.Add(new SqlParameter("@Id", SqlDbType.BigInt) { Value = Id });

					dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_Plant_GET", sqlParameters, true);

					if (dt != null && dt.Rows.Count > 0)
					{
						obj = new Plant()
						{
							Id = dt.Rows[0]["Id"] != DBNull.Value ? Convert.ToInt64(dt.Rows[0]["Id"]) : 0,
							PlantCode = dt.Rows[0]["PlantCode"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["PlantCode"]) : "",
							PlantName = dt.Rows[0]["PlantName"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["PlantName"]) : "",
							AddressLine1 = dt.Rows[0]["AddressLine1"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["AddressLine1"]) : "",
							AddressLine2 = dt.Rows[0]["AddressLine2"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["AddressLine2"]) : "",
							Country_Id = dt.Rows[0]["Country_Id"] != DBNull.Value ? Convert.ToInt64(dt.Rows[0]["Country_Id"]) : 0,
							State_Id = dt.Rows[0]["State_Id"] != DBNull.Value ? Convert.ToInt64(dt.Rows[0]["State_Id"]) : 0,
							District_Id = dt.Rows[0]["District_Id"] != DBNull.Value ? Convert.ToInt64(dt.Rows[0]["District_Id"]) : 0,
							Taluka_Id = dt.Rows[0]["Taluka_Id"] != DBNull.Value ? Convert.ToInt64(dt.Rows[0]["Taluka_Id"]) : 0,
							City_Id = dt.Rows[0]["City_Id"] != DBNull.Value ? Convert.ToInt64(dt.Rows[0]["City_Id"]) : 0,
							CountryName = dt.Rows[0]["CountryName"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["CountryName"]) : "",
							StateName = dt.Rows[0]["StateName"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["StateName"]) : "",
							DistrictName = dt.Rows[0]["DistrictName"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["DistrictName"]) : "",
							TalukaName = dt.Rows[0]["TalukaName"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["TalukaName"]) : "",
							VillageName = dt.Rows[0]["VillageName"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["VillageName"]) : "",
							PinCode = dt.Rows[0]["PinCode"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["PinCode"]) : 0,
						
						};
					}


					sqlParameters = new List<SqlParameter>();
					sqlParameters.Add(new SqlParameter("@Country_Id", SqlDbType.BigInt) { Value = obj.Country_Id });
					dt = new DataTable();

					dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_State_Combo", sqlParameters, true);

					if (dt != null && dt.Rows.Count > 0)
					{
						foreach (DataRow dr in dt.Rows)
						{
							list.Add(new SelectListItem_Custom(Convert.ToString(dr["Id"]), Convert.ToString(dr["StateName"]), "State")
							{
								Value = dr["Id"] != DBNull.Value ? Convert.ToString(dr["Id"]) : "",
								Text = dr["StateName"] != DBNull.Value ? Convert.ToString(dr["StateName"]) : ""
							});
						}

					}
					sqlParameters = new List<SqlParameter>();
					sqlParameters.Add(new SqlParameter("@State_Id", SqlDbType.BigInt) { Value = obj.State_Id });
					dt = new DataTable();

					dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_District_Combo", sqlParameters, true);

					if (dt != null && dt.Rows.Count > 0)
					{
						foreach (DataRow dr in dt.Rows)
						{
							list.Add(new SelectListItem_Custom(Convert.ToString(dr["Id"]), Convert.ToString(dr["Name"]), "District")
							{
								Value = dr["Id"] != DBNull.Value ? Convert.ToString(dr["Id"]) : "",
								Text = dr["Name"] != DBNull.Value ? Convert.ToString(dr["Name"]) : ""
							});
						}

					}

					sqlParameters = new List<SqlParameter>();
					sqlParameters.Add(new SqlParameter("@District_Id", SqlDbType.BigInt) { Value = obj.District_Id });
					dt = new DataTable();

					dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_Taluka_Combo", sqlParameters, true);

					if (dt != null && dt.Rows.Count > 0)
					{
						foreach (DataRow dr in dt.Rows)
						{
							list.Add(new SelectListItem_Custom(Convert.ToString(dr["Id"]), Convert.ToString(dr["Name"]), "Taluka")
							{
								Value = dr["Id"] != DBNull.Value ? Convert.ToString(dr["Id"]) : "",
								Text = dr["Name"] != DBNull.Value ? Convert.ToString(dr["Name"]) : ""
							});
						}

					}
					sqlParameters = new List<SqlParameter>();
					sqlParameters.Add(new SqlParameter("@Taluka_Id", SqlDbType.BigInt) { Value = obj.Taluka_Id });
					dt = new DataTable();

					dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_City_Combo", sqlParameters, true);

					if (dt != null && dt.Rows.Count > 0)
					{
						foreach (DataRow dr in dt.Rows)
						{
							list.Add(new SelectListItem_Custom(Convert.ToString(dr["Id"]), Convert.ToString(dr["Name"]), "Village")
							{
								Value = dr["Id"] != DBNull.Value ? Convert.ToString(dr["Id"]) : "",
								Text = dr["Name"] != DBNull.Value ? Convert.ToString(dr["Name"]) : ""
							});
						}

					}

				}


				dt = new DataTable();

				dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_Country_Combo", null, true);

				if (dt != null && dt.Rows.Count > 0)
				{
					foreach (DataRow dr in dt.Rows)
					{
						list.Add(new SelectListItem_Custom(Convert.ToString(dr["Id"]), Convert.ToString(dr["CountryName"]), "Country")
						{
							Value = dr["Id"] != DBNull.Value ? Convert.ToString(dr["Id"]) : "",
							Text = dr["CountryName"] != DBNull.Value ? Convert.ToString(dr["CountryName"]) : ""
						});
					}

				}
				
				//    sqlParameters = new List<SqlParameter>();
				//    sqlParameters.Add(new SqlParameter("@Lov_Column", SqlDbType.NVarChar) { Value = "QUERY_TYPE" });

				//    dt = DataContext.ExecuteStoredProcedure_DataTable_SQL("SP_Lov_GET", oParams, true);

				//    if (dt != null && dt.Rows.Count > 0)
				//    {
				//        foreach (DataRow dr in dt.Rows)
				//        {
				//            list.Add(new SelectListItem_Custom(Convert.ToString(dr["LOV_CODE"]), Convert.ToString(dr["LOV_DESC"]), "QUERY_TYPE")
				//            {
				//                Value = dr["LOV_CODE"] != DBNull.Value ? Convert.ToString(dr["LOV_CODE"]) : "",
				//                Text = dr["LOV_DESC"] != DBNull.Value ? Convert.ToString(dr["LOV_DESC"]) : ""
				//            });
				//        }
				//    }

			}
			catch (Exception ex) { LogService.LogInsert(GetCurrentAction(), "", ex); }

			CommonViewModel.SelectListItems = list;
			CommonViewModel.Obj = obj;

			return PartialView("_Partial_AddEditForm", CommonViewModel);
		}
		[HttpPost]
		public JsonResult Save(Plant viewModel)
		{
			try
			{

				if (string.IsNullOrEmpty(viewModel.PlantCode))
				{
					CommonViewModel.IsSuccess = false;
					CommonViewModel.StatusCode = ResponseStatusCode.Error;
					CommonViewModel.Message = "Please enter Plant Code.";

					return Json(CommonViewModel);
				}

				if (string.IsNullOrEmpty(viewModel.PlantName))
				{
					CommonViewModel.IsSuccess = false;
					CommonViewModel.StatusCode = ResponseStatusCode.Error;
					CommonViewModel.Message = "Please enter PlantName.";

					return Json(CommonViewModel);
				}
				if (string.IsNullOrEmpty(viewModel.AddressLine1))
				{
					CommonViewModel.IsSuccess = false;
					CommonViewModel.StatusCode = ResponseStatusCode.Error;
					CommonViewModel.Message = "Please enter AddressLine1 name.";

					return Json(CommonViewModel);
				}
				var (IsSuccess, response, Id) = (false, ResponseStatusMessage.Error, 0M);

				List<SqlParameter> oParams = new List<SqlParameter>();

				oParams.Add(new SqlParameter("@Id", SqlDbType.BigInt) { Value = viewModel.Id });
				oParams.Add(new SqlParameter("@plantCode", SqlDbType.VarChar) { Value = viewModel.PlantCode ?? "" });
				oParams.Add(new SqlParameter("@PlantName", SqlDbType.VarChar) { Value = viewModel.PlantName ?? "" });
				oParams.Add(new SqlParameter("@AddressLine1", SqlDbType.VarChar) { Value = viewModel.AddressLine1 ?? "" });
				oParams.Add(new SqlParameter("@AddressLine2", SqlDbType.VarChar) { Value = viewModel.AddressLine2 ?? "" });
				oParams.Add(new SqlParameter("@Country_Id", SqlDbType.BigInt) { Value = viewModel.Country_Id });
				oParams.Add(new SqlParameter("@State_Id", SqlDbType.BigInt) { Value = viewModel.State_Id });
				oParams.Add(new SqlParameter("@District_Id", SqlDbType.BigInt) { Value = viewModel.District_Id });
				oParams.Add(new SqlParameter("@Taluka_Id", SqlDbType.BigInt) { Value = viewModel.Taluka_Id });
				oParams.Add(new SqlParameter("@City_Id", SqlDbType.BigInt) { Value = viewModel.City_Id });
				oParams.Add(new SqlParameter("@PinCode", SqlDbType.BigInt) { Value = viewModel.PinCode });
				oParams.Add(new SqlParameter("@Operated_By", SqlDbType.BigInt) { Value = AppHttpContextAccessor.GetSession(SessionKey.KEY_USER_ID) });
				oParams.Add(new SqlParameter("@Action", SqlDbType.VarChar) { Value = viewModel.Id == 0 ? "INSERT" : "UPDATE" });

				(IsSuccess, response, Id) = DataContext_Command.ExecuteStoredProcedure("SP_Plant_SAVE", oParams, true);

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


		public ActionResult DeleteConfirmed(long Id = 0)
		{
			var parameters = new List<SqlParameter>();
			parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = Id, Direction = ParameterDirection.Input });
			parameters.Add(new SqlParameter("@Operated_By", SqlDbType.Int) { Value = AppHttpContextAccessor.GetSession(SessionKey.KEY_USER_ID), Direction = ParameterDirection.Input });
			
			var response = DataContext_Command.ExecuteStoredProcedure("SP_Plant_Delete", parameters.ToArray());
			var msgtype = response.Split('|').Length > 0 ? response.Split('|')[0] : "";
			var message = response.Split('|').Length > 1 ? response.Split('|')[1].Replace("\"", "") : "";
			var strid = response.Split('|').Length > 2 ? response.Split('|')[2].Replace("\"", "") ?? "0" : "0";
			
			if (msgtype.Contains("S"))
			{
				//Common.Set_Session(SessionKey.USER_NAME, Convert.ToString(strid));
				CommonViewModel.IsConfirm = true;
				CommonViewModel.IsSuccess = true;
				CommonViewModel.StatusCode = ResponseStatusCode.Success;
				CommonViewModel.Message = message;
				CommonViewModel.RedirectURL = Url.Content("~/") + GetCurrentControllerUrl() + "/Index";
				return Json(CommonViewModel);
			}

			CommonViewModel.IsConfirm = true;
			CommonViewModel.IsSuccess = false;
			CommonViewModel.Status = ResponseStatusMessage.Error;
			CommonViewModel.Message = message;
			 
			return Json(CommonViewModel);
		}

		public IActionResult GetCountryStateDistrictVillageTaluka(string Type = "", long ParentId = 0)
		{
			var list = new List<SelectListItem_Custom>();

			List<SqlParameter> oParams = new List<SqlParameter>();

			var dt = new DataTable();

			try
			{
				if (string.IsNullOrEmpty(Type))
				{
					dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_Country_Combo", null, true);

					if (dt != null && dt.Rows.Count > 0)
						foreach (DataRow dr in dt.Rows)
							list.Add(new SelectListItem_Custom(Convert.ToString(dr["Id"]), Convert.ToString(dr["CountryName"]), "Country"));
				}
				else if (!string.IsNullOrEmpty(Type) && Type == "STATE")
				{
					oParams = new List<SqlParameter>();
					oParams.Add(new SqlParameter("@Country_Id", SqlDbType.BigInt) { Value = ParentId });

					dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_State_Combo", oParams, true);

					if (dt != null && dt.Rows.Count > 0)
						foreach (DataRow dr in dt.Rows)
							list.Add(new SelectListItem_Custom(Convert.ToString(dr["Id"]), Convert.ToString(dr["StateName"]), "State"));
				}
				else if (!string.IsNullOrEmpty(Type) && Type == "DISTRICT")
				{
					oParams = new List<SqlParameter>();
					oParams.Add(new SqlParameter("@State_Id", SqlDbType.BigInt) { Value = ParentId });

					dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_District_Combo", oParams, true);

					if (dt != null && dt.Rows.Count > 0)
						foreach (DataRow dr in dt.Rows)
							list.Add(new SelectListItem_Custom(Convert.ToString(dr["Id"]), Convert.ToString(dr["Name"]), "District"));
				}
				else if (!string.IsNullOrEmpty(Type) && Type == "TALUKA")
				{
					oParams = new List<SqlParameter>();
					oParams.Add(new SqlParameter("@District_Id", SqlDbType.BigInt) { Value = ParentId });

					dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_Taluka_Combo", oParams, true);

					if (dt != null && dt.Rows.Count > 0)
						foreach (DataRow dr in dt.Rows)
							list.Add(new SelectListItem_Custom(Convert.ToString(dr["Id"]), Convert.ToString(dr["Name"]), "Taluka"));
				}
				else if (!string.IsNullOrEmpty(Type) && Type == "VILLAGE")
				{
					oParams = new List<SqlParameter>();
					oParams.Add(new SqlParameter("@Taluka_Id", SqlDbType.BigInt) { Value = ParentId });

					dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_City_Combo", oParams, true);

					if (dt != null && dt.Rows.Count > 0)
						foreach (DataRow dr in dt.Rows)
							list.Add(new SelectListItem_Custom(Convert.ToString(dr["Id"]), Convert.ToString(dr["Name"]), "Village"));
				}

			}
			catch (Exception ex) { LogService.LogInsert(GetCurrentAction(), "", ex); }

			return Json(list);
		}
	}
}
