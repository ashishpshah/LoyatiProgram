using Seed_Admin.Controllers;
using Seed_Admin.Infra;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Newtonsoft.Json.Linq;

namespace Seed_Admin.Areas.Admin.Controllers
{

	public class LoyaltyPointController : BaseController<ResponseModel<LoyaltyPointViewModel>>
	{
		public LoyaltyPointController(IRepositoryWrapper repository) : base(repository) { }

		// GET: Admin/LoyaltyPoint
		public ActionResult Index()
		{
			return View(CommonViewModel);
		}


		[HttpGet]
		public IActionResult GetData(JqueryDatatableParam param)
		{
			var result = new PagedResult();

			try
			{
				var listQrcode = _context.Using<LoyaltyPointsQrcode>().GetAll();

				var loyaltyDict = _context.Using<LoyaltyPoint>().GetAll().ToDictionary(x => x.QrcodeId);

				var userIds = loyaltyDict.Select(x => x.Value.UserId).Distinct().ToList();
				var userDict = _context.Using<User>().GetByCondition(x => userIds.Contains(x.Id)).ToDictionary(x => x.Id);

				var list = listQrcode.Select(x =>
				{
					loyaltyDict.TryGetValue(x.Id, out var lp);
					userDict.TryGetValue(lp?.UserId ?? 0, out var user);

					return new LoyaltyPointViewModel
					{
						QrCodeId = x.Id,
						QrCode_Base64 = x.QRCode_Base64,
						QrCode = x.Qrcode,
						Points = x.Points,
						IsClaimed = x.IsScanned,

						UserId = lp?.UserId ?? 0,
						ClaimedBy = user?.PersonName ?? "",
						ClaimedDate_Ticks = lp?.EarnedDateTime.Ticks ?? 0,
						GenerateDate_Ticks = x.CreatedDate?.Ticks ?? 0,
						ClaimedDate_Text = lp?.EarnedDateTime.ToString("dd/MM/yyyy HH:mm:ss").Replace("-", "/") ?? "",
						GenerateDate_Text = x.CreatedDate?.ToString("dd/MM/yyyy HH:mm:ss").Replace("-", "/") ?? ""
					};
				}).ToList();

				var recordsTotal = list.Count();
				IEnumerable<LoyaltyPointViewModel> query = list;

				// Filter (Search)
				if (!string.IsNullOrWhiteSpace(param.sSearch))
				{
					query = query.Where(x =>
						(x.QrCode?.Contains(param.sSearch, StringComparison.OrdinalIgnoreCase) ?? false) ||
						(x.ClaimedBy?.Contains(param.sSearch, StringComparison.OrdinalIgnoreCase) ?? false) ||
						x.Points.ToString().Contains(param.sSearch) ||
						x.ClaimedDate_Text.Contains(param.sSearch) ||
						x.GenerateDate_Text.Contains(param.sSearch)
					);
				}

				// Sort
				string sortColumn = HttpContext.Request.Query.ContainsKey("iSortCol_0") && HttpContext.Request.Query.ContainsKey($"mDataProp_{HttpContext.Request.Query["iSortCol_0"]}") ? Convert.ToString(HttpContext.Request.Query[$"mDataProp_{HttpContext.Request.Query["iSortCol_0"]}"]) : "";

				query = sortColumn?.ToLower() switch
				{
					"qrcode" => Convert.ToString(HttpContext.Request.Query["sSortDir_0"]).ToLower() == "asc" ? query.OrderBy(x => x.QrCode) : query.OrderByDescending(x => x.QrCode),
					"points" => Convert.ToString(HttpContext.Request.Query["sSortDir_0"]).ToLower() == "asc" ? query.OrderBy(x => x.Points) : query.OrderByDescending(x => x.Points),
					"generatedate_text" => Convert.ToString(HttpContext.Request.Query["sSortDir_0"]).ToLower() == "asc" ? query.OrderBy(x => x.GenerateDate_Ticks) : query.OrderByDescending(x => x.GenerateDate_Ticks),
					"isclaimed_text" => Convert.ToString(HttpContext.Request.Query["sSortDir_0"]).ToLower() == "asc" ? query.OrderBy(x => x.IsClaimed) : query.OrderByDescending(x => x.IsClaimed),
					"claimedby" => Convert.ToString(HttpContext.Request.Query["sSortDir_0"]).ToLower() == "asc" ? query.OrderBy(x => x.ClaimedBy) : query.OrderByDescending(x => x.ClaimedBy),
					"claimeddate_text" => Convert.ToString(HttpContext.Request.Query["sSortDir_0"]).ToLower() == "asc" ? query.OrderBy(x => x.ClaimedDate_Ticks) : query.OrderByDescending(x => x.ClaimedDate_Ticks),
					_ => query.OrderByDescending(x => x.GenerateDate_Ticks)
				};

				// Pagination
				var pagedData = param.iDisplayLength > -1 ? query.Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList() : query.ToList();

				return Json(new { param.sEcho, iTotalRecords = pagedData.Count(), iTotalDisplayRecords = recordsTotal, aaData = pagedData }, new System.Text.Json.JsonSerializerOptions());
			}
			catch (Exception ex) { }

			return Json(new { param.sEcho, iTotalRecords = 0, iTotalDisplayRecords = 0, aaData = new JArray() }, new System.Text.Json.JsonSerializerOptions());
		}


