using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Seed_Admin.Infra;
using Seed_Admin.Models;
using System.Data;

namespace Seed_Admin.Controllers
{
	public class StateController : BaseController<ResponseModel<State>>
	{
		public StateController(IRepositoryWrapper repository) : base(repository) { }
		public IActionResult Index(long CountryId=0)
		{
			try
			{
				var dt = new DataTable();

				CommonViewModel.ObjList = new List<State>();

				List<SqlParameter> sqlParameters = new List<SqlParameter>();
				sqlParameters.Add(new SqlParameter("@Id", SqlDbType.BigInt) { Value = 0 });
				sqlParameters.Add(new SqlParameter("@CountryId", SqlDbType.BigInt) { Value = CountryId });

				dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_State_GET", sqlParameters, true);

				if (dt != null && dt.Rows.Count > 0)
				{
					foreach (DataRow dr in dt.Rows)
					{
						CommonViewModel.ObjList.Add(new State()
						{
							Id = dr["Id"] != DBNull.Value ? Convert.ToInt64(dr["Id"]) : 0,
							CountryId = dr["CountryId"] != DBNull.Value ? Convert.ToInt64(dr["CountryId"]) : 0,
							StateName = dr["StateName"] != DBNull.Value ? Convert.ToString(dr["StateName"]) : "",
							IsActive = dr["IsActive"] != DBNull.Value ? Convert.ToBoolean(dr["IsActive"]) : false,
						});
					}
				}

				var dt1 = DataContext_Command.ExecuteQuery("select CountryName from Country where Id =" + CountryId);

				ViewBag.CountryName = dt1.Rows[0]["CountryName"].ToString();
				ViewBag.CountryId = CountryId;
			}
			catch (Exception ex)
			{
				LogService.LogInsert(GetCurrentAction(), "", ex);
			}
			return View(CommonViewModel);
		}
		public IActionResult Partial_AddEditForm(long Id = 0, long CountryId = 0)
		{
			var obj = new State();
			var dt = new DataTable();
			var list = new List<SelectListItem_Custom>();

			try
			{
				List<SqlParameter> sqlParameters = new List<SqlParameter>();
				if (Id > 0)
				{
					sqlParameters.Add(new SqlParameter("@id", SqlDbType.BigInt) { Value = Id });
					sqlParameters.Add(new SqlParameter("@CountryId", SqlDbType.BigInt) { Value = CountryId });
					dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_State_GET", sqlParameters, true);

					if (dt != null && dt.Rows.Count > 0)
					{
						obj = new State()
						{

							Id = dt.Rows[0]["Id"] != DBNull.Value ? Convert.ToInt64(dt.Rows[0]["Id"]) : 0,
							CountryId = dt.Rows[0]["CountryId"] != DBNull.Value ? Convert.ToInt64(dt.Rows[0]["CountryId"]) : 0,
							StateName = dt.Rows[0]["StateName"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["StateName"]) : "",

						};
					}
					
				}
				else
				{
					obj.CountryId = CountryId;
				}
				
			}
			catch (Exception ex)
			{
				LogService.LogInsert(GetCurrentAction(), "", ex);
			}
			CommonViewModel.SelectListItems = list;
			CommonViewModel.Obj = obj;

			return PartialView("Partial_AddEditform", CommonViewModel);
		}
		[HttpPost]
		public JsonResult Save(State viewModel)
		{
			try
			{
				if (string.IsNullOrEmpty(viewModel.StateName))
				{
					CommonViewModel.IsSuccess = false;
					CommonViewModel.StatusCode = ResponseStatusCode.Error;
					CommonViewModel.Message = "Please Enter State Name";

					return Json(CommonViewModel);

				}
				var (IsSuccess, response, Id) = (false, ResponseStatusMessage.Error, 0M);



				List<SqlParameter> oParams = new List<SqlParameter>();

				oParams.Add(new SqlParameter("@Id", SqlDbType.BigInt) { Value = viewModel.Id });
				oParams.Add(new SqlParameter("@CountryId", SqlDbType.BigInt) { Value = viewModel.CountryId });
				oParams.Add(new SqlParameter("@StateName", SqlDbType.VarChar) { Value = viewModel.StateName });
				oParams.Add(new SqlParameter("@Operated_By", SqlDbType.BigInt) { Value = AppHttpContextAccessor.GetSession(SessionKey.KEY_USER_ID) });
				oParams.Add(new SqlParameter("@Action", SqlDbType.VarChar) { Value = viewModel.Id == 0 ? "INSERT" : "UPDATE" });

				(IsSuccess, response, Id) = DataContext_Command.ExecuteStoredProcedure("SP_State_Save", oParams, true);

				CommonViewModel.IsConfirm = true;
				CommonViewModel.IsSuccess = IsSuccess;
				CommonViewModel.StatusCode = IsSuccess ? ResponseStatusCode.Success : ResponseStatusCode.Error;
				CommonViewModel.Message = response;

				CommonViewModel.RedirectURL = IsSuccess ? Url.Content("~/") + GetCurrentControllerUrl() + "/Index?CountryId=" + viewModel.CountryId : "";

				return Json(CommonViewModel);
			}
			catch (Exception ex)
			{
				LogService.LogInsert(GetCurrentAction(), "", ex);

				CommonViewModel.IsSuccess = false;
				CommonViewModel.StatusCode = ResponseStatusCode.Error;
				CommonViewModel.Message = ResponseStatusCode.Error + " | " + ex.Message;
			}
			return Json(CommonViewModel);

		}
		public ActionResult DeleteConfirmed(long Id = 0, long CountryId = 0)
		{

			var parameters = new List<SqlParameter>();

			parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = Id, Direction = ParameterDirection.Input });

			parameters.Add(new SqlParameter("@Operated_By", SqlDbType.Int) { Value = AppHttpContextAccessor.GetSession(SessionKey.KEY_USER_ID), Direction = ParameterDirection.Input });

			var response = DataContext_Command.ExecuteStoredProcedure("SP_State_Delete", parameters.ToArray());

			var msgtype = response.Split('|').Length > 0 ? response.Split('|')[0] : "";

			var message = response.Split('|').Length > 1 ? response.Split('|')[1].Replace("\"", "") : "";

			var strid = response.Split('|').Length > 2 ? response.Split('|')[2].Replace("\"", "") ?? "0" : "0";

			if (msgtype.Contains("S"))

			{

				//Common.Set_Session(SessionKey.USER_NAME, Convert.ToString(strid));

				CommonViewModel.IsSuccess = true;

				CommonViewModel.StatusCode = ResponseStatusCode.Success;

				CommonViewModel.Message = ResponseStatusMessage.Success;

				CommonViewModel.RedirectURL = Url.Content("~/") + GetCurrentControllerUrl() + "/Index?CountryId=" + CountryId;

				return Json(CommonViewModel);

			}

			CommonViewModel.IsSuccess = false;

			CommonViewModel.Status = ResponseStatusMessage.Error;

			CommonViewModel.Message = message;

			return Json(CommonViewModel);

		}


	}
}
