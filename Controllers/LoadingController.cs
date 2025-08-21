using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Seed_Admin.Infra;
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
        public IActionResult Partial_AddEditForm(long User_ID = 0 , long Order_ID = 0)
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

                    dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_OrderDetail_GET", sqlParameters, true);

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            CommonViewModel.ObjList.Add(new Loading()
                            {
                                Id = dr["Id"] != DBNull.Value ? Convert.ToInt64(dr["Id"]) : 0,
                                Product_ID = dr["Product_ID"] != DBNull.Value ? Convert.ToInt64(dr["Product_ID"]) : 0,
                                PackageType_ID = dr["PackageType_ID"] != DBNull.Value ? Convert.ToInt64(dr["PackageType_ID"]) : 0,
                                SKUSize_ID = dr["SKUSize_ID"] != DBNull.Value ? Convert.ToInt64(dr["SKUSize_ID"]) : 0,
                                Qty = dr["Qty"] != DBNull.Value ? Convert.ToDecimal(dr["Qty"]) : 0,
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
    }
}
