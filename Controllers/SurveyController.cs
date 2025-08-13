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

		//[CustomAuthorizeAttribute(AccessType_Enum.Read)]
		public ActionResult Partial_AddEditForm_Question(long SurveyId = 0, long Id = 0)
		{
			if (!(Common.IsSuperAdmin() || Common.IsAdmin()))
			{
				CommonViewModel.IsSuccess = false;
				CommonViewModel.StatusCode = ResponseStatusCode.Error;
				CommonViewModel.Message = ResponseStatusMessage.UnAuthorize;

				return Json(CommonViewModel);
			}

			if (SurveyId <= 0 || !_context.Using<Survey>().Any(x => x.Id == SurveyId))
			{
				CommonViewModel.IsSuccess = false;
				CommonViewModel.StatusCode = ResponseStatusCode.Error;
				CommonViewModel.Message = ResponseStatusMessage.NotFound;

				return Json(CommonViewModel);
			}

			CommonViewModel.Obj = _context.Using<Survey>().GetByCondition(x => x.Id == SurveyId).FirstOrDefault();

			CommonViewModel.Obj.StartDate_Text = CommonViewModel.Obj.StartDate.ToString(Common.DateTimeFormat_ddMMyyyy_HHmm);
			CommonViewModel.Obj.EndDate_Text = CommonViewModel.Obj.EndDate.ToString(Common.DateTimeFormat_ddMMyyyy_HHmm);

			CommonViewModel.SelectListItems = _context.Using<LovMaster>().GetByCondition(x => x.LovColumn.ToLower() == "question_type")
				.Select(x => new SelectListItem_Custom(x.LovCode, x.LovDesc, (x.DisplayOrder ?? 0), x.LovColumn.ToUpper())).ToList();

			CommonViewModel.Obj.Questions = _context.Using<Question>().GetByCondition(x => x.SurveyId == SurveyId).ToList();

			for (int i = 0; i < CommonViewModel.Obj.Questions.Count(); i++)
			{
				CommonViewModel.Obj.Questions[i].QuestionType_Text = CommonViewModel.SelectListItems.Where(x => x.Value == CommonViewModel.Obj.Questions[i].QuestionType).Select(x => x.Text).FirstOrDefault();
			}

			if (Id > 0)
			{
				int index = CommonViewModel.Obj.Questions.FindIndex(q => q.Id == Id);
				if (index != -1)
				{
					CommonViewModel.Obj.Question = CommonViewModel.Obj.Questions[index];

					if (CommonViewModel.Obj.Question.QuestionType == "MCQ" || CommonViewModel.Obj.Question.QuestionType == "RAT")
						CommonViewModel.Obj.Question.Options = _context.Using<QuestionOption>().GetByCondition(x => x.QuestionId == Id && x.IsDeleted == false).Select(x => new OptionDto() { Id = x.Id, Text = x.OptionText, Value = x.OptionValue }).ToList();

				}

				return Json(CommonViewModel.Obj.Question);
			}

			return PartialView("_Partial_AddEditForm_Question", CommonViewModel);
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
		public ActionResult SaveQuestion(Question viewModel)
		{
			if (viewModel == null || string.IsNullOrWhiteSpace(viewModel.QuestionText))
			{
				return Json(new { success = false, message = "Question Text is required." });
			}
			else if (viewModel != null && !string.IsNullOrWhiteSpace(viewModel.QuestionText) && string.IsNullOrWhiteSpace(viewModel.QuestionType))
			{
				return Json(new { success = false, message = "Question Type is required." });
			}
			else if (viewModel != null && !string.IsNullOrWhiteSpace(viewModel.QuestionText) && !string.IsNullOrWhiteSpace(viewModel.QuestionType) && viewModel.QuestionType.ToUpper() == "MCQ" && (viewModel.Options == null || viewModel.Options.Where(x => x.Text.ToUpper() != "MIN" && x.Text.ToUpper() != "MAX").Count() <= 0))
			{
				return Json(new { success = false, message = "Option(s) is required." });
			}
			else if (viewModel != null && !string.IsNullOrWhiteSpace(viewModel.QuestionText) && !string.IsNullOrWhiteSpace(viewModel.QuestionType) && viewModel.QuestionType.ToUpper() == "RAT" && (viewModel.Options == null || viewModel.Options.Where(x => x.Text.ToUpper() == "MIN" || x.Text.ToUpper() == "MAX").Count() <= 0))
			{
				return Json(new { success = false, message = "Min and Max Rating is required." });
			}
			else if (_context.Using<Question>().Any(x => x.QuestionText.ToLower().Replace(" ", "") == viewModel.QuestionText.ToLower().Replace(" ", "") && x.Id != viewModel.Id && x.SurveyId == viewModel.SurveyId))
			{
				return Json(new { success = false, message = "Question already exist. Please try another Question." });
			}

			using (var transaction = _context.BeginTransaction())
			{
				try
				{
					Question obj = _context.Using<Question>().GetByCondition(x => x.Id == viewModel.Id && x.SurveyId == viewModel.SurveyId).FirstOrDefault();

					if (obj != null)
					{
						obj.QuestionText = viewModel.QuestionText;
						obj.QuestionType = viewModel.QuestionType;

						obj.IsActive = true;
						obj.IsDeleted = obj.IsActive ? false : obj.IsDeleted;

						_context.Using<Question>().Update(obj);
					}
					else
					{
						viewModel.DisplayOrder = (_context.Using<Question>().GetByCondition(x => x.SurveyId == viewModel.SurveyId).OrderByDescending(x => x.DisplayOrder).Select(x => x.DisplayOrder).FirstOrDefault() ?? 0) + 1;
						viewModel.IsActive = true;
						viewModel.IsDeleted = viewModel.IsActive ? false : viewModel.IsDeleted;

						var _viewModel = _context.Using<Question>().Add(viewModel);
						viewModel.Id = _viewModel.Id;
					}

					if (viewModel.QuestionType.ToUpper() == "MCQ" || viewModel.QuestionType.ToUpper() == "RAT")
					{
						if (viewModel.Options != null && viewModel.Options.Count() > 0)
						{
							for (int i = 0; i < viewModel.Options.Count(); i++)
							{
								if (!string.IsNullOrWhiteSpace(viewModel.Options[i].Text))
								{
									var existingOption = _context.Using<QuestionOption>().GetByCondition(x => x.Id == viewModel.Options[i].Id && x.QuestionId == viewModel.Id).FirstOrDefault();

									if (existingOption != null)
									{
										existingOption.OptionText = viewModel.Options[i].Text;
										existingOption.OptionValue = viewModel.Options[i].Value ?? "";

										_context.Using<QuestionOption>().Update(existingOption);
									}
									else
									{
										var questionOption = new QuestionOption
										{
											QuestionId = viewModel.Id,
											OptionText = viewModel.Options[i].Text,
											OptionValue = viewModel.Options[i].Value ?? ""
										};

										var _viewModel = _context.Using<QuestionOption>().Add(questionOption);
										viewModel.Options[i].Id = _viewModel.Id;
									}
								}
							}

							var existingOptions = _context.Using<QuestionOption>().GetByCondition(x => x.QuestionId == viewModel.Id).ToList();

							foreach (var option in existingOptions)
								if (viewModel.Options != null && !viewModel.Options.Any(x => x.Id == option.Id))
									_context.Using<QuestionOption>().Delete(option);
						}
					}

					transaction.Commit();

					return Json(new { success = true });
				}
				catch (Exception ex) { transaction.Rollback(); }
			}

			return Json(new { success = false, message = ResponseStatusMessage.Error });
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