using Google.Protobuf.Collections;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using URegister.Common;
using URegister.Core.Contracts;
using URegister.Core.Services;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Extensions;
using URegister.Infrastructure.Model.RegisterForms;
using URegister.NomenclaturesCatalog;
using URegister.ObjectsCatalog;
using URegister.RegistersCatalog;
using static URegister.ObjectsCatalog.ObjectsCatalogGrpc;

namespace URegister.Admin.Controllers
{
    [Display(Name = "Дизайнер")]
    public class DesignerController : BaseController
    {
        private const string ConfiguredFieldName = "mainPreviewedFieldDefaultName";
        private readonly NomenclatureGrpc.NomenclatureGrpcClient _nomenclatureGrpcClient;
        private readonly ObjectsCatalogGrpcClient _objectCatalogGrpcClient;
        private readonly IFormValidationService _formValidationService;
        private readonly IFormFieldsLayoutService _formFieldsLayoutService;
        private readonly RegistersCatalogGrpc.RegistersCatalogGrpcClient _registerGrpcClient;
        public readonly ILogger<DesignerController> Logger;

        public DesignerController(
            NomenclatureGrpc.NomenclatureGrpcClient nomenclatureGrpcClient,
            ObjectsCatalogGrpcClient objectCatalogGrpcClient,
            IFormValidationService formValidationService,
            IFormFieldsLayoutService formFieldsLayoutService,
            RegistersCatalogGrpc.RegistersCatalogGrpcClient registerGrpcClient,
            ILogger<DesignerController> logger)
        {
            _nomenclatureGrpcClient = nomenclatureGrpcClient;
            _objectCatalogGrpcClient = objectCatalogGrpcClient;
            _formValidationService = formValidationService;
            _formFieldsLayoutService = formFieldsLayoutService;
            _registerGrpcClient = registerGrpcClient;
            Logger = logger;
        }

        /// <summary>
        /// Конфигуратор на полета
        /// </summary>
        /// <returns></returns>      
        [Display(Name = "Отваряне на конфигуратор на полета")]
        public async Task<IActionResult> ConfigureFields(string preSelectedType = "")
        {
            IEnumerable<CatalogFieldType> fieldTypes = await FieldTypeCatalogService.GetAllFieldType(_objectCatalogGrpcClient);

            DesignerViewModel viewModel = new DesignerViewModel
            {
                FormTitle = "конфигурация на полета",
                SelectedType = preSelectedType
            };

            if (fieldTypes == null)
            {
                SetErrorMessage("Проблем при зареждане на типовете полета");
                Logger.LogError($"Проблем при зареждане на типовете полета в {nameof(Index)}");
                return View(viewModel);
            }

            ViewBag.DesignerFieldTypes_ddl = fieldTypes
                .Select(e => new SelectListItem
                {
                    Value = e.Type,
                    Text = e.Label
                }).ToList();

            ViewBag.DesignerFieldBasicTypes_ddl = fieldTypes
                .Where(t => !t.IsComplex)
                        .Select(e => new SelectListItem
                        {
                            Value = e.Type,
                            Text = e.Label
                        }).ToList();

            NomenclaturePublicRequest getNomenclaturesRequest = new NomenclaturePublicRequest
            {
                RegisterId = 0
            };

            try
            {
                NomenclaturePublicResponse nomenclatureResult =
                    await _nomenclatureGrpcClient.GetNomenclaturePublicAsync(getNomenclaturesRequest);

                if (nomenclatureResult.ResultStatus.Code != ResultCodes.Ok)
                {
                    Logger.LogError($"GetNomenclaturePublicAsync неуспешен в {nameof(Index)}");
                    SetErrorMessage("Проблем при зареждане на номенклатурите");
                    ViewBag.NomenclatureTypes_ddl = new List<SelectListItem>();
                }

                List<SelectListItem> nomenclatureTypes = nomenclatureResult.NomenclatureTypes.Select(nom =>
                    new SelectListItem(nom.Name, nom.Type)).ToList();

                nomenclatureTypes.Insert(0, new SelectListItem("", ""));

                ViewBag.NomenclatureTypes_ddl = nomenclatureTypes;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Проблем при зареждане на номенклатурите в {nameof(Index)}");
                ViewBag.NomenclatureTypes_ddl = new List<SelectListItem>();
            }

            return View(viewModel);
        }

