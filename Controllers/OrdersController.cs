using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Seed_Admin.Infra;
using System.Collections.Generic;
using System.Data;
using System.Globalization;

namespace Seed_Admin.Controllers
{
    public class OrdersController : BaseController<ResponseModel<Orders>>
    {
        public OrdersController(IRepositoryWrapper repository) : base(repository) { }

        public IActionResult Index()
        {
            
            try
            {
                var dt = new DataTable();
                var list = new List<SelectListItem_Custom>();

                CommonViewModel.ObjList = new List<Orders>();

                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                sqlParameters.Add(new SqlParameter("@Id", SqlDbType.BigInt) { Value = 0 });
                sqlParameters.Add(new SqlParameter("@User_Id", SqlDbType.BigInt) { Value = AppHttpContextAccessor.GetSession(SessionKey.KEY_USER_ID) });

                dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_ORDERS_GET", sqlParameters, true);

                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        CommonViewModel.ObjList.Add(new Orders()
                        {
                            Id = dr["Id"] != DBNull.Value ? Convert.ToInt64(dr["Id"]) : 0,
                            Dealer_Name = dr["Dealer_Name"] !=DBNull.Value ? Convert.ToString(dr["Dealer_Name"]) : "",
                            Order_No = dr["Order_No"] != DBNull.Value ? Convert.ToString(dr["Order_No"]) : "",
                            Order_Date = dr["Order_Date"] != DBNull.Value ? Convert.ToDateTime(dr["Order_Date"]) : nullDateTime,
                            Total_Qty = dr["Total_Qty"] != DBNull.Value ? Convert.ToDecimal(dr["Total_Qty"]) : 0
                        });
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
                CommonViewModel.SelectListItems = list;
            }

            

            catch (Exception ex) { LogService.LogInsert(GetCurrentAction(), "", ex); }

           
            return View(CommonViewModel);
        }

