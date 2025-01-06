using Infrastructure.Constants;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using URegister.Core.Contracts;
using URegister.Core.Services;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Model.RegisterForms;

namespace URegister.Areas.Admin.Controllers
{
    [Area("Admin")]
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
        public IActionResult FormIndex()
        {
            return View();
        }

        /// <summary>
        /// Зареждане на формите от регистъра
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> GetForms()
        {
            var formsFromDb = await _formService.GetForms(await _registerService.GetCurrentRegisterId());

            return Json(new { data = formsFromDb});
        }

        /// <summary>
        /// Редакция или добавяне на форма от регистър
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditForm(AddFormViewModel model)
        {
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

            if (string.IsNullOrWhiteSpace(model.FormTitle))
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
        public async Task<IActionResult> EditForm(int formParentId = 0)
        {
            AddFormViewModel model = new AddFormViewModel(); ;
            if (formParentId > 0)
            {
                FormViewModel dbForm = await _formService.GetFormViewModel(formParentId);
                model.FormTitle = dbForm.FormTitle;
                model.ParentId = dbForm.FormParentId;
                model.Purpose = dbForm.Purpose;
            }

            return View(model);
        }

        /// <summary>
        /// Редакция или добавяне на форма от регистър
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteForm(int id)
        {
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
    }
}
