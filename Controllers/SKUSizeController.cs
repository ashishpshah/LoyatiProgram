using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Seed_Admin.Infra;
using System.Data;

namespace Seed_Admin.Controllers
{
    public class SKUSizeController : BaseController<ResponseModel<SKUSize>>
    {
        public SKUSizeController(IRepositoryWrapper repository) : base(repository) { }

        public IActionResult Index()
        {
            try
            {
                var dt = new DataTable();

                CommonViewModel.ObjList = new List<SKUSize>();

                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                sqlParameters.Add(new SqlParameter("@Id", SqlDbType.BigInt) { Value = 0 });

                dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_SKUSize_GET", sqlParameters, true);

                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        CommonViewModel.ObjList.Add(new SKUSize()
                        {
                            Id = dr["Id"] != DBNull.Value ? Convert.ToInt64(dr["Id"]) : 0,
                            Value = dr["Value"] != DBNull.Value ? Convert.ToDecimal(dr["Value"]) : 0,
                            Unit = dr["Unit"] != DBNull.Value ? Convert.ToString(dr["Unit"]) : "",
                            UNIT_TEXT = dr["UNIT_TEXT"] != DBNull.Value ? Convert.ToString(dr["UNIT_TEXT"]) : "",
                            SKUSizeName = dr["SKUSizeName"] != DBNull.Value ? Convert.ToString(dr["SKUSizeName"]) : "",
                            Description = dr["Description"] != DBNull.Value ? Convert.ToString(dr["Description"]) : "",

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
            var obj = new SKUSize() { Unit = "0" };

            var dt = new DataTable();

            var list = new List<SelectListItem_Custom>();

            try
            {

                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                if (Id > 0)
                {
                    sqlParameters.Add(new SqlParameter("@Id", SqlDbType.BigInt) { Value = Id });

                    dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_SKUSize_GET", sqlParameters, true);

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        obj = new SKUSize()
                        {
                            Id = dt.Rows[0]["Id"] != DBNull.Value ? Convert.ToInt64(dt.Rows[0]["Id"]) : 0,
                            Value = dt.Rows[0]["Value"] != DBNull.Value ? Convert.ToDecimal(dt.Rows[0]["Value"]) : 0,
                            Unit = dt.Rows[0]["Unit"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["Unit"]) : "",
                            UNIT_TEXT = dt.Rows[0]["UNIT_TEXT"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["UNIT_TEXT"]) : "",
                            SKUSizeName = dt.Rows[0]["SKUSizeName"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["SKUSizeName"]) : "",
                            Description = dt.Rows[0]["Description"] != DBNull.Value ? Convert.ToString(dt.Rows[0]["Description"]) : "",

                        };
                    }



                }



                sqlParameters = new List<SqlParameter>();
                sqlParameters.Add(new SqlParameter("@Lov_Column", SqlDbType.VarChar) { Value = "UNIT" });
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



            }
            catch (Exception ex) { LogService.LogInsert(GetCurrentAction(), "", ex); }

            CommonViewModel.SelectListItems = list;
            CommonViewModel.Obj = obj;

            return PartialView("_Partial_AddEditForm", CommonViewModel);
        }
        [HttpPost]
        public JsonResult Save(SKUSize viewModel)
        {
            try
            {

                if (viewModel.Value == 0)
                {
                    CommonViewModel.IsSuccess = false;
                    CommonViewModel.StatusCode = ResponseStatusCode.Error;
                    CommonViewModel.Message = "Please enter value.";

                    return Json(CommonViewModel);
                }

                if (viewModel.Unit == "0")
                {
                    CommonViewModel.IsSuccess = false;
                    CommonViewModel.StatusCode = ResponseStatusCode.Error;
                    CommonViewModel.Message = "Please select unit .";

                    return Json(CommonViewModel);
                }                



                var (IsSuccess, response, Id) = (false, ResponseStatusMessage.Error, 0M);



                List<SqlParameter> oParams = new List<SqlParameter>();

                oParams.Add(new SqlParameter("@Id", SqlDbType.BigInt) { Value = viewModel.Id });
                oParams.Add(new SqlParameter("@Value", SqlDbType.Decimal) { Value = viewModel.Value });
                oParams.Add(new SqlParameter("@Unit", SqlDbType.VarChar) { Value = viewModel.Unit ?? "" });
                oParams.Add(new SqlParameter("@Description", SqlDbType.VarChar) { Value = viewModel.Description ?? "" });
                oParams.Add(new SqlParameter("@Operated_By", SqlDbType.BigInt) { Value = AppHttpContextAccessor.GetSession(SessionKey.KEY_USER_ID) });
                oParams.Add(new SqlParameter("@Action", SqlDbType.VarChar) { Value = viewModel.Id == 0 ? "INSERT" : "UPDATE" });

                (IsSuccess, response, Id) = DataContext_Command.ExecuteStoredProcedure("SP_SKUSize_SAVE", oParams, true);

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
        //public ActionResult DeleteConfirmed(long Id = 0)
        //{
        //    var parameters = new List<SqlParameter>();



        //    parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = Id, Direction = ParameterDirection.Input });
        //    parameters.Add(new SqlParameter("@Operated_By", SqlDbType.Int) { Value = AppHttpContextAccessor.GetSession(SessionKey.KEY_USER_ID), Direction = ParameterDirection.Input });



        //    var response = DataContext_Command.ExecuteStoredProcedure("SP_SKUSize_Delete", parameters.ToArray());



        //    var msgtype = response.Split('|').Length > 0 ? response.Split('|')[0] : "";
        //    var message = response.Split('|').Length > 1 ? response.Split('|')[1].Replace("\"", "") : "";
        //    var strid = response.Split('|').Length > 2 ? response.Split('|')[2].Replace("\"", "") ?? "0" : "0";



        //    if (msgtype.Contains("S"))
        //    {
        //        //Common.Set_Session(SessionKey.USER_NAME, Convert.ToString(strid));
        //        CommonViewModel.IsConfirm = true;
        //        CommonViewModel.IsSuccess = true;
        //        CommonViewModel.StatusCode = ResponseStatusCode.Success;
        //        CommonViewModel.Message = message;
        //        CommonViewModel.RedirectURL = Url.Content("~/") + GetCurrentControllerUrl() + "/Index";
        //        return Json(CommonViewModel);
        //    }


        //    CommonViewModel.IsConfirm = true;
        //    CommonViewModel.IsSuccess = false;
        //    CommonViewModel.Status = ResponseStatusMessage.Error;
        //    CommonViewModel.Message = message;



        //    return Json(CommonViewModel);
        //}
    }
}