        /// <summary>
        /// Потвърждаване на формата с полета
        /// </summary>
        [HttpGet]
        [Display(Name = "Генериране на изглед за поле")]
        public async Task<IActionResult> ShowPreview(string fieldType)
        {
            try
            {
                string fieldConfiguration =
                    await GetFieldDefaultConfiguration(fieldType);
                if (string.IsNullOrWhiteSpace(fieldConfiguration))
                {
                    Logger.LogError(
                        $"Проблем при зареждане на конфигурацията за поле {fieldType} в {nameof(ShowPreview)}");
                    SetErrorMessage($"Проблем при зареждане на конфигурацията за поле {fieldType}");
                    FormViewModel emptyModel = new FormViewModel
                    {
                        FormFields = new List<FormField>(),
                        DontUploadFilesToStorage = true
                    };

                    return View(emptyModel);
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                };

                FormField? configuredField = JsonSerializer.Deserialize<FormField>(fieldConfiguration, options);
                if (string.IsNullOrWhiteSpace(configuredField.Label))
                {
                    configuredField.Label = "<име на полето>";
                }
                configuredField.Name = ConfiguredFieldName;

                var formFields = new List<FormField>() { configuredField };
                _formFieldsLayoutService.GiveSnakeCaseNamesToComplexFieldChildren(formFields);

                FormViewModel viewModel = new FormViewModel();
                viewModel.DontUploadFilesToStorage = true;
                viewModel.FormFields = formFields;
                viewModel.FormTitle = (await FieldTypeCatalogService.GetAllFieldType(_objectCatalogGrpcClient))
                    .First(t => t.Type == fieldType)
                    .Label;
                viewModel.SelectedType = fieldType;
                return View(viewModel);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Проблем при генериране на view model в за поле  {nameof(ShowPreview)}");
                SetErrorMessage($"Проблем при зареждане на конфигурацията за поле {fieldType}");
                FormViewModel emptyModel = new FormViewModel
                {
                    FormFields = new List<FormField>(),
                    DontUploadFilesToStorage = true
                };
                return View(emptyModel);
            }
        }

