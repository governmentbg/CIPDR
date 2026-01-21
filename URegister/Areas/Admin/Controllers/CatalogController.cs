using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using URegister.Common;
using URegister.Core.Contracts;
using URegister.Core.Services;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Model.RegisterForms;
using URegister.NomenclaturesCatalog;

namespace URegister.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.GlobalAdmin}")]
    [Display(Name = "Каталози")]
    public class CatalogController : BaseController
    {
        private readonly IFormConfigurationPersistenceService _formService;
        private readonly NomenclatureGrpc.NomenclatureGrpcClient _nomenclatureGrpcClient;
        private readonly ILogger<CatalogController> _logger;
        private readonly IRegisterService _registerService;

        public CatalogController(IFormConfigurationPersistenceService formService,
            NomenclatureGrpc.NomenclatureGrpcClient nomenclatureGrpcClient,
            ILogger<CatalogController> logger,
            IRegisterService registerService)
        {
            _formService = formService;
            _nomenclatureGrpcClient = nomenclatureGrpcClient;
            _logger = logger;
            _registerService = registerService;
        }

        /// <summary>
        /// Списък с формите в регистъра
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.GlobalAdmin}")]
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
        [Authorize(Roles = UserRoles.GlobalAdmin)]
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
       
        /// <summary>
        /// Зареждане на условия към форма
        /// </summary>
        /// <param name="formParentId">Идентификатор на родителска форма</param>
        /// <returns>JSON response with resolved form conditions</returns>
        [Display(Name = "Извличане на списък с условия към форма")]
        public async Task<IActionResult> GetFormConditions(int formParentId)
        {
            // Fetch form conditions and form view model
            var formConditionsFromDb = await _formService.GetFormConditions(formParentId);
            FormViewModel dbForm = await _formService.GetFormViewModel(formParentId, true);

            // Exit early if no conditions or form fields are found
            if (!formConditionsFromDb.Any() || dbForm?.FormFields == null)
            {
                _logger.LogWarning($"Не са намерени условия или полета към форма с formParentId {formParentId} в {nameof(GetFormConditions)}");
                return Json(new { data = formConditionsFromDb });
            }

            // Cache form fields for efficient lookup
            var formFieldDict = dbForm.FormFields.ToDictionary(f => f.Name, f => f);

            // Collect unique nomenclature types
            var nomenclatureTypes = new HashSet<string>();
            foreach (var formCondition in formConditionsFromDb)
            {
                if (formFieldDict.TryGetValue(formCondition.TriggeringFieldName, out var triggeringField))
                {                                   
                    if (!string.IsNullOrEmpty(triggeringField.NomenclatureType))
                    {
                        nomenclatureTypes.Add(triggeringField.NomenclatureType);
                    }
                }
                else
                {
                    _logger.LogWarning($"Полето активиращо условие {formCondition.TriggeringFieldName} не е намерено към форма с formParentId {formParentId}");
                }
            }

            // Fetch nomenclature data if there are nomenclature types to resolve
            NomenclaturePublicResponse nomenclatureResult = null;
            if (nomenclatureTypes.Any())
            {
                var getNomenclaturesRequest = new NomenclaturePublicRequest
                {
                    RegisterId = 0
                };
                getNomenclaturesRequest.NomenclatureTypes.AddRange(nomenclatureTypes);

                try
                {
                    nomenclatureResult = await _nomenclatureGrpcClient.GetNomenclaturePublicAsync(getNomenclaturesRequest);
                    if (nomenclatureResult.ResultStatus.Code != ResultCodes.Ok)
                    {
                        _logger.LogError($"Неуспех на GetNomenclaturePublicAsync в {nameof(GetFormConditions)}: {nomenclatureResult.ResultStatus.Message}");
                        return StatusCode(500, new { error = "Неуспешно извличане на данни за номенклатури" });
                    }

                    if (!nomenclatureResult.NomenclatureTypes.Any())
                    {
                        _logger.LogWarning($"Няма резултати за номенклатурни типове: {string.Join(", ", nomenclatureTypes)} в {nameof(GetFormConditions)}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Грешка при извличане на данни за номенклатури в {nameof(GetFormConditions)}");
                    return StatusCode(500, new { error = "Грешка при извличане на данни за номенклатури" });
                }
            }

            // Resolve nomenclature values, fields to hide and triggering field name
            foreach (var formCondition in formConditionsFromDb)
            {
                if (formFieldDict.TryGetValue(formCondition.TriggeringFieldName, out var triggeringField))
                {
                    // Resolve nomenclature value if applicable
                    if (nomenclatureResult != null && !string.IsNullOrEmpty(triggeringField.NomenclatureType))
                    {
                        var nomenclatureType = nomenclatureResult.NomenclatureTypes
                            .FirstOrDefault(nt => nt.Type == triggeringField.NomenclatureType);
                        if (nomenclatureType != null)
                        {
                            var concept = nomenclatureType.CodeableConcepts
                                .FirstOrDefault(c => c.Code == formCondition.TriggeringNomenclatureValue);
                            formCondition.TriggeringNomenclatureValue = concept?.Value ?? formCondition.TriggeringNomenclatureValue; // Fallback to original value if not found
                        }
                    }
                    formCondition.TriggeringFieldName = triggeringField.Label;
                }
                else
                {
                    _logger.LogWarning($"Полето активиращо условие {formCondition.TriggeringFieldName} не е намерено към форма с formParentId {formParentId}");
                }

                // Resolve FieldsToHide
                var fieldsToHideArr = formCondition.FieldsToHide?.Split(';', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                var resolvedFieldsToHideNamesArr = new List<string>();
                foreach (var fieldToHide in fieldsToHideArr)
                {
                    if (formFieldDict.TryGetValue(fieldToHide, out var resolvedFieldToHide))
                    {
                        resolvedFieldsToHideNamesArr.Add(resolvedFieldToHide.Label);
                    }
                    else
                    {
                        resolvedFieldsToHideNamesArr.Add(fieldToHide);
                        _logger.LogWarning($"Полето за криене {fieldToHide} не е намерено към форма с formParentId {formParentId}");
                    }
                }
                formCondition.FieldsToHide = string.Join("; ", resolvedFieldsToHideNamesArr);
            }

            return Json(new { data = formConditionsFromDb });
        }

        /// <summary>
        /// Зареждане на списък с условия към форма
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Display(Name = "Зареждане на списък с условия към форма")]
        public async Task<IActionResult> FormConditions(int formParentId)
        {
            FormViewModel dbForm = await _formService.GetFormViewModel(formParentId, true);
            ViewData["FormParentId"] = formParentId;
            ViewData["FormTitle"] = dbForm.FormTitle;
            return View();
        }

        /// <summary>
        /// Редакция или добавяне на форма от регистър
        /// </summary>
        /// <param name="formParentId">Идентификатор на първата версия на формата</param>
        /// <param name="conditionId">Идентификатор на условие към форма. 0 при добавяне на ново</param>
        /// <returns></returns>
        [HttpGet]
        [Display(Name = "Зареждане на форма за добавяне или редакция на форма")]
        public async Task<IActionResult> EditFormCondition(int formParentId, int conditionId = 0)
        {
            AddConditionViewModel model;
            FormViewModel dbForm = await _formService.GetFormViewModel(formParentId, true);

            SetEditConditionViewBag(dbForm);

            if (conditionId > 0)
            {
                model = await _formService.GetFormConditionViewModel(conditionId);

                if (model == null)
                {
                    SetErrorMessage("Проблем при зареждане на условие");
                    model = new AddConditionViewModel();
                }

                ViewData["Title"] = $"Редакция на условие към '{dbForm.FormTitle}'";

                CodeableConceptListRequest request = new CodeableConceptListRequest
                {
                    DataTableRequest = new DatatableRequest { Length = -1 },
                    Type = dbForm.FormFields.SingleOrDefault(f => f.Name == model.TriggeringFieldName)?.NomenclatureType
                };
                try
                {
                    CodeableConceptListResponse response =
                        await _nomenclatureGrpcClient.GetCodeableConceptListAsync(request);

                    if (response.ResultStatus.Code != ResultCodes.Ok)
                    {
                        _logger.LogError($"Проблемен статус ({response.ResultStatus.Code}) на заявка в {nameof(CatalogController)}->{nameof(EditFormCondition)}");
                    }

                    ViewBag.TriggeringNomenclatureValue_ddl = response.Data.Select(c => new SelectListItem(c.Value, c.Code)).ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Грешка в {nameof(CatalogController)}->{nameof(EditFormCondition)}");
                    return new JsonResult(null);
                }
            }
            else
            {
                ViewData["Title"] = $"Добавяне на условие към '{dbForm.FormTitle}'";
                model = new AddConditionViewModel {FormParentId = formParentId};
            }

            return View(model);
        }

        /// <summary>
        /// Редакция или добавяне на форма от регистър
        /// </summary>
        /// <param name="formParentId">Идентификатор на първата версия на формата</param>
        /// <param name="triggeringValue">Името на полето източник на събитие</param>
        /// <returns></returns>
        [HttpGet]
        [Display(Name = "Зареждане на номенклатурните стойсноти за избраното поле източник на събитие")]
        public async Task<IActionResult> GetNomenclatureValuesForTriggeringValue(int formParentId, string triggeringValue)
        {
            AddConditionViewModel model;
            FormViewModel dbForm = await _formService.GetFormViewModel(formParentId, true);


            CodeableConceptListRequest request = new CodeableConceptListRequest
            {
                DataTableRequest = new DatatableRequest { Length = -1 },
                Type = dbForm.FormFields.SingleOrDefault(f => f.Name == triggeringValue)?.NomenclatureType
            };
            try
            {
                CodeableConceptListResponse response =
                    await _nomenclatureGrpcClient.GetCodeableConceptListAsync(request);

                if (response.ResultStatus.Code != ResultCodes.Ok)
                {
                    _logger.LogError($"Проблемен статус ({response.ResultStatus.Code}) на заявка в {nameof(CatalogController)}->{nameof(GetNomenclatureValuesForTriggeringValue)}");
                }

                return new JsonResult(response.Data.Select(c => new SelectListItem(c.Value, c.Code)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Грешка в {nameof(CatalogController)}->{nameof(GetNomenclatureValuesForTriggeringValue)}");
                return new JsonResult(null);
            }
        }

        private void SetEditConditionViewBag(FormViewModel model)
        {
            var placeholder = new SelectListItem("Изберете", "");

            var triggeringFieldNameDdl = model.FormFields.Where(f => f.Type == nameof(SimpleFormFieldType.Select))
                .Select(f => new SelectListItem(f.Label, f.Name)).ToList();

            triggeringFieldNameDdl.Insert(0, placeholder);

            ViewBag.TriggeringFieldName_ddl = triggeringFieldNameDdl;

            var fieldsDdl = model.FormFields.Select(f => new SelectListItem(f.Label, f.Name)).ToList();
            fieldsDdl.Insert(0, placeholder);

            ViewBag.FieldsToHide_ddl = fieldsDdl;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Display(Name = "Промяна на условие към форма")]
        public async Task<IActionResult> EditFormCondition(AddConditionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                SetErrorMessage("Невалидни стойности");
                return View(model);
            }

            SaveOperationResult result = await _formService.SaveFormCondition(model);

            if (result.IsSuccess)
            {
                SetSuccessMessage("Записът е успешен");
                return RedirectToAction(nameof(FormConditions), new { formParentId = model.FormParentId });
            }

            SetErrorMessage(result.ErrorMessage);
            return View(model);
        }

        //[HttpGet]
        //public async Task<IActionResult> GetConditionTreeForFormParentId(int formParentId)
        //{
        //    JsonResult formConditionTree = await _formService.GetConditionTreeForFormParentId(formParentId);
        //    return formConditionTree;
        //}

        /// <summary>
        /// Изтриване на условие към форма
        /// </summary>
        /// <param name="id">Идентификатор на условие</param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Изтриване на условие към форма")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFormCondition(int id)
        {
            if (id <= 0)
            {
                SetErrorMessage("Невалиден идентификатор на условие.");
                return StatusCode(400, new { success = false, message = "Невалиден идентификатор на условие." });
            }

            OperationResult result = await _formService.DeleteFormCondition(id);

            if (result.IsSuccess)
            {
                SetSuccessMessage("Условието е изтрито успешно.");
            }
            else
            {
                SetErrorMessage(result.ErrorMessage);
            }

            return Json(null);
        }
    }
}
