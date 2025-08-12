using Seed_Admin.Controllers;
using Seed_Admin.Infra;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace Seed_Admin.Areas.Admin.Controllers
{

	public class SurveyController : BaseController<ResponseModel<Survey>>
	{
		public SurveyController(IRepositoryWrapper repository) : base(repository) { }

		// GET: Admin/Survey
		public ActionResult Index()
		{
			if (Common.IsSuperAdmin() || Common.IsAdmin()) CommonViewModel.ObjList = _context.Using<Survey>().GetAll().ToList();

			for (int i = 0; i < CommonViewModel.ObjList.Count(); i++)
			{
				CommonViewModel.ObjList[i].StartDate_Text = CommonViewModel.ObjList[i].StartDate.ToString(Common.DateTimeFormat_ddMMyyyy_HHmm);
				CommonViewModel.ObjList[i].EndDate_Text = CommonViewModel.ObjList[i].EndDate.ToString(Common.DateTimeFormat_ddMMyyyy_HHmm);
				CommonViewModel.ObjList[i].CreatedDate_Text = CommonViewModel.ObjList[i].CreatedDate?.ToString(Common.DateTimeFormat_ddMMyyyy_HHmm);
			}

			return View(CommonViewModel);
		}

		//[CustomAuthorizeAttribute(AccessType_Enum.Read)]
		public ActionResult Partial_AddEditForm(long Id = 0)
		{
			if (!(Common.IsSuperAdmin() || Common.IsAdmin()))
			{
				CommonViewModel.IsSuccess = false;
				CommonViewModel.StatusCode = ResponseStatusCode.Error;
				CommonViewModel.Message = ResponseStatusMessage.UnAuthorize;

				return Json(CommonViewModel);
			}

			CommonViewModel.Obj = new Survey() { StartDate = DateTime.Now.Date, EndDate = DateTime.Now.Date.AddDays(1).AddMinutes(-1) };

			if (Id > 0) CommonViewModel.Obj = _context.Using<Survey>().GetByCondition(x => x.Id == Id).FirstOrDefault();

			CommonViewModel.Obj.StartDate_Text = CommonViewModel.Obj.StartDate.ToString(Common.DateTimeFormat_ddMMyyyy_HHmm);
			CommonViewModel.Obj.EndDate_Text = CommonViewModel.Obj.EndDate.ToString(Common.DateTimeFormat_ddMMyyyy_HHmm);


			return PartialView("_Partial_AddEditForm", CommonViewModel);
		}

		[HttpPost]
		//[CustomAuthorizeAttribute(AccessType_Enum.Write)]
		public ActionResult Save(Survey viewModel)
		{
			if (viewModel != null)
			{
				#region Validation

				if (!(Common.IsSuperAdmin() || Common.IsAdmin()))
				{
					CommonViewModel.IsSuccess = false;
					CommonViewModel.StatusCode = ResponseStatusCode.Error;
					CommonViewModel.Message = ResponseStatusMessage.UnAuthorize;

					return Json(CommonViewModel);
				}

				if (string.IsNullOrEmpty(viewModel.Title))
				{
					CommonViewModel.IsSuccess = false;
					CommonViewModel.StatusCode = ResponseStatusCode.Error;
					CommonViewModel.Message = "Please enter Survey Title.";

					return Json(CommonViewModel);
				}

				if (!string.IsNullOrEmpty(viewModel.StartDate_Text)) { try { viewModel.StartDate = DateTime.ParseExact(viewModel.StartDate_Text, "yyyy-MM-dd", CultureInfo.InvariantCulture); } catch { } }

				if (!string.IsNullOrEmpty(viewModel.EndDate_Text)) { try { viewModel.EndDate = DateTime.ParseExact(viewModel.EndDate_Text, "yyyy-MM-dd", CultureInfo.InvariantCulture); } catch { } }

				if (_context.Using<Survey>().Any(x => x.Title.ToLower().Replace(" ", "") == viewModel.Title.ToLower().Replace(" ", "") && x.Id != viewModel.Id &&
						x.StartDate <= viewModel.EndDate && x.EndDate >= viewModel.StartDate))
				{
					CommonViewModel.IsSuccess = false;
					CommonViewModel.StatusCode = ResponseStatusCode.Error;
					CommonViewModel.Message = "Survey already exist. Please try another Survey.";

					return Json(CommonViewModel);
				}

				#endregion

				#region Database-Transaction

				using (var transaction = _context.BeginTransaction())
				{
					try
					{
						Survey obj = _context.Using<Survey>().GetByCondition(x => x.Id == viewModel.Id).FirstOrDefault();

						if (obj != null)
						{
							obj.Title = viewModel.Title;
							obj.Description = viewModel.Description;
							obj.StartDate = viewModel.StartDate;
							obj.EndDate = viewModel.EndDate;

							obj.IsActive = viewModel.IsActive;
							obj.IsDeleted = obj.IsActive ? false : obj.IsDeleted;

							_context.Using<Survey>().Update(obj);
							//_context.Entry(obj).State = EntityState.Modified;
							//_context.SaveChanges();
						}
						else
						{
							var _viewModel = _context.Using<Survey>().Add(viewModel);
							viewModel.Id = _viewModel.Id;
							//_context.SaveChanges();
							//_context.Entry(viewModel).Reload();
						}

						CommonViewModel.IsConfirm = true;
						CommonViewModel.IsSuccess = true;
						CommonViewModel.StatusCode = ResponseStatusCode.Success;
						CommonViewModel.Message = ResponseStatusMessage.Success;
						CommonViewModel.RedirectURL = Url.Action("Index", "SurveyQuestion", new { id = viewModel.Id });

						transaction.Commit();

						return Json(CommonViewModel);
					}
					catch (Exception ex) { transaction.Rollback(); }
				}

				#endregion
			}

			CommonViewModel.IsSuccess = false;
			CommonViewModel.Message = ResponseStatusMessage.Error;
			CommonViewModel.StatusCode = ResponseStatusCode.Error;

			return Json(CommonViewModel);
		}

		[HttpPost]
		//[CustomAuthorizeAttribute(AccessType_Enum.Delete)]
		public ActionResult DeleteConfirmed(long Id)
		{
			try
			{
				if (!(Common.IsSuperAdmin() || Common.IsAdmin()))
				{
					CommonViewModel.IsSuccess = false;
					CommonViewModel.StatusCode = ResponseStatusCode.Error;
					CommonViewModel.Message = ResponseStatusMessage.UnAuthorize;

					return Json(CommonViewModel);
				}
				else if (_context.Using<Survey>().Any(x => x.Id == Id))
				{
					var obj = _context.Using<Survey>().GetByCondition(x => x.Id == Id).FirstOrDefault();
					obj.IsActive = false;
					obj.IsDeleted = true;
					_context.Using<Survey>().Update(obj);
					//_context.Entry(obj).State = EntityState.Deleted;
					//_context.SaveChanges();

					CommonViewModel.IsConfirm = true;
					CommonViewModel.IsSuccess = true;
					CommonViewModel.StatusCode = ResponseStatusCode.Success;
					CommonViewModel.Message = ResponseStatusMessage.Delete;

					CommonViewModel.RedirectURL = Url.Action("Index", "Survey");

					return Json(CommonViewModel);
				}
			}
			catch (Exception ex) { }

			CommonViewModel.IsSuccess = false;
			CommonViewModel.StatusCode = ResponseStatusCode.Error;
			CommonViewModel.Message = ResponseStatusMessage.Unable_Delete;

			return Json(CommonViewModel);
		}

	}
}