using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Primitives;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using URegister.Common;
using URegister.Core.Contracts;
using URegister.Core.Services;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Model.RegisterForms;
using URegister.NomenclaturesCatalog;
using URegister.ObjectsCatalog;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace URegister.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.GlobalAdmin}")]
    [Display(Name = "Дизайнер")]
    public class DesignerController : BaseController
    {
        private readonly IFormFieldsLayoutService formFieldsLayoutService;
        private readonly IFormValidationService formValidationService;
        private readonly NomenclatureGrpc.NomenclatureGrpcClient nomenclatureGrpcClient;
        private readonly ObjectsCatalogGrpc.ObjectsCatalogGrpcClient objectCatalogGrpcClient;
        private readonly IFormConfigurationPersistenceService _formConfigurationPersistenceService;
        private readonly IRegisterService registerService;
        public readonly ILogger<DesignerController> Logger;

        public DesignerController(IFormFieldsLayoutService formFieldsLayoutService,
            IFormValidationService formValidationService,
            NomenclatureGrpc.NomenclatureGrpcClient nomenclatureGrpcClient,
            ObjectsCatalogGrpc.ObjectsCatalogGrpcClient objectCatalogGrpcClient,
            IFormConfigurationPersistenceService formConfigurationPersistenceService,
            IRegisterService registerService,
            ILogger<DesignerController> logger)
        {
            this.formFieldsLayoutService = formFieldsLayoutService;
            this.formValidationService = formValidationService;
            this.nomenclatureGrpcClient = nomenclatureGrpcClient;
            this.objectCatalogGrpcClient = objectCatalogGrpcClient;
            this.registerService = registerService;
            _formConfigurationPersistenceService = formConfigurationPersistenceService;
            Logger = logger;
        }

        /// <summary>
        /// Дизайнер на форма от регистър
        /// </summary>
        /// <returns></returns>
        [Display(Name = "Отваряне на дизайнер за форма от регистър")]
        [HttpGet]
        public async Task<IActionResult> Index(int formParentId)
        {
            FormViewModel formViewModel = await _formConfigurationPersistenceService.GetFormViewModel(formParentId, true);
            if (formViewModel == null)
            {
                Logger.LogError($"Не е намерена форма с parentId {formParentId} в {nameof(Index)}");
                SetErrorMessage("Проблем при зареждане на форма");
                return View(new DesignerViewModel());
            }

            IEnumerable<CatalogFieldType> fieldTypes = await FieldTypeCatalogService.GetAllFieldType(objectCatalogGrpcClient);

            if (fieldTypes == null)
            {
                SetErrorMessage("Проблем при зареждане на типовете полета");
                Logger.LogError($"Проблем при зареждане на типовете полета в {nameof(Index)}");
                return View(formViewModel);
            }

            ViewBag.DesignerFieldTypes_ddl = fieldTypes.Select(t => new SelectListItem
            {
                Value = t.Type,
                Text = t.Label
            }).ToList();

            NomenclaturePublicRequest getNomenclaturesRequest = new NomenclaturePublicRequest
            {
               RegisterId = await registerService.GetCurrentRegisterId(),
            };

            try
            {
                NomenclatureTypeListPublicResponse nomenclatureResult =
                    await nomenclatureGrpcClient.GetNomenclatureTypeListPublicAsync(getNomenclaturesRequest);

                if (nomenclatureResult.ResultStatus.Code != ResultCodes.Ok)
                {
                    Logger.LogError($"GetNomenclaturePublicAsync неуспешен в {nameof(Index)}");
                    SetErrorMessage("Проблем при зареждане на номенклатурите");
                    ViewBag.NomenclatureTypes_ddl = new List<SelectListItem>();
                }

                ViewBag.NomenclatureTypes_ddl = nomenclatureResult.NomenclatureTypes.Select(nom =>
                        new SelectListItem(nom.Name, nom.Type))
                    .ToList();
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, $"Проблем при зареждане на номенклатурите в {nameof(Index)}");
                ViewBag.NomenclatureTypes_ddl = new List<SelectListItem>();
            }

            return View(formViewModel);
        }

        /// <summary>
        /// Запис на конфигурация на форма
        /// </summary>
        /// <param name="jsonFieldsModel">JSON конфигурация на формата</param>
        /// <param name="formParentId">Родителски идентификатор на формата</param>
        /// <param name="formTitle">Заглавие на формата</param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Запис на конфигурация на форма")]
        [ValidateAntiForgeryToken]
        public async Task<bool> SaveConfiguration(string jsonFieldsModel, int formParentId, string formTitle)
        {
            bool isApproved = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Any(c => c.Value == UserRoles.GlobalAdmin);

            bool result = await _formConfigurationPersistenceService.SaveDesignerJson(jsonFieldsModel, formParentId, formTitle, isApproved);
            SetSuccessMessage("Конфигурацията е записана успешно");
            return result;
        }

        /// <summary>
        /// Показване на изглед
        /// </summary>
        [HttpGet]
        [Display(Name = "Генериране на изглед на форма")]
        public async Task<IActionResult> ShowPreview(int formParentId)
        {
            FormViewModel viewModel = await _formConfigurationPersistenceService.GetFormViewModel(formParentId, true);
            viewModel.DontUploadFilesToStorage = true;

            if (viewModel == null || !viewModel.FormFields.Any())
            {
                SetErrorMessage("Проблем при генериране на страницата. Свържете се с администратор");
            }

            return View(viewModel);
        }

        /// <summary>
        /// Потвърждаване на формата с полета
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Display(Name = "Валидиране и преглед на форма")]
        public async Task<IActionResult> ShowPreview(IFormCollection form, int formId = 0)
        {
            try
            {
                FormViewModel viewModel;

                if (formId == 0)
                {
                    int formParentId = int.Parse(form[nameof(FormViewModel.FormParentId)]);
                    viewModel =
                        await _formConfigurationPersistenceService.GetFormViewModel(formParentId, true);
                }
                else
                {
                    viewModel =
                        await _formConfigurationPersistenceService.GetFormViewModelByFormId(formId);
                }

                viewModel.DontUploadFilesToStorage = true;

                formFieldsLayoutService.DistributePostedFieldValuesToViewModel(form, viewModel);
                bool isViewModelValidationSuccess = await formValidationService.ValidateViewModel(
                    viewModel,
                    nomenclatureGrpcClient,
                    await registerService.GetCurrentRegisterId());

                if (isViewModelValidationSuccess)
                {
                    SetSuccessMessage(MessageConstant.SuccessfulValidation);
                    return View(nameof(ShowPreview), viewModel);
                    //return View("ShowReadonlyForm", viewModel);
                }

                return View(nameof(ShowPreview), viewModel);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Грешка в {nameof(ShowPreview)}");
                SetErrorMessage("Проблем при зареждане на формата");
                return View(nameof(ShowPreview), new FormViewModel { FormFields = new List<FormField>(), DontUploadFilesToStorage = true });
            }
        }

        /// <summary>
        /// Подаване на JSON данни
        /// </summary>
        [HttpGet]
        [Display(Name = "Зареждане на форма за подаване на JSON данни")]
        public async Task<IActionResult> SubmitJson(int formId)
        {
            Dictionary<string, string> fieldFlatList = await _formConfigurationPersistenceService.GetFormFieldNamesInFlatList(formId);

            string json = JsonSerializer.Serialize(fieldFlatList.Keys.ToDictionary(v => v, v => string.Empty));

            JsonFormDataViewModel model = new JsonFormDataViewModel
            {
                //FormParentId = formParentId
                FormId = formId,
                JsonData = json
            };

            return View(model);
        }


        /// <summary>
        /// Потвърждаване на формата с JSON данни
        /// </summary>
        [HttpPost]
        [Display(Name = "Обработка и генериране на изглед по JSON данни")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitJson(JsonFormDataViewModel model)
        {
            try
            {
                var formData = new Dictionary<string, StringValues>();

                var jsonDocument = JsonDocument.Parse(model.JsonData);
                foreach (var element in jsonDocument.RootElement.EnumerateObject())
                {
                    formData[element.Name] = new StringValues(element.Value.ToString());
                }

                IFormCollection form = new FormCollection(formData);

                return await ShowPreview(form, model.FormId);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Грешка в {nameof(SubmitJson)}");
                SetErrorMessage("Проблем при четене на JSON данните");
                return View(nameof(ShowPreview), new FormViewModel { FormFields = new List<FormField>() });
            }
        }

        /// <summary>
        /// Зареждане на конфигурацията на форма от базата данни
        /// </summary>
        /// <returns>JSON масив</returns>
        [HttpGet]
        [Display(Name = "Извличане на конфигурация на форма от базата данни")]       
        public async Task<string> LoadConfiguration(int formParentId)
        {
            string result = await _formConfigurationPersistenceService.LoadDesignerJson(formParentId);
            return result;
        }

        /// <summary>
        /// Зареждане на конфигурацията на формата на услугата за вписване от базата данни
        /// </summary>
        /// <returns>JSON</returns>
        [HttpGet]
        [Display(Name = "Импортиране на конфигурация на форма от базата данни")]
        public async Task<JsonResult> ImportRegisterFormConfiguration()
        {
            try
            {
                string result = await _formConfigurationPersistenceService.ImportRegisterFormConfiguration();
                return new JsonResult(new { notFound = string.IsNullOrEmpty(result), config = result });
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Проблем в {nameof(ImportRegisterFormConfiguration)}");
                return new JsonResult(string.Empty);
            }
        }

        /// <summary>
        /// Зареждане на конфигурацията по подразбиране за тип поле
        /// </summary>
        /// <param name="type">Тип на полето</param>
        /// <returns>JSON обект</returns>
        [HttpGet]
        [Display(Name = "Извличане на конфигурацията по подразбиране за тип поле")]
        public async Task<string> GetFieldDefaultConfiguration(string type)
        {
            try
            {
                CatalogFieldRequest request = new CatalogFieldRequest { FieldType = type.ToString() };

                CatalogGetFieldReply reply = await objectCatalogGrpcClient.GetFieldAsync(request);

                if (reply.Status.Code != ResultCodes.Ok)
                {
                    Logger.LogError($"GetFieldAsync неуспешен в {nameof(GetFieldDefaultConfiguration)}");
                    return "{}";
                }

                return reply.Data;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Проблем при извличане на данни в {nameof(GetFieldDefaultConfiguration)} за тип {type.ToString()}");
                return "{}";
            }
        }

        /// <summary>
        /// Одобряване на конфигурация
        /// </summary>
        /// <returns>Празен низ при успех, съобщение за грешка при неуспех</returns>
        [HttpPost]
        [Display(Name = "Одобряване на конфигурация на форма")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.GlobalAdmin)]
        public async Task<IActionResult> ApproveConfiguration(int formId)
        {
            OperationResult result = await _formConfigurationPersistenceService.ApproveConfiguration(formId);

            if (result.IsSuccess)
            {
                SetSuccessMessage("Конфигурацията е записана успешно");
                return new JsonResult(new { success = true, message = string.Empty });
            }

            return new JsonResult(new { success = false, message = result.ErrorMessage });
        }
    }
}
