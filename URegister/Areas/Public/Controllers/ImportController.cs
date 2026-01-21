using Google.Protobuf.WellKnownTypes;
using iText.Kernel.Pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using URegister.Common;
using URegister.Core.Contracts;
using URegister.Core.Models.Process;
using URegister.Core.Models.Service;
using URegister.Core.Services;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Model.EDelivery;
using URegister.Infrastructure.Model.RegisterForms;
using URegister.IntegrationsCatalog;
using URegister.NomenclaturesCatalog;
using URegister.RegistersCatalog;
using static URegister.IntegrationsCatalog.IntegrationGrpc;

namespace URegister.Areas.Public.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Display(Name = "Импортиране")]
    public class ImportController : BaseController
    {
        private readonly ILogger<ImportController> _logger;
        private readonly IFormConfigurationPersistenceService _formConfigurationPersistenceService;
        private readonly IFormFieldsLayoutService _formFieldsLayoutService;
        private readonly IFormValidationService _formValidationService;
        private readonly IRegisterService _registerService;
        private readonly NomenclatureGrpc.NomenclatureGrpcClient _nomenclatureGrpcClient;
        private readonly IProcessService _processService;
        private readonly IServiceService _serviceService;
        private readonly IntegrationGrpcClient _integrationGrpcClient;
        private readonly IRegixReportService _regixReportService;
        private readonly RegistersCatalogGrpc.RegistersCatalogGrpcClient _registerGrpcClient;
        private readonly IFieldFormulaCalculationService _fieldFormulaCalculationService;

        public ImportController(ILogger<ImportController> logger,
            IFormConfigurationPersistenceService formConfigurationPersistenceService,
            IFormFieldsLayoutService formFieldsLayoutService,
            IFormValidationService formValidationService,
            IRegisterService registerService,
            NomenclatureGrpc.NomenclatureGrpcClient nomenclatureGrpcClient,
            IProcessService processService,
            IServiceService serviceService,
            IntegrationGrpcClient integrationGrpcClient,
            IRegixReportService regixReportService,
            RegistersCatalogGrpc.RegistersCatalogGrpcClient registerGrpcClient,
            IFieldFormulaCalculationService fieldFormulaCalculationService)
        {
            _logger = logger;
            _formConfigurationPersistenceService = formConfigurationPersistenceService;
            _formFieldsLayoutService = formFieldsLayoutService;
            _formValidationService = formValidationService;
            _registerService = registerService;
            _nomenclatureGrpcClient = nomenclatureGrpcClient;
            _processService = processService;
            _serviceService = serviceService;
            _integrationGrpcClient = integrationGrpcClient;
            _regixReportService = regixReportService;
            _registerGrpcClient = registerGrpcClient;
            _fieldFormulaCalculationService = fieldFormulaCalculationService;
        }

        //TODO : Да се извиква от ImportApplication, когато е сигурно, че работи
        /// <summary>
        /// Импорт на данни за заявена услуга през json от .pdf файл
        /// </summary>
        /// <param name="model">json данни на заявена услуга.</param>
        [HttpPost("import-json")]
        [Display(Name = "Импорт на данни за заявена услуга от json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ImportResultVM))]
        public async Task<IActionResult> ImportJson([FromBody] ImportJsonVM model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.JsonFromFile))
                {
                    return BadRequest(new { message = "Празен json." });
                }

                var formData = new Dictionary<string, StringValues>();

                var jsonDocument = JsonDocument.Parse(model.JsonFromFile);

                ServiceVM registerService = await _serviceService.GetRegisterService();
                if (model.ServiceId > 0)
                {
                    registerService = await _serviceService.GetService(model.ServiceId, true);
                }

                if (registerService == null)
                {
                    return BadRequest("Не е намерена услуга за вписване или формата асоциирана с нея");
                }

                //Взимаме попълнен модел при частичен импорт
                FormViewModel viewModel = string.IsNullOrWhiteSpace(model.RegisterNumber) ?
                    (await _formConfigurationPersistenceService.GetFormViewModel(registerService.FormParentId, true)) :
                    (await _processService.GetFormViewModel(model.RegisterNumber)).Item1;

                var metaFiles = await _processService.ImportApplicationEDeliveryFile(model.EDeliveryFiles);
                var attachedFileData = metaFiles.ToDictionary(x => x.FileName, x => x.FileId.ToString());

                string readEFormJsonError = await GatherAndSetJsonDataFromEForm(jsonDocument, formData, viewModel, attachedFileData, model.AdministrationUic);

                if (!string.IsNullOrWhiteSpace(readEFormJsonError))
                {
                    return BadRequest(readEFormJsonError);
                }

                var existingProcess = await _processService.GetProcess(Guid.Parse(formData["_referenceNumber"]));
                if (existingProcess != null)
                {
                    return Ok(new ImportResultVM
                    {
                        Status = "Success",
                        IncomingNumber = existingProcess.IncomingNumber,
                        IncomingDate = existingProcess.IncomingDate,
                        ProcessId = existingProcess.Id,
                        Timestamp = DateTime.UtcNow,
                    });
                }

                viewModel.UserTimeZoneOffsetInMinutes = DetermineTimezoneOffsetOfEform(formData);
                viewModel.DontUploadFilesToStorage = true;
                IFormCollection form = new FormCollection(formData);

                _formFieldsLayoutService.DistributePostedFieldValuesToViewModel(form, viewModel);
                await _formConfigurationPersistenceService.ApplyConditionTreeOnFormModel(viewModel);
                bool isViewModelValidationSuccess = await _formValidationService.ValidateViewModel(
                    viewModel,
                    _nomenclatureGrpcClient,
                    await _registerService.GetCurrentRegisterId());


                if (!isViewModelValidationSuccess)
                {
                    Dictionary<string, string> formFieldErrors =
                        await _formValidationService.GetValidatedFormFieldsErrors(viewModel);
                    return BadRequest(formFieldErrors);
                }

                OperationResult calculationResult = await _fieldFormulaCalculationService.CalculateFormulas(viewModel);

                if (!calculationResult.IsSuccess)
                {
                    return BadRequest(calculationResult.ErrorMessage);
                }

                var serviceStep = registerService.Steps.OrderBy(x => x.OrderNum).First();
                var stepVM = await _processService.ToProcessStepVM(Guid.Empty, null, registerService.Id, serviceStep.Id,
                    serviceStep.OrderNum, null, null, viewModel, false);
                stepVM.ProcessInfo.ReceivedChannelId = ChannelType.EDelivery;
                stepVM.ProcessInfo.PreferredResultDeliveryMethod = formData["_resultChannel"].ToString();
                stepVM.FromProcessId = await _processService.GetFromProcessId(model.RegisterNumber);
                (ProcessStepVM addedStep, _) = await _processService.AddStep(
                    stepVM,
                    model.AdministrationUic,
                    Guid.Parse(formData["_referenceNumber"].ToString()));
                await _processService.ImportApplicationEDeliveryFileSetProcess(addedStep.ProcessId, metaFiles);

                return Ok(new ImportResultVM
                {
                    Status = "Success",
                    IncomingNumber = addedStep.IncomingNumber,
                    IncomingDate = addedStep.IncomingDate,
                    ProcessId = addedStep.ProcessId,
                    Timestamp = DateTime.UtcNow,
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Проблем при импорт на данни от е-форма в {nameof(ImportApplication)}");

                var errorResponse = new
                {
                    error = "Проблем при импорт на данни от е-форма",
                    message = e.Message,
                    stackTrace = e.StackTrace,
                    innerException = e.InnerException?.Message
                };

                return new ObjectResult(errorResponse)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }

        private async Task<string> GetAdministrationUicFromEForm(JsonDocument jsonDocument)
        {
            if (jsonDocument?.RootElement.ValueKind == JsonValueKind.Object &&
                jsonDocument.RootElement.TryGetProperty("ServiceRequest", out var serviceRequest) &&
                serviceRequest.TryGetProperty("specificContent", out var specificContent1) &&
                specificContent1.TryGetProperty("specificContent", out var specificContent2) &&
                specificContent2.TryGetProperty("registerOwner", out var registerOwner) &&
                registerOwner.TryGetProperty("value", out var administrationIdentifier))
            {
                return administrationIdentifier.GetString();
            }

            int registerId = await _registerService.GetCurrentRegisterId();
            AdministrationListResponse administrationResponse = await _registerGrpcClient.GetAdministrationListAsync(
                new AdministrationListRequest
                {
                    DataTableRequest = new DatatableRequest { Length = -1 },
                    RegisterId = registerId
                });

            return administrationResponse.Data.ToList().First().Uic;
        }

        //TODO : Да извиква от ImportJson, когато е сигурно, че работи
        /// <summary>
        /// Импорт на данни за заявена услуга през json от .pdf файл
        /// </summary>
        /// <param name="file">Pdf файл с json данни на заявена услуга.</param>
        /// <param name="attachedFileDataJson">Инфромация за качените фалове. Речник с ключ името, и стойност идентификатор на файл от Storage-а</param>
        /// <param name="registerNumber">Номер на заявена услуга при частичен импорт</param>
        /// <param name="serviceId">Идентификатор на услуга за промяна при частичен импорт</param>
        [HttpPost("import-application")]
        [Display(Name = "Импорт на данни за заявена услуга от файл")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportApplication(IFormFile file, 
            [FromForm] string attachedFileDataJson = null, 
            string registerNumber = null, 
            int serviceId = 0)
        {
            try
            {
                Dictionary<string, string> attachedFileData = new Dictionary<string, string>();

                if (!string.IsNullOrEmpty(attachedFileDataJson))
                {
                    try
                    {
                        attachedFileData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(attachedFileDataJson)
                                           ?? new Dictionary<string, string>();
                    }
                    catch (System.Text.Json.JsonException ex)
                    {
                        return BadRequest($"Невалиден JSON формат в {nameof(attachedFileDataJson)}");
                    }
                }


                var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                string jsonFromFile = ResolvePdfJson(memoryStream);

                if (string.IsNullOrWhiteSpace(jsonFromFile))
                {
                    return BadRequest(new { message = "Съдържанието на файла не може да бъде прочетено." });
                }

                if (!await _formValidationService.IsFileAcceptableFormat(file))
                {
                    return BadRequest("Съдържанието на файла не отговаря на разширението му");
                }

                var formData = new Dictionary<string, StringValues>();

                var jsonDocument = JsonDocument.Parse(jsonFromFile);

                ServiceVM registerService = await _serviceService.GetRegisterService();

                if (registerService == null)
                {
                    return BadRequest("Не е намерена услуга за вписване или формата асоциирана с нея");
                }

                //Взимаме попълнен модел при частичен импорт
                FormViewModel viewModel = string.IsNullOrWhiteSpace(registerNumber) ?
                    (await _formConfigurationPersistenceService.GetFormViewModel(registerService.FormParentId, true)) :
                    (await _processService.GetFormViewModel(registerNumber)).Item1;

                string administrationId = await GetAdministrationUicFromEForm(jsonDocument);
                string readEFormJsonError = await GatherAndSetJsonDataFromEForm(jsonDocument, formData, viewModel, attachedFileData, administrationId);

                if (!string.IsNullOrWhiteSpace(readEFormJsonError))
                {
                    return BadRequest(readEFormJsonError);
                }

                var existingProcess = await _processService.GetProcess(Guid.Parse(formData["_referenceNumber"]));
                if (existingProcess != null)
                {
                    return Ok(new
                    {
                        Status = "Success",
                        IncomingNumber = existingProcess.IncomingNumber,
                        IncomingDate = existingProcess.IncomingDate,
                        ProcessId = existingProcess.Id,
                        Timestamp = DateTime.UtcNow,
                        //FileId = ((Guid)saveFileResult.AddedObjectId).ToString()
                    });
                }

                viewModel.UserTimeZoneOffsetInMinutes = DetermineTimezoneOffsetOfEform(formData);

                viewModel.DontUploadFilesToStorage = true;

                IFormCollection form = new FormCollection(formData);

                _formFieldsLayoutService.DistributePostedFieldValuesToViewModel(form, viewModel);
                await _formConfigurationPersistenceService.ApplyConditionTreeOnFormModel(viewModel);
                bool isViewModelValidationSuccess = await _formValidationService.ValidateViewModel(
                    viewModel,
                    _nomenclatureGrpcClient,
                    await _registerService.GetCurrentRegisterId());

                if (!isViewModelValidationSuccess)
                {
                    Dictionary<string, string> formFieldErrors =
                        await _formValidationService.GetValidatedFormFieldsErrors(viewModel);
                    return BadRequest(formFieldErrors);
                }

                OperationResult calculationResult = await _fieldFormulaCalculationService.CalculateFormulas(viewModel);

                if (!calculationResult.IsSuccess)
                {
                    return BadRequest(calculationResult.ErrorMessage);
                }

                var serviceStep = registerService.Steps.Where(x => x.StatusId == (int)ProcessStatus.Registered).First();
                var stepVM = await _processService.ToProcessStepVM(Guid.Empty, null, registerService.Id, serviceStep.Id,
                    serviceStep.OrderNum, null, null, viewModel, false);
                stepVM.ProcessInfo.ReceivedChannelId = ChannelType.EDelivery;
                stepVM.ProcessInfo.PreferredResultDeliveryMethod = formData["_resultChannel"].ToString();
                
                (ProcessStepVM addedStep, _) = await _processService.AddStep(
                    stepVM,
                    administrationId,
                    Guid.Parse(formData["_referenceNumber"].ToString()));
                DateTimeOffset parsedOffset = DateTimeOffset.ParseExact(
                    formData["_requestDateTime"].ToString(),
                    FormattingConstant.ISO8601DateFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal);

                // Convert to DateTime with Kind = Utc
                DateTime eformFillDate = parsedOffset.UtcDateTime;

                SaveOperationResult saveFileResult = await _processService.SaveUploadedFile(file,
                    Guid.Empty,
                    Guid.Parse(formData["_referenceNumber"].ToString()),
                    eformFillDate);

                if (!saveFileResult.IsSuccess)
                {
                    return StatusCode(500, $"Данните са входирани с номер {addedStep.IncomingNumber}, но проблем при качване на файлът в системата. {saveFileResult.ErrorMessage}");
                }

                return Ok(new
                {
                    Status = "Success",
                    IncomingNumber = addedStep.IncomingNumber,
                    IncomingDate = addedStep.IncomingDate,
                    ProcessId = addedStep.ProcessId,
                    Timestamp = DateTime.UtcNow,
                    FileId = ((Guid)saveFileResult.AddedObjectId).ToString()
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Проблем при импорт на данни от е-форма в {nameof(ImportApplication)}");

                var errorResponse = new
                {
                    error = "Проблем при импорт на данни от е-форма",
                    message = e.Message,
                    stackTrace = e.StackTrace,
                    innerException = e.InnerException?.Message
                };

                return new ObjectResult(errorResponse)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }

        private async Task<string> GatherAndSetJsonDataFromEForm(JsonDocument jsonDocument,
            Dictionary<string, StringValues> formData,
            FormViewModel viewModel, Dictionary<string, string>? attachedFileData, string administrationUic)
        {
            JsonElement nodeWithUsefulData;
            JsonElement baseElement;
            try
            {
                baseElement = jsonDocument.RootElement.GetProperty("ServiceRequest");

                nodeWithUsefulData = baseElement
                    .GetProperty("specificContent")
                    .GetProperty("specificContent");
            }
            catch (KeyNotFoundException e)
            {
                return "Структурата на Json данните в документа е неправилна. Не е намерен пътя ServiceRequest->specificContent->specificContent";
            }

            try
            {
                var requestDateTime = baseElement
                    .GetProperty("requestDateTime");

                var identifier = baseElement
                    .GetProperty("requestURI")
                    .GetProperty("identifier");

                var resultChannel = baseElement
                    .GetProperty("resultChannel")
                    .GetProperty("channelType")
                    .GetProperty("code");

                formData["_requestDateTime"] = new StringValues(requestDateTime.GetString());
                formData["_referenceNumber"] = new StringValues(identifier.GetString());
                formData["_resultChannel"] = new StringValues(resultChannel.GetString());
            }
            catch (KeyNotFoundException e)
            {
                return "Структурата на Json данните в документа е неправилна. Не са намерени данни за референтен номер или дата на попълване";
            }

            try
            {
                var administrationIdentifier = baseElement
                    .GetProperty("serviceProvider")
                    .GetProperty("legalIdentifier")
                    .GetProperty("identifier");
                formData["_administrationIdentifier"] = new StringValues(administrationUic);
            }
            catch (KeyNotFoundException e)
            {
                return "Структурата на Json данните в документа е неправилна. Не са намерени данни за идентификатор на администрация";
            }

            var problematicImportFields =
                await AdaptAndAssignCommonEFormFieldsByTags(
                    formData,
                    nodeWithUsefulData,
                    GetFormSubmitNameFieldDictionary(viewModel),
                    attachedFileData);


            if (problematicImportFields.Any())
            {
                return "Проблемен формат на данните в JSON за полета: " +
                                  string.Join(", ", problematicImportFields);
            }

            return String.Empty;
        }

        private Dictionary<string, FormField> GetFormSubmitNameFieldDictionary(FormViewModel viewModel)
        {
            try
            {
                var result = new Dictionary<string, FormField>();

                foreach (FormField field in viewModel.FormFields)
                {
                    if (field.Type != nameof(SimpleFormFieldType.StaticText))
                    {
                        result.Add(field.Name, field);
                    }
                    foreach (FormField subField in field.Fields!
                                 .Where(f => f.Type != nameof(SimpleFormFieldType.StaticText) &&
                                             string.IsNullOrWhiteSpace(f.EFormImportPath))
                            )
                    {
                        result.Add(subField.Name, subField);
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Грешка при {nameof(GetFormSubmitNameFieldDictionary)}");
                return new Dictionary<string, FormField>();
            }
        }

        /// <summary>
        /// Извлича данни от е-формата по таговете в json.
        /// </summary>
        /// <param name="formData"></param>
        /// <param name="nodeWithUsefulData"></param>
        /// <param name="formSubmitNameFieldDictionary"></param>
        /// <param name="attachedFileData"></param>
        /// <returns></returns>
        private async Task<List<string>> AdaptAndAssignCommonEFormFieldsByTags(
            Dictionary<string, StringValues> formData, JsonElement nodeWithUsefulData,
            Dictionary<string, FormField> formSubmitNameFieldDictionary, Dictionary<string, string>? attachedFileData)
        {
            Dictionary<string, JsonElement> tagsDictionary;

            try
            {
                JsonElement tagsElement = nodeWithUsefulData
                    .GetProperty("__additionalSpecificContent")
                    .GetProperty("tags");

                tagsDictionary = tagsElement.Deserialize<Dictionary<string, JsonElement>>();
                tagsDictionary = RefineTagsDictionaryWithRepeatingElements(tagsDictionary, formSubmitNameFieldDictionary);
                _logger.LogInformation("Тагове в импортирания файл/tags in the imported file " + tagsElement);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Проблем при разчитане на Json елемент tags в {nameof(AdaptAndAssignCommonEFormFieldsByTags)}");
                return new List<string> { "Проблем при разчитане на Json елемент tags" };
            }

            List<string> problematicFields = new List<string>();

            foreach (var tagEntry in tagsDictionary)
            {
                FormField field = GetFormFieldByNameOrCloneName(formSubmitNameFieldDictionary, tagEntry.Key);

                if (field == null)
                {
                    continue;
                }

                var foundElement = tagEntry.Value;

                string formDataKey = tagEntry.Key;

                if (field.Type == nameof(SimpleFormFieldType.File))
                {
                    if (foundElement.ValueKind == JsonValueKind.Object)
                    {
                        ImportFile(formData, attachedFileData, foundElement, formDataKey);
                    }
                    else if (foundElement.ValueKind is JsonValueKind.Array)
                    {
                        int fileIndex = 0;
                        foreach (JsonElement jsonElement in foundElement.EnumerateArray())
                        {
                            ImportFile(formData, attachedFileData, jsonElement, formDataKey, fileIndex++);
                        }
                    }
                    else if(foundElement.ValueKind == JsonValueKind.String && foundElement.GetString() == "<непосочено>")
                    {
                    }
                    else
                    {
                        problematicFields.Add(formDataKey);
                    }

                    continue;
                }
                
                if (foundElement.ValueKind == JsonValueKind.Object)
                {
                    if (field.Type == nameof(SimpleFormFieldType.Address))
                    {
                        ImportAddress(formData, foundElement, formDataKey);
                    }
                    else if (field.Type == nameof(SimpleFormFieldType.Person))
                    {
                        ImportPerson(formData, foundElement, formDataKey);
                    }
                    else if (field.Type.ToLower() == nameof(SimpleFormFieldType.authorizedOfficial).ToLower())
                    {
                        ImportPerson(formData, foundElement, formDataKey);
                        await ImportCompany(formData, foundElement, formDataKey);
                    }
                    else if (field.Type == nameof(SimpleFormFieldType.Select) ||
                             field.Type == nameof(SimpleFormFieldType.Autocomplete) ||
                             field.Type == nameof(SimpleFormFieldType.AutocompleteWithCategory))
                    {
                        ImportCode(formData, foundElement, formDataKey);
                    }
                    else if (field.Type == nameof(SimpleFormFieldType.PersonIdentifier))
                    {
                        ImportPersonIdentifier(formData, foundElement, formDataKey);
                    }
                    else if (field.Type == nameof(SimpleFormFieldType.Company))
                    {
                        await ImportCompany(formData, foundElement, formDataKey);
                    }
                    else if (field.Type == nameof(SimpleFormFieldType.CompanyWithAddress))
                    {
                        ImportAddress(formData, foundElement, formDataKey);
                        await ImportCompany(formData, foundElement, formDataKey);
                    }
                    else if (field.Type == nameof(SimpleFormFieldType.CompanyIdentifier))
                    {
                        ImportPersonIdentifier(formData, foundElement, formDataKey);
                    }
                    else if (foundElement.TryGetProperty("value", out var valueProperty))
                    {
                        formData[formDataKey] = new StringValues(valueProperty.ToString());
                    }
                    else if (foundElement.TryGetProperty("SettlementSelect", out var settlementSelect) &&
                             settlementSelect.TryGetProperty("settlementCode", out var settlementCode))
                    {
                        formData[formDataKey] = new StringValues(settlementCode.ToString());
                    }
                    else
                    {
                        problematicFields.Add(formDataKey);
                    }
                }
                else
                {
                    if (field.Type == nameof(SimpleFormFieldType.MultiSelect))
                    {
                        ImportMultiselect(formData, foundElement, formDataKey);
                    }
                    else if (field.Type == nameof(SimpleFormFieldType.Date))
                    {
                        formData[formDataKey] = DateTimeOffset
                            .Parse(foundElement.ToString())
                            .ToString(FormattingConstant.NormalDateFormat);
                    }
                    else if (field.Type == nameof(SimpleFormFieldType.DateTime))
                    {
                        formData[formDataKey] = DateTimeOffset
                            .Parse(foundElement.ToString())
                            .ToString(FormattingConstant.DateTimeFormat);
                    }
                    else
                    {
                        formData[formDataKey] = new StringValues(foundElement.ToString());
                    }
                }
            }

            ApplyRepeatingFieldCantStartWithEmptyTempFix(formData);
            return problematicFields;
        }

        /// <summary>
        /// Поради проблем в генерирането на PDF от е-форма, долният fix се налага за да замени "<непосочено>" с празен низ
        /// </summary>
        /// <param name="formData"></param>
        private static void ApplyRepeatingFieldCantStartWithEmptyTempFix(Dictionary<string, StringValues> formData)
        {
            foreach (KeyValuePair<string, StringValues> keyValuePair in formData)
            {
                if (keyValuePair.Value == "<непосочено>")
                {
                    formData[keyValuePair.Key] = new StringValues(string.Empty);
                }
            }
        }

        private FormField GetFormFieldByNameOrCloneName(Dictionary<string, FormField> formSubmitNameFieldDictionary,
            string fieldName)
        {
            if (formSubmitNameFieldDictionary.ContainsKey(fieldName))
            {
                return formSubmitNameFieldDictionary[fieldName];
            }

            fieldName = Regex.Replace(fieldName, @"#\d+", "");

            if (formSubmitNameFieldDictionary.ContainsKey(fieldName))
            {
                return formSubmitNameFieldDictionary[fieldName];
            }

            return null;
        }

        private Dictionary<string, JsonElement> RefineTagsDictionaryWithRepeatingElements(Dictionary<string, JsonElement> tagsDictionary, Dictionary<string, FormField> formSubmitNameFieldDictionary)
        {
            Dictionary<string, JsonElement> result = new Dictionary<string, JsonElement>();

            foreach (KeyValuePair<string, JsonElement> tagEntry in tagsDictionary)
            {
                if (formSubmitNameFieldDictionary.ContainsKey(tagEntry.Key) &&
                    formSubmitNameFieldDictionary[tagEntry.Key].CanBeRepeated && 
                    tagEntry.Value.ValueKind == JsonValueKind.Array)
                {
                    int index = 0;
                    foreach (JsonElement jsonElement in tagEntry.Value.EnumerateArray())
                    {
                        if (index == 0)
                        {
                            result.Add(tagEntry.Key, jsonElement);
                        }
                        else
                        {
                            result.Add(RepeatedFormFieldHelperService.InsertBeforeFirstUnderscore(tagEntry.Key, "#" + index), jsonElement);
                        }

                        index++;
                    }
                }
                else
                {
                    result.Add(tagEntry.Key, tagEntry.Value);
                }
            }

            return result;
        }

        private static void ImportFile(Dictionary<string, StringValues> formData, 
            Dictionary<string, string>? attachedFileData, 
            JsonElement foundElement,
            string formDataKey, int repeatedFileIndex = 0)
        {
            if (TryGetNameProperty(foundElement, out var fileName))
            {
                if (repeatedFileIndex > 0)
                {
                    formDataKey = RepeatedFormFieldHelperService.InsertBeforeFirstUnderscore(formDataKey, "#" + repeatedFileIndex);
                }

                if (attachedFileData.ContainsKey(fileName))
                {
                    formData[formDataKey] = attachedFileData[fileName];
                }
            }
        }

        private static bool TryGetNameProperty(JsonElement element, out string fileName)
        {
            fileName = null;

            // Check if the current element has a "name" property
            if (element.TryGetProperty("name", out var nameProperty) && nameProperty.ValueKind == JsonValueKind.String)
            {
                fileName = nameProperty.GetString();
                return true;
            }

            // If not found, recursively search through subelements of kind Object
            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in element.EnumerateObject())
                {
                    if (property.Value.ValueKind == JsonValueKind.Object)
                    {
                        if (TryGetNameProperty(property.Value, out fileName))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        private static void ImportAddress(Dictionary<string, StringValues> formData, JsonElement element, string formDataKey)
        {
            var enumeratedObject = element.EnumerateObject();

            foreach (var property in element.EnumerateObject())
            {
                if (enumeratedObject.Count() == 1 && property.Name.EndsWith("Address", StringComparison.OrdinalIgnoreCase) &&
                    !property.Name.EndsWith("FullAddress", StringComparison.OrdinalIgnoreCase))
                {
                    ImportAddress(formData, property.Value, formDataKey);
                    continue;
                }

                if (property.Name.EndsWith("CountryCode", StringComparison.OrdinalIgnoreCase))
                {
                    formData[formDataKey + "_countryImmutable"] = property.Value.ToString();
                    continue;
                }

                if (property.Name.EndsWith("FullAddress", StringComparison.OrdinalIgnoreCase))
                {
                    formData[formDataKey + "_addressAbroadImmutable"] = property.Value.ToString();
                    continue;
                }

                if (property.Name.EndsWith("SettlementCode", StringComparison.OrdinalIgnoreCase))
                {
                    formData[formDataKey + "_settlementImmutable"] = property.Value.ToString();
                    continue;
                }

                if (property.Name.EndsWith("postCode", StringComparison.OrdinalIgnoreCase))
                {
                    formData[formDataKey + "_postalCodeImmutable"] = property.Value.ToString();
                    continue;
                }

                if (property.Name.EndsWith("AreaCode", StringComparison.OrdinalIgnoreCase))
                {
                    formData[formDataKey + "_regionImmutable"] = property.Value.ToString();
                    continue;
                }

                if (property.Name.EndsWith("LocationName", StringComparison.OrdinalIgnoreCase))
                {
                    formData[formDataKey + "_streetImmutable"] = property.Value.ToString();
                    continue;
                }

                if (property.Name.EndsWith("buildingNumber", StringComparison.OrdinalIgnoreCase))
                {
                    formData[formDataKey + "_buildingNumberImmutable"] = property.Value.ToString();
                    continue;
                }

                if (property.Name.EndsWith("entrance", StringComparison.OrdinalIgnoreCase))
                {
                    formData[formDataKey + "_entranceNumberImmutable"] = property.Value.ToString();
                    continue;
                }

                if (property.Name.EndsWith("floor", StringComparison.OrdinalIgnoreCase))
                {
                    formData[formDataKey + "_floorImmutable"] = property.Value.ToString();
                    continue;
                }

                if (property.Name.EndsWith("apartment", StringComparison.OrdinalIgnoreCase))
                {
                    formData[formDataKey + "_apartmentNumberImmutable"] = property.Value.ToString();
                    continue;
                }

                if (property.Name.EndsWith("CountrySelect", StringComparison.OrdinalIgnoreCase) && property.Value.ValueKind == JsonValueKind.Object)
                {
                    foreach (var nestedProperty in property.Value.EnumerateObject())
                    {
                        if (nestedProperty.Name.EndsWith("countryCode", StringComparison.OrdinalIgnoreCase))
                        {
                            formData[formDataKey + "_countryImmutable"] = nestedProperty.Value.ToString();
                        }
                    }
                    continue;
                }

                if (property.Name.EndsWith("SettlementSelect", StringComparison.OrdinalIgnoreCase) && property.Value.ValueKind == JsonValueKind.Object)
                {
                    foreach (var nestedProperty in property.Value.EnumerateObject())
                    {
                        if (nestedProperty.Name.EndsWith("settlementCode", StringComparison.OrdinalIgnoreCase))
                        {
                            formData[formDataKey + "_settlementImmutable"] = nestedProperty.Value.ToString();
                        }
                    }
                }
            }
        }

        private static void ImportPerson(Dictionary<string, StringValues> formData, JsonElement element, string formDataKey)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.Name.EndsWith("firstName", StringComparison.OrdinalIgnoreCase))
                {
                    formData[formDataKey + "_" + ComplexFieldsNameConstants.FirstNameImmutable] = property.Value.ToString();
                }
                else if (property.Name.EndsWith("middleName", StringComparison.OrdinalIgnoreCase))
                {
                    formData[formDataKey + "_" + ComplexFieldsNameConstants.MiddleNameImmutable] = property.Value.ToString();
                }
                else if (property.Name.EndsWith("familyName", StringComparison.OrdinalIgnoreCase))
                {
                    formData[formDataKey + "_" + ComplexFieldsNameConstants.LastNameImmutable] = property.Value.ToString();
                }
                else if (property.Name.EndsWith("identifier", StringComparison.OrdinalIgnoreCase))
                {
                    if (formData.ContainsKey(formDataKey + "_" + ComplexFieldsNameConstants.IdentifierImmutable))
                    {
                        formData[formDataKey + "_" + ComplexFieldsNameConstants.IdentifierImmutable] =
                            formData[formDataKey + "_" + ComplexFieldsNameConstants.IdentifierImmutable] + ":" + property.Value;
                    }
                    else
                    {
                        formData[formDataKey + "_identifierImmutable"] = property.Value.ToString();
                    }
                }
                else if (property.Name.EndsWith("typeSelect", StringComparison.OrdinalIgnoreCase))
                {
                    int readIdentifierCode = EFormsIdentifierTypes[property.Value.ToString()];

                    if (formData.ContainsKey(formDataKey + "_" + ComplexFieldsNameConstants.IdentifierImmutable))
                    {
                        formData[formDataKey + "_" + ComplexFieldsNameConstants.IdentifierImmutable] =
                            readIdentifierCode + ":" + formData[formDataKey + "_" + ComplexFieldsNameConstants.IdentifierImmutable];
                    }
                    else
                    {
                        formData[formDataKey + "_" + ComplexFieldsNameConstants.IdentifierImmutable] = readIdentifierCode.ToString();
                    }
                }
            }
        }

        private static void ImportCode(Dictionary<string, StringValues> formData, JsonElement element, string formDataKey)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.Name.EndsWith("code", StringComparison.OrdinalIgnoreCase) ||
                    property.Name.EndsWith("value", StringComparison.OrdinalIgnoreCase))
                {
                    formData[formDataKey] = property.Value.ToString();
                }
            }
        }

        private static void ImportMultiselect(Dictionary<string, StringValues> formData, JsonElement element, string formDataKey)
        {
            formData[formDataKey] = string.Join(',', element.EnumerateArray().Select(e => e.GetProperty("value").GetRawText()));
        }

        private async Task ImportCompany(Dictionary<string, StringValues> formData, JsonElement element, string formDataKey)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.Name.EndsWith("LegalName", StringComparison.OrdinalIgnoreCase))
                {
                    formData[formDataKey + "_" + ComplexFieldsNameConstants.CompanyNameImmutable] = property.Value.ToString();
                }
                else if (property.Name.Contains("Eik", StringComparison.OrdinalIgnoreCase) ||
                         property.Name.Contains("vatnumber", StringComparison.OrdinalIgnoreCase) ||
                         property.Name.Contains("bulstat", StringComparison.OrdinalIgnoreCase))
                {
                    //var companyInfo = ImportPersonIdentifier(formData, property.Value, formDataKey + "_companyNumberImmutable");

                    SaveOperationResult companyData = await GetCompanyData(property.Value.ToString(), formData["_administrationIdentifier"].ToString());

                    if (!companyData.IsSuccess)
                    {
                        throw new ArgumentException(companyData.ErrorMessage);
                    }

                    formData[formDataKey + "_" + ComplexFieldsNameConstants.CompanyNumberImmutable] = (int)companyData.AddedObjectId + ":" + property.Value;

                    if ((int)companyData.AddedObjectId == (int)CidTypes.BULSTAT)
                    {
                        formData[formDataKey + "_" + ComplexFieldsNameConstants.LegalFormBulstatImmutable] = ((GetCompanyInfoResponse)companyData.CustomObject).LegalFormCode.ToString();
                    }
                    else if ((int)companyData.AddedObjectId == (int)CidTypes.EIK)
                    {
                        formData[formDataKey + "_" + ComplexFieldsNameConstants.LegalFormEIKImmutable] = ((GetCompanyInfoResponse)companyData.CustomObject).LegalFormCode.ToString();
                    }
                }
            }
        }

        private async Task<IntegrationServiceContextInfo> GetRegexContextInfo(string administrationUic)
        {
            string administration = administrationUic;
            try
            {
                GetAdministrationResponse administrationResponse
                    = await _registerGrpcClient.GetAdministrationNameByUicAsync(new StringValue()
                    {
                        Value = administrationUic
                    });

                if (administrationResponse.Status.Code == ResultCodes.Ok)
                {
                    administration = administrationResponse.Data.Name;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Проблем при извличане на данни от {nameof(_registerGrpcClient.GetAdministrationNameByUicAsync)} в {nameof(GetRegexContextInfo)}");
            }

            return new IntegrationServiceContextInfo()
            {
                EmployeeAdministration = administration,
                EmployeeNames = "Услуга към системата за електронно връчване",
                EmployeePosition = "Услуга към системата за електронно връчване"
            };
        }

        private async Task<SaveOperationResult> GetCompanyData(string cid, string administrationId)
        {
            var validationResult = PidValidateService.ValidateCompanyId(cid, (int)CidTypes.EIK);//TODO

            if (!validationResult)
            {
                return new SaveOperationResult($"{cid} е невалиден идентификатор");
            }

            var cidType = CidTypes.EIK;

            var contextInfo = await GetRegexContextInfo(administrationId);

            GetCompanyInfoRequest request = new GetCompanyInfoRequest()
            {
                Cid = cid,
                CidType = (int)cidType,
                ContextInfo = contextInfo
            };

            GetCompanyInfoResponse response = await _integrationGrpcClient.GetCompanyInfoAsync(request);

            if (response.ResultStatus.Code != ResultCodes.Ok)
            {
                cidType = CidTypes.BULSTAT;
                request = new GetCompanyInfoRequest()
                {
                    Cid = cid,
                    CidType = (int)cidType,
                    ContextInfo = contextInfo
                };

                response = await _integrationGrpcClient.GetCompanyInfoAsync(request);

                if (response.ResultStatus.Code != ResultCodes.Ok)
                {
                    _logger.LogError(
                        $"Не може да се извлекат данни за компания в {nameof(GetCompanyData)} {response.ResultStatus.Message}");
                    return new SaveOperationResult("Проблем при извличане на данни за компания");
                }
            }

            await _regixReportService.CreateRegixReport(
                JsonSerializer.Serialize(request),
                JsonSerializer.Serialize(response),
                ((int)RegixRequestTypes.DataRequestForCompany).ToString());

            return new SaveOperationResult(true, (int)cidType)
            {
                CustomObject = response
            };
        }

        private static (int, string) ImportPersonIdentifier(Dictionary<string, StringValues> formData, JsonElement element, string formDataKey)
        {
            if (element.TryGetProperty("identifier", out var identifier)
                && element.TryGetProperty("identifierType", out var identifierType)
                && identifierType.TryGetProperty("code", out var identifierCode))
            {
                int readIdentifierCode = EFormsIdentifierTypes[identifierCode.ToString()];

                formData[formDataKey] = new StringValues($"{readIdentifierCode}:{identifier}");
                return (readIdentifierCode, identifier.ToString());
            }

            return (0, string.Empty);
        }

        public static Dictionary<string, int> EFormsIdentifierTypes = new Dictionary<string, int>()
        {
            { "1", (int)PidTypes.EGN },
            { "2", (int)PidTypes.LNCH },
            { "6", (int)CidTypes.EIK },
            { "7", (int)CidTypes.BULSTAT },
            { "idn", (int)PidTypes.EGN },
            { "icn", (int)PidTypes.LNCH },
            { "1006-100002", (int)CidTypes.EIK },
            { "1006-100003", (int)CidTypes.BULSTAT },
            { "1006-100004", (int)PidTypes.EGN },
            { "1006-100005", (int)PidTypes.LNCH },
        };

        //TODO : да се измисли нещо друго
        private int DetermineTimezoneOffsetOfEform(Dictionary<string, StringValues> formData)
        {
            DateTimeOffset result = new DateTimeOffset();

            KeyValuePair<string, StringValues> dateValue = formData.FirstOrDefault(v =>
                !string.IsNullOrEmpty(v.Value.ToString()) &&
                DateTimeOffset.TryParseExact(
                    v.Value.ToString(),
                    FormattingConstant.EFormDateFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out result));

            return -(int)(result.Offset.TotalMinutes);
        }

        private string ResolvePdfJson(MemoryStream fileStream)
        {
            try
            {

                PdfDocument pdfDoc = new(new PdfReader(fileStream));
                int objNumber = pdfDoc.GetNumberOfPdfObjects();

                for (int i = 1; i <= objNumber; i++)
                {
                    PdfObject obj = pdfDoc.GetPdfObject(i);

                    if (obj != null && obj.IsDictionary())
                    {
                        PdfDictionary dict = (PdfDictionary)obj;

                        //с изчистени параметри до необходимите и видими полета при попълване на заявление. Този вариант е по-подходящ за работа при интеграции система-система.
                        var key = new PdfName("application.json_json");

                        if (dict.ContainsKey(key))
                        {
                            return dict.GetAsString(key).ToUnicodeString();
                        }

                        //съдържа пълната информация за уеб формата на дадена услуга (оставена от разработчика с цел при нужда от проверки или корекции)
                        //var key = new PdfName("application.json_submission");

                        //if (dict.ContainsKey(key))
                        //{
                        //    return dict.GetAsString(key).ToUnicodeString();
                        //}
                    }
                }

                return string.Empty;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Проблем при извличане на данни от Pdf в {nameof(ResolvePdfJson)}");
                return string.Empty;
            }
        }
        [HttpPost("import-edelivery-file")]
        [Display(Name = "Импорт на данни за заявена услуга чрез Е-Връчване")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public async Task<IActionResult> ImportEDeliveryFile(EDeliveryMessageVM model)
        {
            await _processService.ImportEDeliveryFile(model);
            return StatusCode(StatusCodes.Status200OK);
        }
    }
}