        /// <summary>
        /// Потвърждаване на формата с полета
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Display(Name = "Валидиране и преглед на форма")]
        public async Task<IActionResult> ShowPreview(IFormCollection form)
        {
            try
            {
                string jsonFieldsModel = await GetFieldDefaultConfiguration(form[nameof(FormViewModel.SelectedType)]);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                };

                FormField? configuredField = JsonSerializer.Deserialize<FormField>(jsonFieldsModel, options);
                if (string.IsNullOrWhiteSpace(configuredField.Label))
                {
                    configuredField.Label = "<име на полето>";
                }
                configuredField.Name = ConfiguredFieldName;

                var formFields = new List<FormField> { configuredField };
                _formFieldsLayoutService.GiveSnakeCaseNamesToComplexFieldChildren(formFields);

                FormViewModel viewModel = new FormViewModel
                {
                    FormFields = formFields,
                    FormTitle =
                        (await FieldTypeCatalogService.GetAllFieldType(_objectCatalogGrpcClient))
                        .First(t => t.Type == configuredField.Type)
                        .Label,
                    SelectedType = configuredField.Type,
                    DontUploadFilesToStorage = true
                };

                _formFieldsLayoutService.DistributePostedFieldValuesToViewModel(form, viewModel);
                bool isViewModelValidationSuccess = await _formValidationService.ValidateViewModel(viewModel, _nomenclatureGrpcClient, 0);

                if (isViewModelValidationSuccess)
                {
                    SetSuccessMessage(MessageConstant.SuccessfulValidation);
                }

                return View(viewModel);
                
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Грешка в {nameof(ShowPreview)}");
                SetErrorMessage("Проблем при зареждане на формата");
                return View(new FormViewModel { FormFields = new List<FormField>(), DontUploadFilesToStorage = true });
            }
        }

        /// <summary>
        /// Сравняваме дали записаният файл не е същия, за да избегнем създаване на нова версия в DB
        /// </summary>
        /// <param name="jsonFieldDefaults"></param>
        /// <returns></returns>
        private async Task<bool> IsFieldToBeSavedSameAsLatest(string jsonFieldDefaults)
        {
            FormField jsonAsField = jsonFieldDefaults.FromJson<FormField>();
            string latestVersionInDbJson = await GetFieldDefaultConfiguration(jsonAsField.Type);
            if (latestVersionInDbJson.Length < 10) //няма преден запис
            {
                return false;
            }
            FormField latestVersionInDb = latestVersionInDbJson.FromJson<FormField>();

            if (latestVersionInDb != null)
            {
                if (JsonSerializer.Serialize(jsonAsField) == JsonSerializer.Serialize(latestVersionInDb))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Запис на конфигурация на форма
        /// </summary>
        /// <param name = "jsonFieldDefaults" > JSON конфигурация на тип поле</param>
        /// <returns></returns>
        [HttpPost]       
        [Display(Name = "Запис на настройки по подразбиране за тип поле")]
        [ValidateAntiForgeryToken]
        public async Task<bool> SaveDefaults(string jsonFieldDefaults)
        {
            if (await IsFieldToBeSavedSameAsLatest(jsonFieldDefaults))
            {
                return true;
            }

            CatalogSerializedData request = new CatalogSerializedData
            {
                Data = jsonFieldDefaults
            };

            CatalogSetFieldReply reply;
            try
            {
                reply = await _objectCatalogGrpcClient.SetFieldAsync(request);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Проблем при запис на стойности за поле {jsonFieldDefaults} в {nameof(SaveDefaults)}");
                return false;
            }

            if (reply.Status.Code != ResultCodes.Ok)
            {
                Logger.LogError($"Проблем при запис на стойности за поле {jsonFieldDefaults} в {nameof(SaveDefaults)}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Зареждане на конфигурацията по подразбиране за тип поле
        /// </summary>
        /// <param name="type">Тип на полето</param>
        /// <returns>JSON обект</returns>
        [HttpGet]
        [Display(Name = "Извличане на конфигурация по подразбиране за тип поле")]
        public async Task<string> GetFieldDefaultConfiguration(string type)
        {
            try
            {
                CatalogFieldRequest request = new CatalogFieldRequest { FieldType = type };

                CatalogGetFieldReply reply = await _objectCatalogGrpcClient.GetFieldAsync(request);

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
        /// Връща списък от същесвуващите типове полета
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Display(Name = "Зареждане на списък с типове полета")]
        public async Task<IActionResult> FieldTypeList()
        {
            return View();
        }

        /// <summary>
        /// Добавяне на нов тип поле
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Display(Name = "Зареждане на форма за добавяне на нов тип поле")]
        public async Task<IActionResult> AddFieldType(string fieldType)
        {
            if (!string.IsNullOrEmpty(fieldType))
            {
                try
                {                  
                    var reply = await FieldTypeCatalogService.GetFieldType(_objectCatalogGrpcClient, fieldType);
                 
                    if (reply == null)
                    {
                        SetErrorMessage("Проблем при зареждане на тип поле");
                        Logger.LogError($"Проблем при зареждане на тип поле в {nameof(AddFieldType)}");
                        return RedirectToAction(nameof(FieldTypeList));
                    }

                    var editModel = new AddFieldTypeViewModel
                    {
                        Label = reply.Label,
                        Type = reply.Type,
                        Id = reply.FieldTypeId,
                        RegisterRestrictionCodes = reply.RegisterRestrictionCodes.ToList()
                    };

                    ViewData["Title"] = $"Редактиране на тип поле \"{editModel.Label}\"";
                    await FillFieldTypeViewBags();
                    return View(editModel);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"Проблем при извличане на данни в {nameof(AddFieldType)} за тип {fieldType}");
                    SetErrorMessage($"Проблем при извличане на данни за тип поле.");
                    return RedirectToAction(nameof(FieldTypeList));
                }
            }
            ViewData["Title"] = "Добавяне на нов тип поле";
            await FillFieldTypeViewBags();
            return View(new AddFieldTypeViewModel());
        }

        private async Task FillFieldTypeViewBags()
        {
            RegisterListRequest request = new RegisterListRequest(){ DataTableRequest = new DatatableRequest { Length = -1 }, };
            var response = await _registerGrpcClient.GetRegisterFullListAsync(request);

            ViewBag.RegisterRestrictionCodes_ddl =
                response.Data.Select(e => new SelectListItem(e.Name, e.Code)).ToList();
        }

        /// <summary>
        /// Добавяне или редактиране на тип поле
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Добавяне или редактиране на тип поле")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFieldType(AddFieldTypeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await FillFieldTypeViewBags();
                ViewData["Title"] = model.Id == 0 ? "Добавяне на нов тип поле" : $"Редактиране на тип поле \"{model.Label}\"";
                return View(model);
            }

            CatalogFieldType fieldType = new CatalogFieldType
            {
                IsComplex = true,
                TemplateName = "FormFieldComplex",
                Label = model.Label.Trim(),
                Type = model.Type.Trim(),
            };

            if (model.RegisterRestrictionCodes != null)
            {
                fieldType.RegisterRestrictionCodes.AddRange(model.RegisterRestrictionCodes);
            }

            if (model.Id == 0)
            {
                var allFieldTypes = await FieldTypeCatalogService.GetAllFieldType(_objectCatalogGrpcClient);
                if (allFieldTypes.Any(t => (t.Label.Equals(fieldType.Label, StringComparison.InvariantCultureIgnoreCase) ||
                                   t.Type.Equals(fieldType.Type, StringComparison.InvariantCultureIgnoreCase))))
                {
                    SetErrorMessage("Тип поле с това име или тип вече съществува.");
                    await FillFieldTypeViewBags();
                    ViewData["Title"] = "Добавяне на нов тип поле";
                    return View(model);
                }
            }           

            try
            {
                ResultStatus reply = await _objectCatalogGrpcClient.SetFieldTypeAsync(fieldType);
                if (reply.Code != ResultCodes.Ok)
                {                   
                    Logger.LogError(
                     $"Проблем при {(model.Id > 0 ? "редактиране" : "запис")} на тип поле {model.Type} в {nameof(AddFieldType)}. {reply.Message}");
                    SetErrorMessage($"Проблем при {(model.Id > 0 ? "редактиране" : "запис")} на тип поле.");
                    await FillFieldTypeViewBags();
                    ViewData["Title"] = model.Id > 0 ? $"Редактиране на тип поле \"{model.Label}\"" : "Добавяне на нов тип поле";
                    return View(model);
                }
            }
            catch (Exception ex)
            {                
                Logger.LogError(
                 $"Проблем при {(model.Id > 0 ? "редактиране" : "запис")} на тип поле {model.Type} в {nameof(AddFieldType)}. {ex.Message}");
                SetErrorMessage($"Проблем при {(model.Id > 0 ? "редактиране" : "запис")} на тип поле.");
                await FillFieldTypeViewBags();
                ViewData["Title"] = model.Id > 0 ? $"Редактиране на тип поле \"{model.Label}\"" : "Добавяне на нов тип поле";
                return View(model);
            }

            SetSuccessMessage($"Тип поле {model.Label} е успешно {(model.Id > 0 ? "редактиран" : "записан")}");
            return RedirectToAction(nameof(FieldTypeList), new { preSelectedType = model.Type });
        }
      
        [HttpGet]
        [Display(Name = "Извличане на списък с типове полета")]
        public async Task<IActionResult?> GetFieldTypes()
        {
            IEnumerable<CatalogFieldType> fieldTypes = await FieldTypeCatalogService.GetAllFieldType(_objectCatalogGrpcClient);
            var data = fieldTypes.OrderBy(f => f.IsComplex).ToList();

            return Json(new { data = data });
        }

        [HttpPost]
        [Display(Name = "Качване на файл към форма")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadFile(IFormFile file, int formParentId, string fieldName, string key)
        {
            try
            {
                if (file == null || file.Length <= 0)
                {
                    return Json(new { success = false, error = MessageConstant.Values.FileIsEmpty });
                }

                string fieldConfiguration =
                    await GetFieldDefaultConfiguration(SimpleFormFieldType.File.ToString());

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                };

                FormField? fieldMetadata = JsonSerializer.Deserialize<FormField>(fieldConfiguration, options);

                if (fieldMetadata == null)
                {
                    return Json(new { success = false, error = MessageConstant.Values.FileUploadFailed });
                }

                bool validationResult = await _formValidationService.ValidateFile(fieldMetadata, file);

                if (!validationResult)
                {
                    return Json(new { success = false, error = fieldMetadata.ValidationError });
                }

                return Json(new { success = true, fileKey = Guid.NewGuid() });
                
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Проблем при качване на файл в {UploadFile}, за форма с parentId {formParentId}");
                return Json(new { success = false, error = MessageConstant.Values.FileUploadFailed });
            }
        }

        [HttpPost]
        [Display(Name = "Изтриване на файл")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFile(string key)
        {
            return Json(new { success = true });
        }

        /// <summary>
        /// Търси поле на форма по име до едно ниво навътре
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        private FormField FindFieldMetadata(IEnumerable<FormField> fields, string fieldName)
        {
            foreach (var field in fields)
            {
                if (field.Name == fieldName)
                {
                    return field;
                }

                foreach (var innerField in field.Fields)
                {
                    if (innerField.Name == fieldName)
                    {
                        return innerField;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Изтриване на поле
        /// </summary>
        /// <param name="id">Идентификатор на тип услуга за изтриване</param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Изтриване на тип поле")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFieldType(int id)
        {
            GetFieldTypeRequest request = new GetFieldTypeRequest()
            {
                FieldTypeId = id
            };

            ResultStatus result = await _objectCatalogGrpcClient.DeleteFieldTypeAsync(request);

            if (result.Code == ResultCodes.Ok)
            {
                SetSuccessMessage("Типа поле е изтрит успешно");
            }
            else
            {
                SetErrorMessage(result.Message);
            }

            return Json(null);
        }

    }
}