        [HttpGet]
        public IActionResult Partial_AddEditForm(long Id = 0 , string ORDER_DETAIL = "")
        {
            var obj = new Orders();
            var listOrderDetail = new List<Order_Detail>();

            var ds = new DataSet();
            var dt = new DataTable();

            var list = new List<SelectListItem_Custom>();

            try
            {

                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                if (Id > 0)
                {
                    sqlParameters.Add(new SqlParameter("@Id", SqlDbType.BigInt) { Value = Id });
                    sqlParameters.Add(new SqlParameter("@User_Id", SqlDbType.BigInt) { Value = AppHttpContextAccessor.GetSession(SessionKey.KEY_USER_ID) });

                    ds = DataContext_Command.ExecuteStoredProcedure_DataSet("SP_ORDERS_GET", sqlParameters);

                    if (ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                    {
                        obj = new Orders()
                        {
                            Id = ds.Tables[0].Rows[0]["Id"] != DBNull.Value ? Convert.ToInt64(ds.Tables[0].Rows[0]["Id"]) : 0,
                            Order_No = ds.Tables[0].Rows[0]["Order_No"] != DBNull.Value ? Convert.ToString(ds.Tables[0].Rows[0]["Order_No"]) : "",
                            Order_Date = ds.Tables[0].Rows[0]["Order_Date"] != DBNull.Value ? Convert.ToDateTime(ds.Tables[0].Rows[0]["Order_Date"]) : nullDateTime,
                            Total_Qty = ds.Tables[0].Rows[0]["Total_Qty"] != DBNull.Value ? Convert.ToDecimal(ds.Tables[0].Rows[0]["Total_Qty"]) : 0
                        };
                        foreach (DataRow dr in ds.Tables[1].Rows)
                        {
                            var product = dr["Product_Name"] != DBNull.Value ? Convert.ToString(dr["Product_Name"]) : "";
                            var PackageType_ID = dr["PackageType_ID"] != DBNull.Value ? Convert.ToInt64(dr["PackageType_ID"]) : 0;
                            listOrderDetail.Add(new Order_Detail()
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

                            sqlParameters = new List<SqlParameter>();
                            sqlParameters.Add(new SqlParameter("@Product", SqlDbType.VarChar) { Value = product });
                            dt = new DataTable();
                            dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_PackageType_Combo", sqlParameters, true);

                            if (dt != null && dt.Rows.Count > 0)
                            {
                                foreach (DataRow drpkg in dt.Rows)
                                {
                                    list.Add(new SelectListItem_Custom(Convert.ToString(drpkg["Id"]), Convert.ToString(drpkg["PackageTypeName"]),product ,"PackageType")
                                    {
                                        Value = drpkg["Id"] != DBNull.Value ? Convert.ToString(drpkg["Id"]) : "",
                                        Text = drpkg["PackageTypeName"] != DBNull.Value ? Convert.ToString(drpkg["PackageTypeName"]) : ""
                                    });
                                }

                            }

                            sqlParameters = new List<SqlParameter>();
                            sqlParameters.Add(new SqlParameter("@PackageType_ID", SqlDbType.BigInt) { Value = PackageType_ID });
                            dt = new DataTable();
                            dt = DataContext_Command.ExecuteStoredProcedure_DataTable("SP_SKUSize_Combo", sqlParameters, true);

                            if (dt != null && dt.Rows.Count > 0)
                            {
                                foreach (DataRow drpkg in dt.Rows)
                                {
                                    list.Add(new SelectListItem_Custom(Convert.ToString(drpkg["Id"]), Convert.ToString(drpkg["SKUSizeName"]), PackageType_ID.ToString(), "SKUSize")
                                    {
                                        Value = drpkg["Id"] != DBNull.Value ? Convert.ToString(drpkg["Id"]) : "",
                                        Text = drpkg["SKUSizeName"] != DBNull.Value ? Convert.ToString(drpkg["SKUSizeName"]) : ""
                                    });
                                }

                            }

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
            CommonViewModel.Obj.list_Order_Details = listOrderDetail;          
            return PartialView("_Partial_AddEditForm", CommonViewModel);
        }
        [HttpPost]
        public JsonResult Save(Orders viewModel)
        {
            try
            {
                //DateTime? Order_Date = viewModel.Order_Date_Text != null ? DateTime.ParseExact(viewModel.Order_Date_Text, "dd/MM/yyyy", CultureInfo.InvariantCulture) : null;
                if (string.IsNullOrEmpty(viewModel.Order_No) && viewModel.Id  == 0)
                {
                    CommonViewModel.IsSuccess = false;
                    CommonViewModel.StatusCode = ResponseStatusCode.Error;
                    CommonViewModel.Message = "Please enter order id.";

                    return Json(CommonViewModel);
                }

                if (viewModel.Order_Date == null)
                {
                    CommonViewModel.IsSuccess = false;
                    CommonViewModel.StatusCode = ResponseStatusCode.Error;
                    CommonViewModel.Message = "Please select date.";

                    return Json(CommonViewModel);
                }
               

                if(viewModel.list_Order_Details == null)
                {
                    CommonViewModel.IsSuccess = false;
                    CommonViewModel.StatusCode = ResponseStatusCode.Error;
                    CommonViewModel.Message = "Please enter  atleast one order detail.";

                    return Json(CommonViewModel);
                }
                if (viewModel.list_Order_Details != null && viewModel.list_Order_Details.Count > 0)
                {
                    for (int i = 0; i < viewModel.list_Order_Details.Count; i++)
                    {
                        if (viewModel.list_Order_Details[i].Product_ID == 0)
                        {
                            CommonViewModel.IsSuccess = false;
                            CommonViewModel.StatusCode = ResponseStatusCode.Error;
                            CommonViewModel.Message = "Please select product at  line no " + (i + 1);
                            return Json(CommonViewModel);
                        }
                    }
                }
                if (viewModel.list_Order_Details != null && viewModel.list_Order_Details.Count > 0)
                {
                    for (int i = 0; i < viewModel.list_Order_Details.Count; i++)
                    {
                        if (viewModel.list_Order_Details[i].PackageType_ID == 0)
                        {
                            CommonViewModel.IsSuccess = false;
                            CommonViewModel.StatusCode = ResponseStatusCode.Error;
                            CommonViewModel.Message = "Please select package type at  line no " + (i + 1);
                            return Json(CommonViewModel);
                        }
                    }
                }
                if (viewModel.list_Order_Details != null && viewModel.list_Order_Details.Count > 0)
                {
                    for (int i = 0; i < viewModel.list_Order_Details.Count; i++)
                    {
                        if (viewModel.list_Order_Details[i].SKUSize_ID == 0)
                        {
                            CommonViewModel.IsSuccess = false;
                            CommonViewModel.StatusCode = ResponseStatusCode.Error;
                            CommonViewModel.Message = "Please select SKU Size at  line no " + (i + 1);
                            return Json(CommonViewModel);
                        }
                    }
                }
                if (viewModel.list_Order_Details != null && viewModel.list_Order_Details.Count > 0)
                {
                    for (int i = 0; i < viewModel.list_Order_Details.Count; i++)
                    {
                        if (viewModel.list_Order_Details[i].Qty == 0)
                        {
                            CommonViewModel.IsSuccess = false;
                            CommonViewModel.StatusCode = ResponseStatusCode.Error;
                            CommonViewModel.Message = "Please enter quantity at line no " + (i + 1);
                            return Json(CommonViewModel);
                        }
                    }
                }

                DataTable orderdetailtable = new DataTable();
                orderdetailtable.Columns.Add("Product_ID", typeof(long));
                orderdetailtable.Columns.Add("PackageType_ID", typeof(long));
                orderdetailtable.Columns.Add("SKUSize_ID", typeof(long));
                orderdetailtable.Columns.Add("Qty", typeof(decimal));

                if (viewModel.list_Order_Details != null && viewModel.list_Order_Details.Count > 0)
                {
                    foreach (var order_Detail in viewModel.list_Order_Details)
                    {
                        orderdetailtable.Rows.Add(order_Detail.Product_ID ,order_Detail.PackageType_ID ,order_Detail.SKUSize_ID, order_Detail.Qty);
                    }
                }

                var (IsSuccess, response, Id) = (false, ResponseStatusMessage.Error, 0M);

                

                List<SqlParameter> oParams = new List<SqlParameter>();

                oParams.Add(new SqlParameter("@Id", SqlDbType.BigInt) { Value = viewModel.Id });
                oParams.Add(new SqlParameter("@Order_No", SqlDbType.VarChar) { Value = viewModel.Order_No ?? "" });
                oParams.Add(new SqlParameter("@Order_Date", SqlDbType.DateTime) { Value = viewModel.Order_Date ?? null });
                oParams.Add(new SqlParameter("@Type_ORDER_DETAIL", SqlDbType.Structured) { Value = orderdetailtable  });
                oParams.Add(new SqlParameter("@Operated_By", SqlDbType.BigInt) { Value = AppHttpContextAccessor.GetSession(SessionKey.KEY_USER_ID) });
                oParams.Add(new SqlParameter("@Action", SqlDbType.VarChar) { Value = viewModel.Id == 0 ? "INSERT" : "UPDATE" });

                (IsSuccess, response, Id) = DataContext_Command.ExecuteStoredProcedure("SP_ORDERS_SAVE", oParams, true);

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
