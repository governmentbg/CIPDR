using System.ComponentModel;
using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using URegister.Core.Contracts;
using URegister.Core.Models.Service;
using URegister.Core.Services;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Model.RegisterForms;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace URegister.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRoles.Admin)]
    [Display(Name = "Формули за изчисления")]
    public class FieldFormulaController(
        //IServiceService serviceService,
        IFormConfigurationPersistenceService formConfigurationPersistenceService,
        IFieldFormulaCalculationService service
    ) : BaseController
    {
        [Display(Name = "Зареждане на формули за изчисления")]
        public async Task<IActionResult> Index(int formParentId)
        {
            FormViewModel dbForm = await formConfigurationPersistenceService.GetFormViewModel(formParentId, true);
            ViewData["FormParentId"] = formParentId;
            ViewData["FormTitle"] = dbForm.FormTitle;
            return View();
        }

        [Display(Name = "Зареждане на форма за добавяне на нова формула")]
        public async Task<IActionResult> Add(int formParentId)
        {
            var model = new FieldFormulaVM()
            {
                FormParentId = formParentId
            };
            await FillEditViewBags(formParentId);
            return View(nameof(Edit), model);
        }

        [Display(Name = "Зареждане на форма за редакция на формула")]
        public async Task<IActionResult> Edit(int id, int formParentId)
        {
            var model = await service.GetFormulaModel(id);
            await FillEditViewBags(formParentId, id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Display(Name = "Запис на форма за формула")]
        public async Task<IActionResult> Edit(FieldFormulaVM model)
        {
            if (!ModelState.IsValid)
            {
                await FillEditViewBags(model.FormParentId, model.Id);
                return View(model);
            }

            SaveOperationResult saveResult = await service.Save(model);

            if (saveResult.IsSuccess)
            {
                SetSuccessMessage(MessageConstant.SuccessMessage);

                return RedirectToAction(nameof(Index), new {formParentId = model.FormParentId});
            }
            else
            {
                SetErrorMessage(saveResult.ErrorMessage);
                await FillEditViewBags(model.FormParentId, model.Id);
                return View(model);
            }
        }

        [HttpPost]
        [Display(Name = "Смяна на подреждане")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PriorityUp(int id)
        {
            await service.ChangePriority(id, true);
            return Json("OK");
        }
        [HttpPost]
        [Display(Name = "Смяна на подреждане")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PriorityDown(int id)
        {
            await service.ChangePriority(id, false);
            return Json("OK");
        }

        private async Task FillEditViewBags(int formParentId, int editedId = 0)
        {
            FormViewModel formViewModel = await formConfigurationPersistenceService.GetFormViewModel(formParentId, true);

            List<SelectListItem> targetFieldDdl = new List<SelectListItem>(){new SelectListItem(string.Empty, string.Empty)};

            var existingFormulas = await service.GetListOfModels(formParentId).ToListAsync();

            foreach (FormField formField in formViewModel.FormFields)
            {
                await AddTargetFieldDdlItem(formField, existingFormulas, targetFieldDdl, editedId);

                foreach (FormField subField in formField.Fields)
                {
                    await AddTargetFieldDdlItem(subField, existingFormulas, targetFieldDdl, editedId,formField.Label + "->");
                }
            }

            ViewBag.TargetField_Ddl = targetFieldDdl;
        }

        /// <summary>
        /// Излвичане на полета кандидати за променливи във формула
        /// </summary>
        /// <param name="formParentId">Идентификатор на родителската форма</param>
        /// <returns></returns>
        [HttpGet]
        [Display(Name = "Излвичане на полета кандидати за променливи във формула")]
        public async Task<IActionResult> GetFormulaMemberCandidates(int formParentId)
        {
            FormViewModel formViewModel = await formConfigurationPersistenceService.GetFormViewModel(formParentId, true);

            List<SelectListItem> formulaMembersDdl = new List<SelectListItem>();

            foreach (FormField formField in formViewModel.FormFields)
            {
                AddFormulaMemberDdlItem(formField, formulaMembersDdl);

                foreach (FormField subField in formField.Fields)
                {
                    AddFormulaMemberDdlItem(subField, formulaMembersDdl, formField.Label + "->");
                }
            }

            //Полета които може да участват във формули
            //ViewBag.FormulaMembers_Ddl = formulaMembersDdl;

            return Json(formulaMembersDdl);
        }


        private async Task AddTargetFieldDdlItem(FormField formField, 
            List<FieldFormulaVM> existingFormulas, 
            List<SelectListItem> targetFieldDdl, 
            int editedId,
            string parentPrefix = "")
        {
            FieldFormulaVM editedModel = null;
            if (editedId != 0)
            {
                editedModel = await service.GetFormulaModel(editedId);
            }

            if ((formField.Type is nameof(SimpleFormFieldType.Number)
                    or nameof(SimpleFormFieldType.BulgarianCurrency)) &&
                (formField.IsReadonly || formField.IsHidden) &&
                (existingFormulas.All(f => f.TargetField != formField.Name) || (editedModel != null && formField.Name == editedModel.TargetField)))
            {
                targetFieldDdl.Add(new SelectListItem( parentPrefix + formField.Label, formField.Name));
            }
        }
        
        private static void AddFormulaMemberDdlItem(FormField formField, List<SelectListItem> formulaMembersDdl,
            string parentPrefix = "")
        {
            if (formField.Type is nameof(SimpleFormFieldType.Number)
                    or nameof(SimpleFormFieldType.BulgarianCurrency))
            {
                formulaMembersDdl.Add(new SelectListItem( parentPrefix + formField.Label, formField.Name));
            }
        }

        /// <summary>
        /// Списък на  услуги
        /// </summary>
        /// <param name="request"></param>
        /// <param name="formParentId"></param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Извличане на списък с формули")]
        public async Task<IActionResult> GetList(IDataTablesRequest request, int formParentId)
        {
            return await service.GetList(request, formParentId);
        }

        /// <summary>
        /// Изтриване на банка
        /// </summary>
        /// <param name="id">Идентификатор на номенклатура</param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Изтриване на формула")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTemplate(int id)
        {
            var result = await service.Delete(id);
            if (result.IsSuccess)
            {
                SetSuccessMessage("Формулата е изтрита успешно");
            }
            else
            {
                SetErrorMessage(result.ErrorMessage);
            }

            return Json(null);
        }
    }
}
