using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Seed_Admin.Infra;
using System.Data;

namespace Seed_Admin.Controllers
{
	public class FarmerController : BaseController<ResponseModel<Farmer>>
	{
		public FarmerController(IRepositoryWrapper repository) : base(repository) { }

		public IActionResult Index()
		{
			try
			{
				var dt = new DataTable();

				CommonViewModel.ObjList = new List<Farmer>();

				List<SqlParameter> sqlParameters = new List<SqlParameter>();
				sqlParameters.Add(new SqlParameter("@Id", SqlDbType.BigInt) { Value = 0 });

				dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_Farmer_GET", sqlParameters, true);

				if (dt != null && dt.Rows.Count > 0)
				{
					foreach (DataRow dr in dt.Rows)
					{
						CommonViewModel.ObjList.Add(new Farmer()
						{
							Id = dr["Id"] != DBNull.Value ? Convert.ToInt64(dr["Id"]) : 0,
							User_ID = dr["User_ID"] != DBNull.Value ? Convert.ToString(dr["User_ID"]) : "",
							Password = dr["Password"] != DBNull.Value ? Convert.ToString(dr["Password"]) : "",
							AadharNumber = dr["AadharNumber"] != DBNull.Value ? Convert.ToString(dr["AadharNumber"]) : "",
							Land_Size = dr["Land_Size"] != DBNull.Value ? Convert.ToString(dr["Land_Size"]) : "",
							Country_Id = dr["Country_Id"] != DBNull.Value ? Convert.ToInt64(dr["Country_Id"]) : 0,
							State_Id = dr["State_Id"] != DBNull.Value ? Convert.ToInt64(dr["State_Id"]) : 0,
							District_Id = dr["District_Id"] != DBNull.Value ? Convert.ToInt64(dr["District_Id"]) : 0,
							Taluka_Id = dr["Taluka_Id"] != DBNull.Value ? Convert.ToInt64(dr["Taluka_Id"]) : 0,
							Village_Id = dr["Village_Id"] != DBNull.Value ? Convert.ToInt64(dr["Village_Id"]) : 0,
							CountryName = dr["CountryName"] != DBNull.Value ? Convert.ToString(dr["CountryName"]) : "",
							StateName = dr["StateName"] != DBNull.Value ? Convert.ToString(dr["StateName"]) : "",
							DistrictName = dr["DistrictName"] != DBNull.Value ? Convert.ToString(dr["DistrictName"]) : "",
							TalukaName = dr["TalukaName"] != DBNull.Value ? Convert.ToString(dr["TalukaName"]) : "",
							VillageName = dr["VillageName"] != DBNull.Value ? Convert.ToString(dr["VillageName"]) : "",
							Preferred_Language = dr["Preferred_Language"] != DBNull.Value ? Convert.ToString(dr["Preferred_Language"]) : "",
							Preferred_Language_TEXT = dr["Preferred_Language_TEXT"] != DBNull.Value ? Convert.ToString(dr["Preferred_Language_TEXT"]) : "",
							PinCode = dr["PinCode"] != DBNull.Value ? Convert.ToInt32(dr["PinCode"]) : 0,
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
			var obj = new Farmer() { Preferred_Language = "0" };

			var dt = new DataTable();

			var list = new List<SelectListItem_Custom>();

			try
			{

				List<SqlParameter> sqlParameters = new List<SqlParameter>();

				sqlParameters.Add(new SqlParameter("@Id", SqlDbType.BigInt) { Value = (Id > 0) ? Id : -1 });

				dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_Farmer_GET", sqlParameters, true);

				if (dt != null && dt.Rows.Count > 0 && Id > 0)
				{
					obj = new Farmer()
					{
						Id = dt.Rows[0]["Id"] != DBNull.Value ? Convert.ToInt64(dt.Rows[0]["Id"]) : 0,
						User_ID = dt.Rows[0]["User_ID"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["User_ID"]) : "",
						Password = dt.Rows[0]["Password"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["Password"]) : "",
						AadharNumber = dt.Rows[0]["AadharNumber"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["AadharNumber"]) : "",
						Land_Size = dt.Rows[0]["Land_Size"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["Land_Size"]) : "",
						Country_Id = dt.Rows[0]["Country_Id"] != DBNull.Value ? Convert.ToInt64(dt.Rows[0]["Country_Id"]) : 0,
						State_Id = dt.Rows[0]["State_Id"] != DBNull.Value ? Convert.ToInt64(dt.Rows[0]["State_Id"]) : 0,
						District_Id = dt.Rows[0]["District_Id"] != DBNull.Value ? Convert.ToInt64(dt.Rows[0]["District_Id"]) : 0,
						Taluka_Id = dt.Rows[0]["Taluka_Id"] != DBNull.Value ? Convert.ToInt64(dt.Rows[0]["Taluka_Id"]) : 0,
						Village_Id = dt.Rows[0]["Village_Id"] != DBNull.Value ? Convert.ToInt64(dt.Rows[0]["Village_Id"]) : 0,
						CountryName = dt.Rows[0]["CountryName"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["CountryName"]) : "",
						StateName = dt.Rows[0]["StateName"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["StateName"]) : "",
						DistrictName = dt.Rows[0]["DistrictName"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["DistrictName"]) : "",
						TalukaName = dt.Rows[0]["TalukaName"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["TalukaName"]) : "",
						VillageName = dt.Rows[0]["VillageName"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["VillageName"]) : "",
						Preferred_Language = dt.Rows[0]["Preferred_Language"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["Preferred_Language"]) : "",
						Preferred_Language_TEXT = dt.Rows[0]["Preferred_Language_TEXT"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["Preferred_Language_TEXT"]) : "",
						PinCode = dt.Rows[0]["PinCode"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["PinCode"]) : 0,

					};
				}
				else if (dt != null && dt.Rows.Count > 0)
					obj.User_ID = dt.Rows[0]["User_ID"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["User_ID"]) : "";



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

				sqlParameters = new List<SqlParameter>();
				sqlParameters.Add(new SqlParameter("@Lov_Column", SqlDbType.VarChar) { Value = "PREFERRED_LANGUAGE" });
				dt = new DataTable();
				dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_Multiple_Lov_Combo", sqlParameters, true);

				if (dt != null && dt.Rows.Count > 0)
				{
					foreach (DataRow dr in dt.Rows)
					{
						list.Add(new SelectListItem_Custom(Convert.ToString(dr["Lov_Code"]), Convert.ToString(dr["Lov_Desc"]), Convert.ToString(dr["Lov_Column"]))
						{
							Value = dr["Lov_Code"] != DBNull.Value ? Convert.ToString(dr["Lov_Code"]) : "",
							Text = dr["Lov_Desc"] != DBNull.Value ? Convert.ToString(dr["Lov_Desc"]) : "",
							Group = dr["Lov_Column"] != DBNull.Value ? Convert.ToString(dr["Lov_Column"]) : "",
						});
					}

				}
				obj.Password = obj.Password != null ? Common.Decrypt(obj.Password) : obj.Password;


			}
			catch (Exception ex) { LogService.LogInsert(GetCurrentAction(), "", ex); }

			CommonViewModel.SelectListItems = list;
			CommonViewModel.Obj = obj;

			return PartialView("_Partial_AddEditForm", CommonViewModel);
		}
		[HttpPost]
		public JsonResult Save(Farmer viewModel)
		{
			try
			{

				if (viewModel.Id == 0 && string.IsNullOrEmpty(viewModel.User_ID))
				{
					CommonViewModel.IsSuccess = false;
					CommonViewModel.StatusCode = ResponseStatusCode.Error;
					CommonViewModel.Message = "Please enter user id.";

					return Json(CommonViewModel);
				}

				if (viewModel.Id == 0 && string.IsNullOrEmpty(viewModel.Password))
				{
					CommonViewModel.IsSuccess = false;
					CommonViewModel.StatusCode = ResponseStatusCode.Error;
					CommonViewModel.Message = "Please enter password.";

					return Json(CommonViewModel);
				}
				if (string.IsNullOrEmpty(viewModel.AadharNumber))
				{
					CommonViewModel.IsSuccess = false;
					CommonViewModel.StatusCode = ResponseStatusCode.Error;
					CommonViewModel.Message = "Please enter aadhar no.";

					return Json(CommonViewModel);
				}



				var (IsSuccess, response, Id) = (false, ResponseStatusMessage.Error, 0M);

				viewModel.Password = Common.Encrypt(viewModel.Password);

				List<SqlParameter> oParams = new List<SqlParameter>();

				oParams.Add(new SqlParameter("@Id", SqlDbType.BigInt) { Value = viewModel.Id });
				oParams.Add(new SqlParameter("@User_ID", SqlDbType.VarChar) { Value = viewModel.User_ID ?? "" });
				oParams.Add(new SqlParameter("@Password", SqlDbType.VarChar) { Value = viewModel.Password ?? "" });
				oParams.Add(new SqlParameter("@AadharNumber", SqlDbType.VarChar) { Value = viewModel.AadharNumber ?? "" });
				oParams.Add(new SqlParameter("@Land_Size", SqlDbType.VarChar) { Value = viewModel.Land_Size ?? "" });
				oParams.Add(new SqlParameter("@Country_Id", SqlDbType.BigInt) { Value = viewModel.Country_Id });
				oParams.Add(new SqlParameter("@State_Id", SqlDbType.BigInt) { Value = viewModel.State_Id });
				oParams.Add(new SqlParameter("@District_Id", SqlDbType.BigInt) { Value = viewModel.District_Id });
				oParams.Add(new SqlParameter("@Village_Id", SqlDbType.BigInt) { Value = viewModel.Village_Id });
				oParams.Add(new SqlParameter("@Taluka_Id", SqlDbType.BigInt) { Value = viewModel.Taluka_Id });
				oParams.Add(new SqlParameter("@PinCode", SqlDbType.BigInt) { Value = viewModel.PinCode });
				oParams.Add(new SqlParameter("@Preferred_Language", SqlDbType.VarChar) { Value = viewModel.Preferred_Language });
				oParams.Add(new SqlParameter("@Operated_By", SqlDbType.BigInt) { Value = AppHttpContextAccessor.GetSession(SessionKey.KEY_USER_ID) });
				oParams.Add(new SqlParameter("@Action", SqlDbType.VarChar) { Value = viewModel.Id == 0 ? "INSERT" : "UPDATE" });

				(IsSuccess, response, Id) = DataContext_Command.ExecuteStoredProcedure("SP_Farmer_SAVE", oParams, true);

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



			var response = DataContext_Command.ExecuteStoredProcedure("SP_Farmer_Delete", parameters.ToArray());



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
