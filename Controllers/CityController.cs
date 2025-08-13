using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Seed_Admin.Infra;
using Seed_Admin.Models;
using System.Data;

namespace Seed_Admin.Controllers
{
	public class CityController : BaseController<ResponseModel<City>>
	{
		public CityController(IRepositoryWrapper repository) : base(repository) { }
		public IActionResult Index(long TalukaId = 0, long StateId = 0, long CountryId = 0, long DistrictId = 0)
		{
			try
			{
				var dt = new DataTable();

				CommonViewModel.ObjList = new List<City>();

				List<SqlParameter> sqlParameters = new List<SqlParameter>();
				sqlParameters.Add(new SqlParameter("@Id", SqlDbType.BigInt) { Value = 0 });
				sqlParameters.Add(new SqlParameter("@Taluka_Id", SqlDbType.BigInt) { Value = TalukaId });


				dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_City_GET", sqlParameters, true);

				if (dt != null && dt.Rows.Count > 0)
				{
					foreach (DataRow dr in dt.Rows)
					{
						CommonViewModel.ObjList.Add(new City()
						{
							Id = dr["Id"] != DBNull.Value ? Convert.ToInt64(dr["Id"]) : 0,
							TalukaId = dr["Taluka_Id"] != DBNull.Value ? Convert.ToInt64(dr["Taluka_Id"]) : 0,
							Name = dr["Name"] != DBNull.Value ? Convert.ToString(dr["Name"]) : "",
							IsActive = dr["IsActive"] != DBNull.Value ? Convert.ToBoolean(dr["IsActive"]) : false,
						});
					}
				}

				var dt1 = DataContext_Command.ExecuteQuery("select Name from Taluka where Id =" + TalukaId);

				ViewBag.TalukaName = dt1.Rows[0]["Name"].ToString();
				ViewBag.StateId = StateId;
				ViewBag.CountryId = CountryId;
				ViewBag.DistrictId = DistrictId;
				ViewBag.TalukaId = TalukaId;
			}

			catch (Exception ex)
			{
				LogService.LogInsert(GetCurrentAction(), "", ex);
			}
			return View(CommonViewModel);
		}
		public IActionResult Partial_AddEditForm(long Id = 0, long TalukaId = 0, long DistrictId = 0, long StateId = 0, long CountryId = 0)
		{
			var obj = new City();
			var dt = new DataTable();
			var list = new List<SelectListItem_Custom>();

			try
			{
				List<SqlParameter> sqlParameters = new List<SqlParameter>();
				if (Id > 0)
				{
					sqlParameters.Add(new SqlParameter("@id", SqlDbType.BigInt) { Value = Id });
					sqlParameters.Add(new SqlParameter("@Taluka_Id", SqlDbType.BigInt) { Value = TalukaId });
					dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_City_GET", sqlParameters, true);

					if (dt != null && dt.Rows.Count > 0)
					{
						obj = new City()
						{

							Id = dt.Rows[0]["Id"] != DBNull.Value ? Convert.ToInt64(dt.Rows[0]["Id"]) : 0,
							TalukaId = dt.Rows[0]["Taluka_Id"] != DBNull.Value ? Convert.ToInt64(dt.Rows[0]["Taluka_Id"]) : 0,
							Name = dt.Rows[0]["Name"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["Name"]) : "",

						};
					}
				}
				else
				{
					obj.TalukaId = TalukaId;

				}
				obj.StateId = StateId;
				obj.DistrictId = DistrictId;
				obj.CountryId = CountryId;
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
		public JsonResult Save(City viewModel)
		{
			try
			{
				if (string.IsNullOrEmpty(viewModel.Name))
				{
					CommonViewModel.IsSuccess = false;
					CommonViewModel.StatusCode = ResponseStatusCode.Error;
					CommonViewModel.Message = "Please Enter City Name";

					return Json(CommonViewModel);

				}
				var (IsSuccess, response, Id) = (false, ResponseStatusMessage.Error, 0M);

				List<SqlParameter> oParams = new List<SqlParameter>();

				oParams.Add(new SqlParameter("@Id", SqlDbType.BigInt) { Value = viewModel.Id });
				oParams.Add(new SqlParameter("@Taluka_Id", SqlDbType.BigInt) { Value = viewModel.TalukaId });
				oParams.Add(new SqlParameter("@Name", SqlDbType.VarChar) { Value = viewModel.Name });
				oParams.Add(new SqlParameter("@Operated_By", SqlDbType.BigInt) { Value = AppHttpContextAccessor.GetSession(SessionKey.KEY_USER_ID) });
				oParams.Add(new SqlParameter("@Action", SqlDbType.VarChar) { Value = viewModel.Id == 0 ? "INSERT" : "UPDATE" });

				(IsSuccess, response, Id) = DataContext_Command.ExecuteStoredProcedure("SP_City_Save", oParams, true);

				CommonViewModel.IsConfirm = true;
				CommonViewModel.IsSuccess = IsSuccess;
				CommonViewModel.StatusCode = IsSuccess ? ResponseStatusCode.Success : ResponseStatusCode.Error;
				CommonViewModel.Message = response;

				CommonViewModel.RedirectURL = IsSuccess ? $"{Url.Content("~/")}{GetCurrentControllerUrl()}/Index?TalukaId={viewModel.TalukaId}&DistrictId={viewModel.DistrictId}&StateId={viewModel.StateId}&CountryId={viewModel.CountryId}" : "";

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
		public ActionResult DeleteConfirmed(long Id = 0, long TalukaId = 0, long DistrictId = 0, long StateId = 0, long CountryId = 0)

		{

			var parameters = new List<SqlParameter>();

			parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = Id, Direction = ParameterDirection.Input });

			parameters.Add(new SqlParameter("@Operated_By", SqlDbType.Int) { Value = AppHttpContextAccessor.GetSession(SessionKey.KEY_USER_ID), Direction = ParameterDirection.Input });

			var response = DataContext_Command.ExecuteStoredProcedure("SP_City_Delete", parameters.ToArray());

			var msgtype = response.Split('|').Length > 0 ? response.Split('|')[0] : "";

			var message = response.Split('|').Length > 1 ? response.Split('|')[1].Replace("\"", "") : "";

			var strid = response.Split('|').Length > 2 ? response.Split('|')[2].Replace("\"", "") ?? "0" : "0";

			if (msgtype.Contains("S"))

			{

				//Common.Set_Session(SessionKey.USER_NAME, Convert.ToString(strid));

				CommonViewModel.IsSuccess = true;

				CommonViewModel.StatusCode = ResponseStatusCode.Success;

				CommonViewModel.Message = ResponseStatusMessage.Success;

				CommonViewModel.RedirectURL = Url.Content("~/") + GetCurrentControllerUrl() + "/Index?TalukaId=" + TalukaId + "&StateId=" + StateId + "&DistrictId=" + DistrictId + "&CountryId=" + CountryId;


				return Json(CommonViewModel);

			}

			CommonViewModel.IsSuccess = false;

			CommonViewModel.Status = ResponseStatusMessage.Error;

			CommonViewModel.Message = message;

			return Json(CommonViewModel);

		}


	}
}
