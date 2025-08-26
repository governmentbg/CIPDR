using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using URegister.Core.Contracts;
using URegister.Core.Services;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Model.RegisterForms;
using DataTables.AspNet.Core;
using System.ComponentModel.DataAnnotations;

namespace URegister.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.GlobalAdmin}")]
    [Display(Name = "Каталози")]
    public class CatalogController : BaseController
    {
        private readonly IFormConfigurationPersistenceService _formService;
        private readonly ILogger<CatalogController> _logger;
        private readonly IRegisterService _registerService;

        public CatalogController(IFormConfigurationPersistenceService formService, 
            ILogger<CatalogController> logger,
            IRegisterService registerService)
        {
            _formService = formService;
            _logger = logger;
            _registerService = registerService;
        }

        /// <summary>
        /// Списък с формите в регистъра
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = $"{UserRoles.GlobalAdmin}")]
        [Display(Name = "Зареждане на списък с формите в регистъра")]
        public IActionResult FormIndex()
        {
            return View();
        }

        /// <summary>
        /// Зареждане на формите от регистъра
        /// </summary>
        /// <returns></returns>
        [Display(Name = "Извличане на списък с форми от регистъра")]
        public async Task<IActionResult> GetForms()
        {
            var formsFromDb = await _formService.GetForms(await _registerService.GetCurrentRegisterId());
            var isGlobalAdmin = User.IsInRole(UserRoles.GlobalAdmin);
            return Json(new { data = formsFromDb, isGlobalAdmin });
        }

        /// <summary>
        /// Зареждане на формите от регистъра
        /// </summary>
        /// <returns></returns>
        [Display(Name = "Извличане на списък с форми за контролното табло в регистъра")]
        public async Task<IActionResult> GetFormListDashboard(IDataTablesRequest request, int approvalStatus)
        {
            var formsFromDb = await _formService.GetFormListDashboard(request, await _registerService.GetCurrentRegisterId(), approvalStatus);

            return formsFromDb;
        }

        /// <summary>
        /// Редакция или добавяне на форма от регистър
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Добавяне или редакция на форма в регистъра")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditForm(AddFormViewModel model)
        {
            if (model.ParentId > 0)
            {
                ViewData["Title"] = "Редакция на форма";
            }
            else
            {
                ViewData["Title"] = "Добавяне на форма";
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.FormTitle))
            {
                ModelState.AddModelError(nameof(model.FormTitle), MessageConstant.FieldIsRequiredNoParam);
            }
            else 
            {
                model.FormTitle = model.FormTitle.Trim();
                if (!Regex.IsMatch(model.FormTitle, RegexPatterns.CyrillicTextPattern))
                {
                    ModelState.AddModelError(nameof(model.FormTitle), MessageConstant.NotCyrillic);
                }
            }

            if (string.IsNullOrWhiteSpace(model.Purpose))
            {
                ModelState.AddModelError(nameof(model.Purpose), MessageConstant.FieldIsRequiredNoParam);
            }
            else
            {
                model.Purpose = model.Purpose.Trim();
                if(!Regex.IsMatch(model.Purpose, RegexPatterns.CyrillicTextPattern))
                {
                    ModelState.AddModelError(nameof(model.Purpose), MessageConstant.NotCyrillic);
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            SaveOperationResult result = model.ParentId > 0 ? await _formService.EditForm(model) : await _formService.SaveForm(model);

            if (result.IsSuccess)
            {
                SetSuccessMessage("Формата е записана успешно");
                if (model.ParentId > 0)
                {
                    return RedirectToAction(nameof(FormIndex), "Catalog");
                }
                else
                {
                    return RedirectToAction("Index", "Designer", new { formParentId = result.AddedObjectId });
                }
            }

            SetErrorMessage(result.ErrorMessage);
            return View(model);
        }

        /// <summary>
        /// Редакция или добавяне на форма от регистър
        /// </summary>
        /// <param name="formParentId">Идентификатор на първата версия на формата</param>
        /// <returns></returns>
        [HttpGet]
        [Display(Name = "Зареждане на форма за добавяне или редакция на форма")]
        public async Task<IActionResult> EditForm(int formParentId = 0)
        {
            AddFormViewModel model = new AddFormViewModel(); ;
            if (formParentId > 0)
            {              
                FormViewModel dbForm = await _formService.GetFormViewModel(formParentId, true);
                model.FormTitle = dbForm.FormTitle;
                model.ParentId = dbForm.FormParentId;
                model.Purpose = dbForm.Purpose;

                ViewData["Title"] = "Редакция на форма";
            }
            else
            {
                ViewData["Title"] = "Добавяне на форма";
            }

            return View(model);
        }

        /// <summary>
        /// Изтриване на форма от регистъра
        /// </summary>
        /// <param name="id">Идентификатор на форма</param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Изтриване на форма от регистъра")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteForm(int id)
        {
            var form = await _formService.GetFormById(id);
            if (form == null) 
            {
                SetErrorMessage("Формата не е намерена.");
                return Json(new { success = false });
            }

            if (form.ApprovalStatus == (int)ApprovalStatus.Approved && !User.IsInRole(UserRoles.GlobalAdmin))
            {
                SetErrorMessage("Нямате права за изтриване на одобрена форма.");
                return Unauthorized();
            }

            OperationResult result = await _formService.DeleteForm(id);

            if (result.IsSuccess)
            {
                SetSuccessMessage("Формата е изтрита успешно");
            }
            else
            {
                SetErrorMessage(result.ErrorMessage);
            }

            return Json(null);
        }

        /// <summary>
        /// Списък с формите в регистъра
        /// </summary>
        /// <returns></returns>
        [Display(Name = "Зареждане на списък с потребителски справки")]
        public IActionResult CustomViewsIndex()
        {
            return View();
        }

        /// <summary>
        /// Зареждане на потребителските справки
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = UserRoles.Admin)]
        [Display(Name = "Извличане на списък с потребителски справки")]
        public async Task<IActionResult> GetCustomViews()
        {
            try
            {
                var customViews = await _formService.GetCustomViews();
                return Json(new { data = customViews });
            }
            catch (Exception ex)
            {
                SetErrorMessage("Проблем при извличане на потребитлеските");
                return Json(new object());
            }
        }

        /// <summary>
        /// Редакция или добавяне на потребителска справка
        /// </summary>
        /// <param name="id">Идентификатор на потребителска справка. 0 при създаване на нова</param>
        /// <returns></returns>
        [HttpGet]
        [Display(Name = "Зареждане на форма за добавяне или редакция на потребителска справка")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> UpsertCustomView(int id = 0)
        {
            await SetCustomViewViewBag();

            if (id == 0)
            {
                CustomViewViewModel model = new CustomViewViewModel();
                return View(model);
            }

            CustomViewViewModel dbModel = await _formService.GetCustomViewViewModel(id);

            return View(dbModel);
        }

        private async Task SetCustomViewViewBag()
        {
            Dictionary<string, string> ddl = await _formService.CustomViewColumns();
            ViewBag.SelectedColumns_ddl = ddl.Select(i => new SelectListItem(i.Value, i.Key));
        }

        /// <summary>
        /// Редакция или добавяне на потребителска справка
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Добавяне или редакция на потребителска справка")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> UpsertCustomView(CustomViewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await SetCustomViewViewBag();
                return View(model);
            }

            model.CustomViewTitle = model.CustomViewTitle!.Trim();

            SaveOperationResult result = await _formService.UpsertCustomView(model);

            if (result.IsSuccess)
            {
                SetSuccessMessage("Потребителската справка е записана успешно");
                return RedirectToAction(nameof(CustomViewsIndex), "Catalog");
            }

            SetErrorMessage(result.ErrorMessage);
            await SetCustomViewViewBag();
            return View(model);
        }

        /// <summary>
        /// Изтриване на потребителска справка
        /// </summary>
        /// <param name="id">Идентификатор на потребителска справка</param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Изтриване на потребителска справка")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> DeleteCustomView(int id)
        {
            OperationResult result = await _formService.DeleteCustomView(id);

            if (result.IsSuccess)
            {
                SetSuccessMessage("Потребителската справка е изтрита успешно");
            }
            else
            {
                SetErrorMessage(result.ErrorMessage);
            }

            return Json(null);
        }
    }
}
