using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using URegister.Core.Contracts;
using URegister.Core.Models.Service;
using URegister.Infrastructure.Constants;

namespace URegister.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRoles.Admin)]
    [Display(Name = "Бланки на публични полета")]
    public class PublicFieldTemplateController(
        IPublicFieldTemplateService service,
        IServiceService serviceService,
        IFormConfigurationPersistenceService formConfigurationPersistenceService
    ) : BaseController
    {
        [Display(Name = "Зареждане на страница на публични полета")]
        public IActionResult Index()
        {
            return View();
        }

        [Display(Name = "Зареждане на форма за добавяне на нова бланка")]
        public async Task<IActionResult> Add()
        {
            var model = new PublicFieldTemplateVM();
            return View(nameof(Edit), model);
        }

        [Display(Name = "Зареждане на форма за редакция на бланка")]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await service.GetTemplate(id);
            return View(model);
        }
        [Display(Name = "Зареждане на форма за редакция на съдържание на бланка")]
        public async Task<IActionResult> EditContent(int id)
        {
            var model = await service.GetTemplate(id);
            return View(model);
        }
        /// <summary>
        /// Запис на  тип услуга
        /// </summary>
        /// <param name="model">Модел на услуга</param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Запис или редакция на бланка")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PublicFieldTemplateVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await service.AppendUpdate(model);
                    SetSuccessMessage(model.Id > 0 ? "Успешно добавена бланка" : "Успешна редакция на бланка");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    SetErrorMessage("Проблем при запис!");
                }
            }
            else
            {
                SetErrorMessage("Невалидни данни!");

            }
            return View(model);
        }
        /// <summary>
        /// Запис на  тип услуга
        /// </summary>
        /// <param name="model">Модел на услуга</param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Запис на съдържание на бланка")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditContent(PublicFieldTemplateVM model)
        {
            try
            {
                model.Content = model.ContentText;
                await service.AppendUpdateContent(model);
                SetSuccessMessage("Успешна записана бланка");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                SetErrorMessage("Проблем при запис!");
            }
            return View(model);
        }

        [Display(Name = "Извличане на полета за формуляр на бланка")]
        public async Task<JsonResult> GetFormFields()
        {
            var registerService = await serviceService.GetRegisterService();
            var formModel = await formConfigurationPersistenceService.GetFormViewModel(registerService.FormParentId);
            var paramList = await service.GetTemplateParam(formModel, string.Empty);
            return Json(paramList);
        }

        /// <summary>
        /// Списък на  услуги
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Извличане на списък с бланки")]
        public async Task<IActionResult> GetTemplateList(IDataTablesRequest request)
        {
            return await service.GetTemplateList(request);
        }

        /// <summary>
        /// Изтриване на банка
        /// </summary>
        /// <param name="id">Идентификатор на номенклатура</param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Изтриване на бланка")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTemplate(int id)
        {
            var result = await service.DeleteTemplate(id);
            if (result.IsSuccess)
            {
                SetSuccessMessage("Бланката е изтрита успешно");
            }
            else
            {
                SetErrorMessage(result.ErrorMessage);
            }

            return Json(null);
        }

        [HttpPost]
        [Display(Name = "Смяна на подреждане")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OrderNumUp(int id)
        {
            await service.OrderNumChange(id, true);
            return Json("OK");
        }
        [HttpPost]
        [Display(Name = "Смяна на подреждане")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OrderNumDown(int id)
        {
            await service.OrderNumChange(id, false);
            return Json("OK");
        }
    }
}
