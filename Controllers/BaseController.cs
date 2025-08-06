using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Seed_Admin.Infra;

namespace Seed_Admin.Controllers
{
	public class BaseController : Controller
	{

	}

	public class BaseController<T> : BaseController where T : class
	{
		public T CommonViewModel { get; set; } = default(T);

		public bool IsLogActive = false;

		public readonly DateTime? nullDateTime = null;
		public string ControllerName = "";
		public string ActionName = "";
		public string AreaName = "";


		public IRepositoryWrapper _context;

		public BaseController() {  }

		public BaseController(IRepositoryWrapper repository)
		{
			_context = repository;
			CommonViewModel = (dynamic)Activator.CreateInstance(typeof(T));
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			try
			{
				ControllerName = Convert.ToString(context.RouteData.Values["controller"]);
				ActionName = Convert.ToString(context.RouteData.Values["action"]);

				if (context.RouteData.DataTokens != null) AreaName = Convert.ToString(context.RouteData.DataTokens["area"]);

				if (string.IsNullOrEmpty(AreaName)) AreaName = Convert.ToString(context.RouteData.Values["area"]);

				List<UserMenuAccess> listMenuAccess = Common.GetUserMenuPermission();

				if (listMenuAccess != null && listMenuAccess.Count > 0)
				{
					if (listMenuAccess.FindIndex(x => x.Controller == ControllerName) > -1)
					{
						//CommonViewModel.IsCreate = listMenuAccess[listMenuAccess.FindIndex(x => x.Controller == ControllerName)].IsCreate;
						//CommonViewModel.IsRead = listMenuAccess[listMenuAccess.FindIndex(x => x.Controller == ControllerName)].IsRead;
						//CommonViewModel.IsUpdate = listMenuAccess[listMenuAccess.FindIndex(x => x.Controller == ControllerName)].IsUpdate;
						//CommonViewModel.IsDelete = listMenuAccess[listMenuAccess.FindIndex(x => x.Controller == ControllerName)].IsDelete;

						try { Common.Set_Session_Int(SessionKey.CURRENT_MENU_ID, listMenuAccess[listMenuAccess.FindIndex(x => x.Controller == ControllerName)].MenuId); }
						catch { Common.Set_Session_Int(SessionKey.CURRENT_MENU_ID, 0); }
					}
				}

				if (!Common.IsUserLogged() && !string.IsNullOrEmpty(AreaName) && !(Convert.ToString(ControllerName).ToLower() == "home" && (new string[] { "account", "login" }).Any(x => x == Convert.ToString(ActionName).ToLower())))
				{
					context.Result = new RedirectResult(Url.Content("~/") + (string.IsNullOrEmpty(AreaName) ? "" : AreaName + "/") + "Home/Account");
					return;
				}
				else if (!Common.IsUserLogged() && !Common.IsAdmin() && !string.IsNullOrEmpty(AreaName) && !(Convert.ToString(ControllerName).ToLower() == "home" && (new string[] { "account", "login" }).Any(x => x == Convert.ToString(ActionName).ToLower())))
				{
					context.Result = new RedirectResult(Url.Content("~/") + "Home/Login");
					return;
				}

			}
			catch (Exception ex) { LogService.LogInsert(GetCurrentAction(), "", ex); }
		}


		public string GetCurrentAction() => (string.IsNullOrEmpty(AreaName) ? "" : AreaName + " - ") + ControllerName + " - " + ActionName;
		public string GetCurrentControllerUrl() => (string.IsNullOrEmpty(AreaName) ? "" : AreaName + "/") + ControllerName;
	}
}