		//[CustomAuthorizeAttribute(AccessType_Enum.Read)]
		public ActionResult Partial_AddEditForm(int NoOfRows = 1, int minValue = 10, int maxValue = 35)
		{
			List<(string QRCode, string QRCode_Base64, int Point)> list = new List<(string QRCode, string QRCode_Base64, int Point)>();

			long tick = DateTime.UtcNow.Ticks;

			for (int i = 1; i <= NoOfRows; i++)
			{
				string qrText = $"{tick}-{i}";
				string base64Image = Common.GenerateQrAsBase64(qrText);

				list.Add((qrText, base64Image, new Random().Next(minValue, maxValue)));

			}

			CommonViewModel.Data = list;

			return PartialView("_Partial_AddEditForm", CommonViewModel);
		}

		[HttpPost]
		//[CustomAuthorizeAttribute(AccessType_Enum.Write)]
		public ActionResult Save(List<LoyaltyPointsQrcode> listQrcode)
		{
			try
			{
				if (listQrcode != null)
				{
					#region Validation

					if (!Common.IsAdmin())
					{
						CommonViewModel.IsSuccess = false;
						CommonViewModel.StatusCode = ResponseStatusCode.Error;
						CommonViewModel.Message = ResponseStatusMessage.UnAuthorize;

						return Json(CommonViewModel);
					}

					if (listQrcode.Count() <= 0)
					{
						CommonViewModel.IsSuccess = false;
						CommonViewModel.StatusCode = ResponseStatusCode.Error;
						CommonViewModel.Message = "Please enter data of QR Code.";

						return Json(CommonViewModel);
					}

					var normalizedQrStrings = listQrcode.Select(z => z.Qrcode.ToLower().Replace(" ", "")).ToList();

					var existingNormalizedQrs = _context.Using<LoyaltyPointsQrcode>().GetByCondition(q => normalizedQrStrings.Contains(q.Qrcode)).Select(x => x.Qrcode.ToLower().Replace(" ", "")).ToList();

					if (existingNormalizedQrs != null && existingNormalizedQrs.Count() > 0)
					{
						CommonViewModel.IsSuccess = false;
						CommonViewModel.StatusCode = ResponseStatusCode.Error;
						CommonViewModel.Message = "QR Code " + string.Join(", ", existingNormalizedQrs.ToArray()) + " already exist. Please try another QR Code.";

						return Json(CommonViewModel);
					}

					#endregion

					#region Database-Transaction

					using (var transaction = _context.BeginTransaction())
					{
						try
						{
							foreach (var item in listQrcode)
							{
								item.QRCode_Base64 = Common.GenerateQrAsBase64(item.Qrcode);

								var _viewModel = _context.Using<LoyaltyPointsQrcode>().Add(item);
								item.Id = _viewModel.Id;
							}

							CommonViewModel.IsConfirm = true;
							CommonViewModel.IsSuccess = true;
							CommonViewModel.StatusCode = ResponseStatusCode.Success;
							CommonViewModel.Message = ResponseStatusMessage.Success;
							CommonViewModel.RedirectURL = Url.Action("Index", "LoyaltyPoint");

							transaction.Commit();

							return Json(CommonViewModel);
						}
						catch (Exception ex) { transaction.Rollback(); }
					}

					#endregion
				}
			}
			catch (Exception ex) { }

			CommonViewModel.Message = ResponseStatusMessage.Error;
			CommonViewModel.IsSuccess = false;
			CommonViewModel.StatusCode = ResponseStatusCode.Error;

			return Json(CommonViewModel);
		}

		[HttpPost]
		//[CustomAuthorizeAttribute(AccessType_Enum.Delete)]
		public ActionResult DeleteConfirmed(long Id)
		{
			try
			{
				if (!Common.IsAdmin())
				{
					CommonViewModel.IsSuccess = false;
					CommonViewModel.StatusCode = ResponseStatusCode.Error;
					CommonViewModel.Message = ResponseStatusMessage.UnAuthorize;

					return Json(CommonViewModel);
				}

				if (Common.IsAdmin() && _context.Using<LoyaltyPoint>().Any(x => x.Id == Id))
				{
					var obj = _context.Using<LoyaltyPoint>().GetByCondition(x => x.Id == Id).FirstOrDefault();

					_context.Using<LoyaltyPoint>().Update(obj);
					//_context.Entry(obj).State = EntityState.Deleted;
					//_context.SaveChanges();

					CommonViewModel.IsConfirm = true;
					CommonViewModel.IsSuccess = true;
					CommonViewModel.StatusCode = ResponseStatusCode.Success;
					CommonViewModel.Message = ResponseStatusMessage.Delete;

					CommonViewModel.RedirectURL = Url.Action("Index", "LoyaltyPoint");

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