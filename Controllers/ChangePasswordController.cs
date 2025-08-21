using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Seed_Admin.Infra;
using Seed_Admin.Models;
using System.Data;

namespace Seed_Admin.Controllers
{
    public class ChangePasswordController : BaseController<ResponseModel<ChangePassword>>
    {
        public ChangePasswordController(IRepositoryWrapper repository) : base(repository) { }
        public IActionResult Index()
        {
            CommonViewModel.Obj = new ChangePassword();
            return View(CommonViewModel);
        }
        [HttpPost]
        public JsonResult ChangePassword(ChangePassword viewModel)
        {
            try
            {
                if (viewModel.OldPassword == null)
                {
                    CommonViewModel.Message = "Please Enter Old Password";
                    CommonViewModel.IsSuccess = false;
                    CommonViewModel.StatusCode = ResponseStatusCode.Error;
                    return Json(CommonViewModel);
                }
                if (viewModel.NewPassword == null)
                {
                    CommonViewModel.Message = "Please Enter New Password";
                    CommonViewModel.IsSuccess = false;
                    CommonViewModel.StatusCode = ResponseStatusCode.Error;
                    return Json(CommonViewModel);
                }
                if (viewModel.ConfirmPassword == null)
                {
                    CommonViewModel.Message = "Please Enter Confirm Password";
                    CommonViewModel.IsSuccess = false;
                    CommonViewModel.StatusCode = ResponseStatusCode.Error;
                    return Json(CommonViewModel);
                }

                if (viewModel.NewPassword != viewModel.ConfirmPassword)
                {
                    CommonViewModel.Message = "Confirm Password is not matching with New Password";
                    CommonViewModel.IsSuccess = false;
                    CommonViewModel.StatusCode = ResponseStatusCode.Error;
                    return Json(CommonViewModel);
                }
                var (IsSuccess, response, Id) = (false, ResponseStatusMessage.Error, 0M);



                List<SqlParameter> oParams = new List<SqlParameter>();

                oParams.Add(new SqlParameter("@ConfirmPassword", SqlDbType.VarChar) { Value = Common.Encrypt(viewModel.ConfirmPassword) });
                oParams.Add(new SqlParameter("@OldPassword", SqlDbType.VarChar) { Value = Common.Encrypt(viewModel.OldPassword) });
                oParams.Add(new SqlParameter("@Operated_By", SqlDbType.BigInt) { Value = AppHttpContextAccessor.GetSession(SessionKey.KEY_USER_ID) });
               
                (IsSuccess, response, Id) = DataContext_Command.ExecuteStoredProcedure("SP_ChangePassword_Save", oParams, true);

                CommonViewModel.IsConfirm = true;
                CommonViewModel.IsSuccess = IsSuccess;
                CommonViewModel.StatusCode = IsSuccess ? ResponseStatusCode.Success : ResponseStatusCode.Error;
                CommonViewModel.Message = response;

                CommonViewModel.RedirectURL = IsSuccess ? Url.Action("Login", "Home"):"";

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
    }
}
