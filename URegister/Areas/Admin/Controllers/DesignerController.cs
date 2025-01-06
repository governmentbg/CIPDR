using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using URegister.Common;
using URegister.Core.Contracts;
using URegister.Core.Services;
using URegister.Infrastructure.Model.RegisterForms;
using URegister.NomenclaturesCatalog;
using URegister.ObjectsCatalog;

namespace URegister.Areas.Admin.Controllers
{
    [Area("Admin")]
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
        [DisplayName("Дизайнер на форма от регистър")]
        public async Task<IActionResult> Index(int formParentId)
        {
            FormViewModel formViewModel = await _formConfigurationPersistenceService.GetFormViewModel(formParentId);
            if (formViewModel == null)
            {
                Logger.LogError($"Не е намерена форма с parentId {formParentId} в {nameof(Index)}");
                SetErrorMessage("Проблем при зареждане на форма");
                return View(new DesignerViewModel());
            }

            DesignerViewModel viewModel = new DesignerViewModel
            {
                FormParentId = formParentId,
                FormTitle = formViewModel.FormTitle
            };

            IEnumerable<CatalogFieldType> fieldTypes = await FieldTypeCatalogService.GetAllFieldType(objectCatalogGrpcClient);

            if (fieldTypes == null)
            {
                SetErrorMessage("Проблем при зареждане на типовете полета");
                Logger.LogError($"Проблем при зареждане на типовете полета в {nameof(Index)}");
                return View(viewModel);
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

            return View(viewModel);
        }

        /// <summary>
        /// Запис на конфигурация на форма
        /// </summary>
        /// <param name="jsonFieldsModel">JSON конфигурация на формата</param>
        /// <param name="formParentId">Родителски идентификатор на формата</param>
        /// <param name="formTitle">Заглавие на формата</param>
        /// <returns></returns>
        [HttpPost]
        [DisplayName("Запис на конфигурация на форма")]
        [ValidateAntiForgeryToken]
        public async Task<bool> SaveConfiguration(string jsonFieldsModel, int formParentId, string formTitle)
        {
            bool result = await _formConfigurationPersistenceService.SaveDesignerJson(jsonFieldsModel, formParentId, formTitle);
            return result;
        }

        /// <summary>
        /// Показване на изглед
        /// </summary>
        [HttpGet]
        [DisplayName("Генериране на изглед")]
        public async Task<IActionResult> ShowPreview(int formParentId)
        {
            FormViewModel viewModel = await _formConfigurationPersistenceService.GetFormViewModel(formParentId);

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
        [DisplayName("Генериране на изглед")]
        public async Task<IActionResult> ShowPreview(IFormCollection form)
        {
            try
            {
                int formParentId = int.Parse(form[nameof(FormViewModel.FormParentId)]);
                FormViewModel viewModel = await _formConfigurationPersistenceService.GetFormViewModel(formParentId);

                formFieldsLayoutService.DistributePostedFieldValuesToViewModel(form, viewModel);
                bool isViewModelValidationSuccess = await formValidationService.ValidateViewModel(
                    viewModel,
                    nomenclatureGrpcClient,
                    await registerService.GetCurrentRegisterId());

                if (isViewModelValidationSuccess)
                {
                    return View(nameof(ShowPreview), viewModel);
                    //return View("ShowReadonlyForm", viewModel);
                }

                return View(nameof(ShowPreview), viewModel);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Грешка в {nameof(ShowPreview)}");
                SetErrorMessage("Проблем при зареждане на формата");
                return View(nameof(ShowPreview), new FormViewModel { FormFields = new List<FormField>() });
            }
        }

        /// <summary>
        /// Зареждане на конфигурацията на форма от базата данни
        /// </summary>
        /// <returns>JSON масив</returns>
        [HttpGet]
        [DisplayName("Зареждане на конфигурацията на форма от базата данни")]
        public async Task<string> LoadConfiguration(int formParentId)
        {
            string result = await _formConfigurationPersistenceService.LoadDesignerJson(formParentId);
            return result;
        }

        /// <summary>
        /// Зареждане на конфигурацията по подразбиране за тип поле
        /// </summary>
        /// <param name="type">Тип на полето</param>
        /// <returns>JSON обект</returns>
        [HttpGet]
        [DisplayName("Зареждане на конфигурацията по подразбиране за тип поле")]
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
    }
}
