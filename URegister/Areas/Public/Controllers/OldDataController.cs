using BlueCardPortal.Infrastructure.Validation;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
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
using URegister.Infrastructure.Model.RegisterForms;
using URegister.IntegrationsCatalog;
using URegister.NomenclaturesCatalog;
using static URegister.IntegrationsCatalog.IntegrationGrpc;

namespace URegister.Areas.Public.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Display(Name = "Импортиране на стари данни")]
    public class OldDataController : BaseController
    {
        private readonly IFormConfigurationPersistenceService _formConfigurationPersistenceService;
        private readonly IServiceService _serviceService;
        private readonly IProcessService _processService;
        private readonly IntegrationGrpcClient _integrationGrpcClient;
        private readonly IFormValidationService _formValidationService;
        private readonly IRegisterService _registerService;
        private readonly NomenclatureGrpc.NomenclatureGrpcClient _nomenclatureGrpcClient;
        private readonly ILogger<OldDataController> _logger;

        public OldDataController(
            IFormConfigurationPersistenceService formConfigurationPersistenceService,
            IServiceService serviceService,
            IProcessService processService,
            IntegrationGrpcClient integrationGrpcClient,
            IFormValidationService formValidationService,
            IRegisterService registerService,
            NomenclatureGrpc.NomenclatureGrpcClient nomenclatureGrpcClient,
            ILogger<OldDataController> logger)
        {
            _formConfigurationPersistenceService = formConfigurationPersistenceService;
            _serviceService = serviceService;
            _processService = processService;
            _integrationGrpcClient = integrationGrpcClient;
            _formValidationService = formValidationService;
            _registerService = registerService;
            _nomenclatureGrpcClient = nomenclatureGrpcClient;
            _logger = logger;
        }

        private Dictionary<string, string> GetKeys(ExcelWorksheet sheet)
        {
            var result = new Dictionary<string, string>();
            var colCount = sheet.Dimension.Columns;
            var lenght = 26;//???
            for (int i = 0; i < colCount; i++)
            {
                var colCode = string.Empty;
                var prefix = i / lenght;
                if (prefix > 0)
                {
                    colCode += (char)(((int)'A') + prefix - 1);
                }
                colCode += (char)(((int)'A') + i - prefix * lenght);
                var val = sheet.Cells[$"{colCode}1"].Value?.ToString();
                if (string.IsNullOrWhiteSpace(val))
                {
                    return result;
                }
                result.Add(val ?? string.Empty, $"{colCode}");
            }
            return result;
        }

        /// <summary>
        /// Импорт на данни за заявена услуга от файл [R00001]
        /// </summary>
        /// <param name="file">Pdf файл с json данни на заявена услуга.</param>
        //[HttpPost("import-excel-file-for-r00001")]
        //[Display(Name = "Импорт на данни за заявена услуга от файл [R00001]")]
        //[Consumes("multipart/form-data")]
        //public async Task<IActionResult> ImportExcelFileForR00001(IFormFile file, CancellationToken cancellationToken)
        //{
        //    List<string> errors = new List<string>();
        //    List<int> addedRows = new List<int>();

        //    List<Dictionary<string, string>> data = await ExcelToDictionary(file);

        //    ServiceVM registerServiceVM = await _serviceService.GetRegisterService();

        //    if (registerServiceVM == null)
        //    {
        //        return BadRequest("Не е намерена услуга за вписване или формата асоциирана с нея");
        //    }

        //    int rowNumber = 1;
        //    foreach (Dictionary<string, string> row in data)
        //    {
        //        rowNumber++;

        //        try
        //        {
        //            if (cancellationToken.IsCancellationRequested)
        //            {
        //                return StatusCode(499, "Request was canceled by the client.");
        //            }

        //            FormViewModel viewModel =
        //                await _formConfigurationPersistenceService.GetFormViewModel(registerServiceVM.FormParentId, true);

        //            List<string> values = row.Values.ToList();

        //            if (string.IsNullOrWhiteSpace(values[0]))
        //            {
        //                continue;
        //            }

        //            string oldIncomingNumber = values[0].Trim(new char[] { ' ', '№' });

        //            ProcessVM alreadyImported = await _processService.GetProcessByOldIncomingNumber(oldIncomingNumber);

        //            if (alreadyImported != null)
        //            {
        //                continue;
        //            }

        //            var company = viewModel.FormFields.First(f => f.Name == "Designation");

        //            bool companyImported = await ImportCompany(values[2], company, errors, rowNumber);

        //            if (!companyImported)
        //            {
        //                continue;
        //            }
                    
        //            bool isViewModelValidationSuccess = await _formValidationService.ValidateViewModel(
        //                viewModel,
        //                _nomenclatureGrpcClient,
        //                await _registerService.GetCurrentRegisterId(),
        //                null,
        //                true);

        //            if (!isViewModelValidationSuccess)
        //            {
        //                string validationError = string.Join(',',
        //                    (await _formValidationService.GetValidatedFormFieldsErrors(viewModel)));
        //                errors.Add($"ред {rowNumber}, {validationError}");
        //                continue;
        //            }

        //            var serviceStep = registerServiceVM.Steps.Where(x => x.StatusId == (int)ProcessStatus.Registered)
        //                .First();
        //            var stepVM = await _serviceStep.OrderNum, StepVM(Guid.Empty, null, registerServiceVM.Id,
        //                serviceStep.Id,
        //                serviceStep.OrderNum, oldIncomingNumber, null, viewModel, false);
        //            stepVM.ProcessInfo.PreferredResultDeliveryMethod = ChannelType.OnDesk;
        //            (ProcessStepVM addedStep, _) = await _processService.AddStep(
        //                stepVM,
        //                "000695160");

        //            addedRows.Add(rowNumber);
        //        }
        //        catch (Exception e)
        //        {
        //            _logger.LogError(e, nameof(ImportExcelFileForR00001));
        //            errors.Add($"ред {rowNumber}, {e.Message} {e.InnerException?.Message}. Стойности на реда: {string.Join("; ", row.Values)}");
        //        }

        //        _processService.ClearTracker();
        //    }

        //    string errorMessage = string.Join(Environment.NewLine, errors);
        //    string addedRowsString = string.Join(", ", addedRows);

        //    _logger.LogInformation($"Добавени редове в {nameof(ImportExcelFileForR00001)} {addedRowsString}");

        //    return new ContentResult
        //    {
        //        Content = $"Добавени {addedRows.Count} записи. Грешки: {Environment.NewLine}{errorMessage}",
        //        ContentType = "text/plain",
        //        StatusCode = 200
        //    };
        //}

        ///// <summary>
        ///// Импорт на данни за заявена услуга от файл нови тютюневи изделия
        ///// </summary>
        ///// <param name="file">Pdf файл с json данни на заявена услуга.</param>
        //[HttpPost("import-excel-file-for-e-cigarettes")]
        //[Display(Name = "Импорт на данни за заявена услуга от файл електронни цигари и контейнери за многократно пълнене")]
        //[Consumes("multipart/form-data")]
        //public async Task<IActionResult> ImportExcelFileForECigarettes(IFormFile file, CancellationToken cancellationToken)
        //{
        //    List<string> errors = new List<string>();
        //    List<int> addedRows = new List<int>();

        //    int sheetCount = await GetExcelSheetCount(file);

        //    for (int sheetIndex = 0; sheetIndex < sheetCount; sheetIndex++)
        //    {

        //        string oldDateNotification = null;

        //        List<Dictionary<string, string>> data = await ExcelToDictionary(file, sheetIndex);

        //        ServiceVM registerServiceVM = await _serviceService.GetRegisterService();

        //        if (registerServiceVM == null)
        //        {
        //            return BadRequest("Не е намерена услуга за вписване или формата асоциирана с нея");
        //        }

        //        int rowNumber = 1;
        //        foreach (Dictionary<string, string> row in data)
        //        {
        //            rowNumber++;

        //            try
        //            {
        //                if (cancellationToken.IsCancellationRequested)
        //                {
        //                    return StatusCode(499, "Request was canceled by the client.");
        //                }

        //                FormViewModel viewModel =
        //                    await _formConfigurationPersistenceService.GetFormViewModel(registerServiceVM.FormParentId,
        //                        true);

        //                List<string> values = row.Values.ToList();

        //                if (string.IsNullOrWhiteSpace(values[0]))
        //                {
        //                    continue;
        //                }

        //                string dateInformationProvision = ReadExcelDate(values[0].Trim());
        //                var dateInformationProvisionField =
        //                    viewModel.FormFields.First(f => f.Name == "dateInformationProvision");
        //                dateInformationProvisionField.Value = dateInformationProvision;

        //                _logger.LogInformation(
        //                    $"Импорт в {nameof(ImportExcelFileForECigarettes)} на ред {rowNumber}, {nameof(dateInformationProvisionField)} стойност {dateInformationProvisionField.Value}");

        //                var dateNotificationField = viewModel.FormFields.First(f => f.Name == "dateNotification");
        //                if (string.IsNullOrWhiteSpace(values[8]))
        //                {
        //                    dateNotificationField.Value = oldDateNotification;
        //                }
        //                else
        //                {
        //                    string dateNotification = ReadExcelDate(values[8].Trim());
        //                    oldDateNotification = dateNotification;
        //                    dateNotificationField.Value = dateNotification;
        //                }

        //                _logger.LogInformation(
        //                    $"Импорт в {nameof(ImportExcelFileForECigarettes)} на ред {rowNumber}, {nameof(dateNotificationField)} стойност {dateNotificationField.Value}");

        //                string Manufacturer = values[1].Trim();
        //                var ManufacturerField = viewModel.FormFields.First(f => f.Name == "Manufacturer");
        //                ManufacturerField.Value = Manufacturer;

        //                _logger.LogInformation(
        //                    $"Импорт в {nameof(ImportExcelFileForECigarettes)} на ред {rowNumber}, {nameof(ManufacturerField)} стойност {ManufacturerField.Value}");

        //                var company = viewModel.FormFields.First(f => f.Name == "BasicData");

        //                if (/*!string.IsNullOrWhiteSpace(company.Value) && */!string.IsNullOrWhiteSpace(values[9]))
        //                {
        //                    bool companyImported = await ImportCompany(values[9], company, errors, rowNumber);

        //                    if (!companyImported)
        //                    {
        //                        _logger.LogError($"Не може да импортира компания с id {values[9]}, ред {rowNumber}");
        //                    }
        //                }

        //                string companyName = values[2].Trim(' ', '-');
        //                if (!string.IsNullOrWhiteSpace(companyName))
        //                {
        //                    var companyNameField =
        //                        company.Fields.First(f => f.Name == "BasicData_companyNameImmutable");
        //                    if (sheetIndex > 0)
        //                    {
        //                        companyNameField.Value = companyName.Replace(" ЕООД", string.Empty);
        //                    }
        //                    else
        //                    {
        //                        companyNameField.Value = Manufacturer.Replace(" ЕООД", string.Empty);
        //                    }
                                
        //                    var importerNameField =
        //                        viewModel.FormFields.First(f => f.Name == "responsiblePersonName");

        //                    importerNameField.Value = companyName;
        //                    _logger.LogInformation(
        //                        $"Импорт в {nameof(ImportExcelFileForECigarettes)} на ред {rowNumber}, {nameof(companyNameField)} стойност {companyNameField.Value}");
        //                    _logger.LogInformation(
        //                        $"Импорт в {nameof(ImportExcelFileForECigarettes)} на ред {rowNumber}, companyPid стойност {company.Fields.First(f => f.Name == "BasicData_companyNumberImmutable").Value}");
        //                }

        //                string idProduct = values[3].Trim();
        //                var idProductField = viewModel.FormFields.First(f => f.Name == "idProduct");
        //                idProductField.Value = idProduct;

        //                string trademark = values[4].Trim();
        //                var trademarkField = viewModel.FormFields.First(f => f.Name == "trademark");
        //                trademarkField.Value = trademark;

        //                string VarietyProduct = values[5].Trim();
        //                var VarietyProductField = viewModel.FormFields.First(f => f.Name == "VarietyProduct");
        //                VarietyProductField.Value = VarietyProduct;

        //                var ElectronicCigarettesRefillableContainersField = viewModel.FormFields.First(f => f.Name == "ElectronicCigarettesRefillableContainers");


        //                string productType = values[6].Trim(' ', '.');
        //                if (productType.EndsWith("еднократна употреба", StringComparison.InvariantCultureIgnoreCase))
        //                {
        //                    ElectronicCigarettesRefillableContainersField.Value = "1";
        //                }
        //                else if (productType.Contains("йство. Всяка презареждаща се, която може да се използва и за многократно пълнене, трябва да се докладва в категорият", StringComparison.InvariantCultureIgnoreCase))
        //                {
        //                    ElectronicCigarettesRefillableContainersField.Value = "2";
        //                } 
        //                else if (productType.Contains("(фиксирана комбинация). Всяка презареждащ", StringComparison.InvariantCultureIgnoreCase))
        //                {
        //                    ElectronicCigarettesRefillableContainersField.Value = "3";
        //                }
        //                else if (productType.Contains("за многократно пълнене, пусната на пазара с един вид течност", StringComparison.InvariantCultureIgnoreCase))
        //                {
        //                    ElectronicCigarettesRefillableContainersField.Value = "4";
        //                }
        //                else if (productType.Contains("за многократно пълнене, само устройство", StringComparison.InvariantCultureIgnoreCase))
        //                {
        //                    ElectronicCigarettesRefillableContainersField.Value = "5";
        //                }
        //                else if (productType.Contains("тделна част от електронна", StringComparison.InvariantCultureIgnoreCase))
        //                {
        //                    ElectronicCigarettesRefillableContainersField.Value = "6";
        //                }
        //                else if (productType.Contains("тделен съд за електронна", StringComparison.InvariantCultureIgnoreCase))
        //                {
        //                    ElectronicCigarettesRefillableContainersField.Value = "6";
        //                }
        //                else if (productType.Contains("Комплект", StringComparison.InvariantCultureIgnoreCase))
        //                {
        //                    ElectronicCigarettesRefillableContainersField.Value = "7";
        //                }
        //                else if (productType.Contains("Контейнер за многократно пълнене", StringComparison.InvariantCultureIgnoreCase))
        //                {
        //                    ElectronicCigarettesRefillableContainersField.Value = "8";
        //                }
        //                else if (productType.Contains("Друго", StringComparison.InvariantCultureIgnoreCase))
        //                {
        //                    ElectronicCigarettesRefillableContainersField.Value = "9";
        //                }
        //                else
        //                {
        //                    errors.Add($"Таб {sheetIndex + 1}, ред {rowNumber}, непознат тип цигарено изделие {productType}");
        //                    continue;
        //                }

        //                string ingredients = values[7].Trim();
        //                var ingredientsField = viewModel.FormFields.First(f => f.Name == "ingredients");
        //                ingredientsField.Value = ingredients;

        //                bool isViewModelValidationSuccess = await _formValidationService.ValidateViewModel(
        //                    viewModel,
        //                    _nomenclatureGrpcClient,
        //                    await _registerService.GetCurrentRegisterId(),
        //                    null,
        //                    true);

        //                if (!isViewModelValidationSuccess)
        //                {
        //                    string validationError = string.Join(',',
        //                        (await _formValidationService.GetValidatedFormFieldsErrors(viewModel)));
        //                    errors.Add($"ред {rowNumber}, {validationError}");
        //                    continue;
        //                }

        //                string oldIncomingNumber = string.Empty;

        //                var serviceStep = registerServiceVM.Steps
        //                    .Where(x => x.StatusId == (int)ProcessStatus.Registered)
        //                    .First();
        //                var stepVM = await _processService.ToProcessStepVM(Guid.Empty, null, registerServiceVM.Id,
        //                    serviceStep.Id,
        //                    serviceStep.OrderNum, oldIncomingNumber, null, viewModel, false);

        //                //_logger.LogInformation("ViewModel за импорт: " + JsonSerializer.Serialize(viewModel));
        //                //_logger.LogInformation("Модел на стъпка за импорт: " + JsonSerializer.Serialize(stepVM));

        //                stepVM.ProcessInfo.PreferredResultDeliveryMethod = ChannelType.OnDesk;
        //                (ProcessStepVM addedStep, _) = await _processService.AddStep(
        //                    stepVM,
        //                    "177549105");

        //                addedRows.Add(rowNumber);
        //            }
        //            catch (Exception e)
        //            {
        //                _logger.LogError(e, nameof(ImportExcelFileForECigarettes));
        //                errors.Add(
        //                    $"таб {sheetIndex + 1}, ред {rowNumber}, {e.Message} {e.InnerException?.Message}. Стойности на реда: {string.Join("; ", row.Values)}");
        //            }

        //            _processService.ClearTracker();
        //        }
        //    }

        //    string errorMessage = string.Join(Environment.NewLine, errors);
        //    string addedRowsString = string.Join(", ", addedRows);

        //    _logger.LogInformation($"Добавени редове в {nameof(ImportExcelFileForECigarettes)} {addedRowsString}");

        //    return new ContentResult
        //    {
        //        Content = $"Добавени {addedRows.Count} записи. Грешки: {Environment.NewLine}{errorMessage}",
        //        ContentType = "text/plain",
        //        StatusCode = 200
        //    };
        //}

        ////Методът е изпълнен и импорта е успешен. Закоментиран е за да се предотврати погрешно изпълнение
        
        ///// <summary>
        ///// Импорт на данни за заявена услуга от файл нови тютюневи изделия
        ///// </summary>
        ///// <param name="file">Pdf файл с json данни на заявена услуга.</param>
        //[HttpPost("import-excel-file-for-new-tobacco-products")]
        //[Display(Name = "Импорт на данни за заявена услуга от файл нови тютюневи изделия")]
        //[Consumes("multipart/form-data")]
        //public async Task<IActionResult> ImportExcelFileForNewTobaccoProducts(IFormFile file, CancellationToken cancellationToken)
        //{
        //    List<string> errors = new List<string>();
        //    List<int> addedRows = new List<int>();

        //    List<Dictionary<string, string>> data = await ExcelToDictionary(file);

        //    ServiceVM registerServiceVM = await _serviceService.GetRegisterService();

        //    if (registerServiceVM == null)
        //    {
        //        return BadRequest("Не е намерена услуга за вписване или формата асоциирана с нея");
        //    }

        //    int rowNumber = 1;
        //    foreach (Dictionary<string, string> row in data)
        //    {
        //        rowNumber++;

        //        try
        //        {
        //            if (cancellationToken.IsCancellationRequested)
        //            {
        //                return StatusCode(499, "Request was canceled by the client.");
        //            }

        //            FormViewModel viewModel =
        //                await _formConfigurationPersistenceService.GetFormViewModel(registerServiceVM.FormParentId, true);

        //            List<string> values = row.Values.ToList();

        //            if (string.IsNullOrWhiteSpace(values[0]))
        //            {
        //                continue;
        //            }

        //            string dateInformationProvision = ReadExcelDate(values[0].Trim());
        //            var dateInformationProvisionField = viewModel.FormFields.First(f => f.Name == "dateInformationProvision");
        //            dateInformationProvisionField.Value = dateInformationProvision;

        //            _logger.LogInformation($"Импорт в {nameof(ImportExcelFileForNewTobaccoProducts)} на ред {rowNumber}, {nameof(dateInformationProvisionField)} стойност {dateInformationProvisionField.Value}");

        //            string dateNotification = ReadExcelDate(values[1].Trim());
        //            var dateNotificationField = viewModel.FormFields.First(f => f.Name == "dateNotification");
        //            dateNotificationField.Value = dateNotification;

        //            _logger.LogInformation($"Импорт в {nameof(ImportExcelFileForNewTobaccoProducts)} на ред {rowNumber}, {nameof(dateNotificationField)} стойност {dateNotificationField.Value}");

        //            string Manufacturer = values[2].Trim();
        //            var ManufacturerField = viewModel.FormFields.First(f => f.Name == "Manufacturer");
        //            ManufacturerField.Value = Manufacturer;

        //            _logger.LogInformation($"Импорт в {nameof(ImportExcelFileForNewTobaccoProducts)} на ред {rowNumber}, {nameof(ManufacturerField)} стойност {ManufacturerField.Value}");

        //            var company = viewModel.FormFields.First(f => f.Name == "BasicData");
        //            bool companyImported = await ImportCompany(values[9], company, errors, rowNumber);

        //            if (!companyImported)
        //            {
        //                _logger.LogError($"Не може да импортира компания с id {values[9]}, ред {rowNumber}");
        //                errors.Add($"Ред {rowNumber} Не може да импортира компания с id {values[9]}, ред {rowNumber}");
        //                continue;
        //            }

        //            string companyName = values[3].Trim();
        //            var companyNameField = company.Fields.First(f => f.Name == "BasicData_companyNameImmutable");
        //            companyNameField.Value = companyName;

        //            var importedAddress = viewModel.FormFields.First(f => f.Name == "ImportedAddress");

        //            viewModel.FormFields.First(f => f.Name == "responsiblePersonName").Value =
        //                company.Fields.First(f => f.Name == "BasicData_companyNameImmutable").Value;

        //            importedAddress.Fields.First(f => f.Name == "ImportedAddress_countryImmutable").Value =
        //                company.Fields.First(f => f.Name == "BasicData_countryImmutable").Value;

        //            importedAddress.Fields.First(f => f.Name == "ImportedAddress_settlementImmutable").Value =
        //                company.Fields.First(f => f.Name == "BasicData_settlementImmutable").Value;

        //            importedAddress.Fields.First(f => f.Name == "ImportedAddress_postalCodeImmutable").Value =
        //                company.Fields.First(f => f.Name == "BasicData_postalCodeImmutable").Value;

        //            importedAddress.Fields.First(f => f.Name == "ImportedAddress_regionImmutable").Value =
        //                company.Fields.First(f => f.Name == "BasicData_regionImmutable").Value;

        //            importedAddress.Fields.First(f => f.Name == "ImportedAddress_streetImmutable").Value =
        //                company.Fields.First(f => f.Name == "BasicData_streetImmutable").Value;

        //            importedAddress.Fields.First(f => f.Name == "ImportedAddress_buildingNumberImmutable").Value =
        //                company.Fields.First(f => f.Name == "BasicData_buildingNumberImmutable").Value;

        //            importedAddress.Fields.First(f => f.Name == "ImportedAddress_entranceNumberImmutable").Value =
        //                company.Fields.First(f => f.Name == "BasicData_entranceNumberImmutable").Value;

        //            importedAddress.Fields.First(f => f.Name == "ImportedAddress_floorImmutable").Value =
        //                company.Fields.First(f => f.Name == "BasicData_floorImmutable").Value;

        //            importedAddress.Fields.First(f => f.Name == "ImportedAddress_apartmentNumberImmutable").Value =
        //                company.Fields.First(f => f.Name == "BasicData_apartmentNumberImmutable").Value;

        //            importedAddress.Fields.First(f => f.Name == "ImportedAddress_addressAbroadImmutable").Value =
        //                company.Fields.First(f => f.Name == "BasicData_addressAbroadImmutable").Value;


        //            _logger.LogInformation($"Импорт в {nameof(ImportExcelFileForNewTobaccoProducts)} на ред {rowNumber}, {nameof(companyNameField)} стойност {companyNameField.Value}");
        //            _logger.LogInformation($"Импорт в {nameof(ImportExcelFileForNewTobaccoProducts)} на ред {rowNumber}, companyPid стойност {company.Fields.First(f => f.Name == "BasicData_companyNumberImmutable").Value}");


        //            string idProduct = values[4].Trim();
        //            var idProductField = viewModel.FormFields.First(f => f.Name == "idProduct");
        //            idProductField.Value = idProduct;

        //            string trademark = values[5].Trim();
        //            var trademarkField = viewModel.FormFields.First(f => f.Name == "trademark");
        //            trademarkField.Value = trademark;

        //            string VarietyProduct = values[6].Trim();
        //            var VarietyProductField = viewModel.FormFields.First(f => f.Name == "VarietyProduct");
        //            VarietyProductField.Value = VarietyProduct;

        //            var typesProductsField = viewModel.FormFields.First(f => f.Name == "typesProducts");
        //            typesProductsField.Value = "1";//Бездимно

        //            string ingredients = values[8].Trim();
        //            var ingredientsField = viewModel.FormFields.First(f => f.Name == "ingredients");
        //            ingredientsField.Value = ingredients;

        //            bool isViewModelValidationSuccess = await _formValidationService.ValidateViewModel(
        //                viewModel,
        //                _nomenclatureGrpcClient,
        //                await _registerService.GetCurrentRegisterId(),
        //                null,
        //                true);

        //            if (!isViewModelValidationSuccess)
        //            {
        //                string validationError = string.Join(',',
        //                    (await _formValidationService.GetValidatedFormFieldsErrors(viewModel)));
        //                errors.Add($"ред {rowNumber}, {validationError}");
        //                continue;
        //            }

        //            string oldIncomingNumber = string.Empty;

        //            var serviceStep = registerServiceVM.Steps.Where(x => x.StatusId == (int)ProcessStatus.Registered)
        //                .First();
        //            var stepVM = await _processService.ToProcessStepVM(Guid.Empty, null, registerServiceVM.Id,
        //                serviceStep.Id,
        //                serviceStep.OrderNum, oldIncomingNumber, null, viewModel, false);

        //            _logger.LogInformation("ViewModel за импорт: " + JsonSerializer.Serialize(viewModel));
        //            _logger.LogInformation("Модел на стъпка за импорт: " + JsonSerializer.Serialize(stepVM));

        //            stepVM.ProcessInfo.PreferredResultDeliveryMethod = ChannelType.OnDesk;
        //            (ProcessStepVM addedStep, _) = await _processService.AddStep(
        //                stepVM,
        //                "177549105");

        //            addedRows.Add(rowNumber);
        //        }
        //        catch (Exception e)
        //        {
        //            _logger.LogError(e, nameof(ImportExcelFileForNewTobaccoProducts));
        //            errors.Add($"ред {rowNumber}, {e.Message} {e.InnerException?.Message}. Стойности на реда: {string.Join("; ", row.Values)}");
        //        }

        //        _processService.ClearTracker();
        //    }

        //    string errorMessage = string.Join(Environment.NewLine, errors);
        //    string addedRowsString = string.Join(", ", addedRows);

        //    _logger.LogInformation($"Добавени редове в {nameof(ImportExcelFileForNewTobaccoProducts)} {addedRowsString}");

        //    return new ContentResult
        //    {
        //        Content = $"Добавени {addedRows.Count} записи. Грешки: {Environment.NewLine}{errorMessage}",
        //        ContentType = "text/plain",
        //        StatusCode = 200
        //    };
        //}

        private string ReadExcelDate(string excelDate)
        {
            string[] formats =
            {
                "d.MM.yyyy", 
                "d.M.yyyy", 
                "dd.M.yyyy", 
                FormattingConstant.NormalDateFormat, 
                FormattingConstant.DateTimeFormat, 
                "d.MM.yyyy HH:mm", 
                "d.MM.yyyy г. HH:mm", 
                "d.MM.yyyy г. H:mm", 
                "dd.MM.yyyy г. HH:mm",
                "dd.MM.yyyy г. H:mm", 
                "d.M.yyyy HH:mm", 
                "d.M.yyyy г. HH:mm", 
                "d.M.yyyy г. H:mm", 
                "dd.M.yyyy г. HH:mm",
                "dd.M.yyyy г. H:mm",

            };
            int spaceIndex = excelDate.IndexOf(' ');
            excelDate = spaceIndex >= 0 ? excelDate.Substring(0, spaceIndex) : excelDate;
            if (DateTime.TryParseExact(excelDate, formats, NumberFormatInfo.InvariantInfo, DateTimeStyles.None, out DateTime outDate))
            {
                return outDate.ToString(FormattingConstant.NormalDateFormat);
            }

            DateTime date = DateTime.FromOADate((int)double.Parse(excelDate));
            string formattedDate = date.ToString(FormattingConstant.NormalDateFormat);
            return formattedDate;
        }

        private async Task<bool> ImportCompany(string cidValue, FormField company, List<string> errors, int rowNumber)
        {
            string sanitizedCid = Regex.Replace(cidValue, "[^0-9]", "");
            SaveOperationResult companyInfoResult = await GetCompanyData(sanitizedCid);

            if (companyInfoResult.IsSuccess)
            {
                GetCompanyInfoResponse companyInfo = companyInfoResult.CustomObject as GetCompanyInfoResponse;

                var companyNumberField = company.Fields.FirstOrDefault(f => f.Name == company.Name + "_" + ComplexFieldsNameConstants.CompanyNumberImmutable);
                if (companyNumberField != null)
                {
                    companyNumberField.Value = (int)companyInfoResult.AddedObjectId + ":" + sanitizedCid;
                }

                if ((int)companyInfoResult.AddedObjectId == (int)CidTypes.BULSTAT)
                {
                    var legalFormBulstatField = company.Fields.FirstOrDefault(f => f.Name == company.Name + "_" + ComplexFieldsNameConstants.LegalFormBulstatImmutable);
                    if (legalFormBulstatField != null)
                    {
                        legalFormBulstatField.Value = companyInfo.LegalFormCode.ToString();
                    }
                }
                else if ((int)companyInfoResult.AddedObjectId == (int)CidTypes.EIK)
                {
                    var legalFormEIKField = company.Fields.FirstOrDefault(f => f.Name == company.Name + "_" + ComplexFieldsNameConstants.LegalFormEIKImmutable);
                    if (legalFormEIKField != null)
                    {
                        legalFormEIKField.Value = companyInfo.LegalFormCode.ToString();
                    }
                }

                var companyNameField = company.Fields.FirstOrDefault(f => f.Name == company.Name + "_" + ComplexFieldsNameConstants.CompanyNameImmutable);
                if (companyNameField != null)
                {
                    companyNameField.Value = companyInfo.Name;
                }

                var countryField = company.Fields.FirstOrDefault(f => f.Name == company.Name + "_countryImmutable");
                if (countryField != null)
                {
                    countryField.Value = companyInfo.CountryCode;
                }

                var settlementField = company.Fields.FirstOrDefault(f => f.Name == company.Name + "_settlementImmutable");
                if (settlementField != null)
                {
                    settlementField.Value = companyInfo.SettlementCode;
                }

                var postalCodeField = company.Fields.FirstOrDefault(f => f.Name == company.Name + "_postalCodeImmutable");
                if (postalCodeField != null)
                {
                    postalCodeField.Value = companyInfo.PostCode;
                }

                var regionField = company.Fields.FirstOrDefault(f => f.Name == company.Name + "_regionImmutable");
                if (regionField != null)
                {
                    regionField.Value = companyInfo.RegionCode;
                }

                var streetField = company.Fields.FirstOrDefault(f => f.Name == company.Name + "_streetImmutable");
                if (streetField != null)
                {
                    streetField.Value = companyInfo.StreetName;
                }

                var buildingNumberField = company.Fields.FirstOrDefault(f => f.Name == company.Name + "_buildingNumberImmutable");
                if (buildingNumberField != null)
                {
                    buildingNumberField.Value = companyInfo.BuildingNumber;
                }

                var entranceNumberField = company.Fields.FirstOrDefault(f => f.Name == company.Name + "_entranceNumberImmutable");
                if (entranceNumberField != null)
                {
                    entranceNumberField.Value = companyInfo.EntranceName;
                }

                var floorField = company.Fields.FirstOrDefault(f => f.Name == company.Name + "_floorImmutable");
                if (floorField != null)
                {
                    floorField.Value = companyInfo.FloorNumber;
                }

                var apartmentNumberField = company.Fields.FirstOrDefault(f => f.Name == company.Name + "_apartmentNumberImmutable");
                if (apartmentNumberField != null)
                {
                    apartmentNumberField.Value = companyInfo.ApartmentNumber;
                }

                var addressAbroadField = company.Fields.FirstOrDefault(f => f.Name == company.Name + "_addressAbroadImmutable");
                if (addressAbroadField != null)
                {
                    addressAbroadField.Value = companyInfo.ForeignAddress;
                }

                return true;
            }
            else
            {
                errors.Add($"ред {rowNumber}, БУЛСТАТ/ЕИК {cidValue}");
                return false;
                //var companyNumberField = company.Fields.FirstOrDefault(f => f.Name == company.Name + "_companyNumberImmutable");
                //if (companyNumberField != null)
                //{
                //    companyNumberField.Value = "2:" + sanitizedCid; //TODO
                //}

                //var companyNameField = company.Fields.FirstOrDefault(f => f.Name == company.Name + "_companyNameImmutable");
                //if (companyNameField != null)
                //{
                //    companyNameField.Value = values[1];
                //}

                //var addressAbroadField = company.Fields.FirstOrDefault(f => f.Name == company.Name + "_addressAbroadImmutable");
                //if (addressAbroadField != null)
                //{
                //    addressAbroadField.Value = values[3];
                //}
            }
        }

        private async Task<SaveOperationResult> GetCompanyData(string cid)
        {
            var validationResult = PidValidateService.ValidateCompanyId(cid, (int)CidTypes.EIK);//TODO

            if (!validationResult)
            {
                return new SaveOperationResult($"{cid} е невалиден идентификатор");
            }

            var cidType = CidTypes.EIK;

            var contextInfo = GetRegixContextInfo();

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

            //await _regixReportService.CreateRegixReport(
            //    JsonSerializer.Serialize(request),
            //    JsonSerializer.Serialize(response),
            //    ((int)RegixRequestTypes.DataRequestForCompany).ToString());

            _logger.LogInformation($"Данни за компания {response.Name} извлечени от Regix");

            return new SaveOperationResult(true, (int)cidType)
            {
                CustomObject = response
            };
        }

        private IntegrationServiceContextInfo GetRegixContextInfo()
        {
            return new IntegrationServiceContextInfo()
            {
                EmployeeAdministration = "Batch import",
                    //UserContext.AvailableAdministrations.FirstOrDefault(a => UserContext.AdministrationId.ToString() == a.Id)?.Name,
                EmployeeNames = "Batch import",//; UserContext.FirstName + " " + UserContext.LastName,
                EmployeePosition = "Batch import"//string.Join(", ", roles)
            };
        }


        private async Task<int> GetExcelSheetCount(IFormFile file)
        {
            try
            {
                var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                ExcelPackage.License.SetNonCommercialPersonal("ISCIPR");

                using (memoryStream)
                {
                    using (var package = new ExcelPackage(memoryStream))
                    {
                        return package.Workbook.Worksheets.Count;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(GetExcelSheetCount)}");
                throw;
            }
        }

        private async Task<List<Dictionary<string, string>>> ExcelToDictionary(IFormFile file, int worksheetIndex = 0)
        {
            var data = new List<Dictionary<string, string>>();

            try
            {
                var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                ExcelPackage.License.SetNonCommercialPersonal("ISCIPR");

                using (memoryStream)
                {
                    using (var package = new ExcelPackage(memoryStream))
                    {
                        var sheet = package.Workbook.Worksheets[worksheetIndex];
                        var keys = GetKeys(sheet);
                        var rowCount = sheet.Dimension.Rows;
                        for (int row = 2; row <= rowCount; row++)
                        {
                            var rowData = new Dictionary<string, string>();
                            var hasValue = false;
                            foreach (var kv in keys)
                            {
                                var val = sheet.Cells[$"{kv.Value}{row}"].Value?.ToString() ?? string.Empty;
                                rowData.Add(kv.Key, val);
                                if (!string.IsNullOrEmpty(val))
                                {
                                    hasValue = true;
                                }
                            }

                            // закоментирано за да имаме правилни номера на редове
                            //if (hasValue)
                            //{
                                data.Add(rowData);
                            //}
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(ExcelToDictionary)}");
            }

            return data;
        }

        ///// <summary>
        ///// Импорт на данни за заявена услуга от файл [R00036]
        ///// </summary>
        ///// <param name="file">Pdf файл с json данни на заявена услуга.</param>
        //[HttpPost("import-excel-file-for-r00036-zznn")]
        //[Display(Name = "Импорт на данни за заявена услуга от файл [R00036]")]
        //[Consumes("multipart/form-data")]
        //public async Task<IActionResult> ImportExcelFileForZZNNR0036(IFormFile file, CancellationToken cancellationToken)
        //{
        //    List<string> errors = new List<string>();
        //    List<int> addedRows = new List<int>();

        //    List<Dictionary<string, string>> data = await ExcelToDictionary(file);

        //    ServiceVM registerServiceVM = await _serviceService.GetRegisterService();

        //    if (registerServiceVM == null)
        //    {
        //        return BadRequest("Не е намерена услуга за вписване или формата асоциирана с нея");
        //    }

        //    int rowNumber = 1;
        //    foreach (Dictionary<string, string> row in data)
        //    {
        //        rowNumber++;

        //        try
        //        {
        //            if (cancellationToken.IsCancellationRequested)
        //            {
        //                return StatusCode(499, "Request was canceled by the client.");
        //            }

        //            FormViewModel viewModel =
        //                await _formConfigurationPersistenceService.GetFormViewModel(registerServiceVM.FormParentId, true);

        //            List<string> values = row.Values.ToList();

        //            if (string.IsNullOrWhiteSpace(values[0]))
        //            {
        //                continue;
        //            }

        //            //string oldIncomingNumber = values[0].Trim(new char[] { ' ', '№' });

        //            //ProcessVM alreadyImported = await _processService.GetProcessByOldIncomingNumber(oldIncomingNumber);

        //            //if (alreadyImported != null)
        //            //{
        //            //    continue;
        //            //}

        //            var company = viewModel.FormFields.First(f => f.Name == "PersonObliged");

        //            bool companyImported = await ImportCompany(values[2], company, errors, rowNumber);

        //            if (!companyImported)
        //            {
        //                continue;
        //            }

        //            //if (!string.IsNullOrWhiteSpace(companyName))
        //            //Взимаме от Regix според #405807
        //            //{
        //            //    var companyNameField =
        //            //        company.Fields.First(f => f.Name == "PersonObliged_companyNameImmutable");

        //            //    string companyName = values[1];

        //            //    companyNameField.Value = companyName.Replace(" ЕООД", string.Empty);

        //            //}

        //            string oldIncomingNumber = values[0].Trim();
        //            string[] numberAndDate =  oldIncomingNumber.Split(new char[] {'/'});

                    
        //            var numberField = viewModel.FormFields.First(f => f.Name == "regNumber");
        //            //numberField.Value = numberAndDate[1].Replace("г.", string.Empty);
        //            numberField.Value = RemoveFirstWordAfterQuote(numberAndDate[0]);

        //            string entryReasonAsString = values[3].Trim();
        //            var entryReasonField = viewModel.FormFields.First(f => f.Name == "EntryReason");

        //            if (entryReasonAsString == "чл. 23, ал. 2")
        //            {
        //                entryReasonField.Value = "1";
        //            }
        //            else if (entryReasonAsString == "чл. 23, ал. 3")
        //            {
        //                entryReasonField.Value = "2";
        //            }
        //            else if(entryReasonAsString is "чл. 23, ал. 2 и ал. 3" or "чл. 23, ал. 2 и 3" or "чл. 23, ал 2 и ал. 3")
        //            {
        //                entryReasonField.Value = "3";
        //            }
        //            else
        //            {
        //                errors.Add($"Непознато ОСНОВАНИЕ ЗА ВЪЗНИКВАНЕ НА ЗАДЪЛЖЕНИЕТО СЪГЛАСНО ЗЗНН {entryReasonAsString} ред {rowNumber}");
        //            }

        //            string periodAsString = values[4].Trim();
        //            var periodField = viewModel.FormFields.First(f => f.Name == "period");

        //            if (periodAsString == "01.07.2024 - 30.06.2025")
        //            {
        //                periodField.Value = "1";
        //            }
        //            else if (periodAsString == "01.07.2025 - 30.06.2026")
        //            {
        //                periodField.Value = "2";
        //            }
        //            else if (periodAsString == "01.07.2026 - 30.06.2027")
        //            {
        //                periodField.Value = "3";
        //            }
        //            else if (periodAsString == "01.07.2027 - 30.06.2028")
        //            {
        //                periodField.Value = "4";
        //            }
        //            else
        //            {
        //                errors.Add($"Непознат период {periodField} ред {rowNumber}");
        //            }

        //            bool isViewModelValidationSuccess = await _formValidationService.ValidateViewModel(
        //                    viewModel,
        //                    _nomenclatureGrpcClient,
        //                    await _registerService.GetCurrentRegisterId(),
        //                    null,
        //                    true);

        //            if (!isViewModelValidationSuccess)
        //            {
        //                string validationError = string.Join(',',
        //                    (await _formValidationService.GetValidatedFormFieldsErrors(viewModel)));
        //                errors.Add($"ред {rowNumber}, {validationError}");
        //                continue;
        //            }

        //            var serviceStep = registerServiceVM.Steps.Where(x => x.StatusId == (int)ProcessStatus.Registered)
        //                .First();
        //            var stepVM = await _processService.ToProcessStepVM(Guid.Empty, null, registerServiceVM.Id,
        //                serviceStep.Id,
        //                serviceStep.OrderNum, oldIncomingNumber, null, viewModel, false);
        //            stepVM.ProcessInfo.PreferredResultDeliveryMethod = ChannelType.OnDesk;
        //            (ProcessStepVM addedStep, _) = await _processService.AddStep(
        //                stepVM,
        //                "831913661");

        //            addedRows.Add(rowNumber);
        //        }
        //        catch (Exception e)
        //        {
        //            _logger.LogError(e, nameof(ImportExcelFileForZZNNR0036));
        //            errors.Add($"ред {rowNumber}, {e.Message} {e.InnerException?.Message}. Стойности на реда: {string.Join("; ", row.Values)}");
        //        }

        //        _processService.ClearTracker();
        //    }

        //    string errorMessage = string.Join(Environment.NewLine, errors);
        //    string addedRowsString = string.Join(", ", addedRows);

        //    _logger.LogInformation($"Добавени редове в {nameof(ImportExcelFileForZZNNR0036)} {addedRowsString}");

        //    return new ContentResult
        //    {
        //        Content = $"Добавени {addedRows.Count} записи. Грешки: {Environment.NewLine}{errorMessage}",
        //        ContentType = "text/plain",
        //        StatusCode = 200
        //    };
        //}

        /// <summary>
        /// Импорт на данни за заявена услуга от файл [R00030]
        /// </summary>
        /// <param name="file">Pdf файл с json данни на заявена услуга.</param>
        [HttpPost("import-excel-file-for-r00030-Emergency-situations")]
        [Display(Name = "Импорт на данни за заявена услуга от файл [R00030]")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportExcelFileForEmergencySituationsR0030(IFormFile file, CancellationToken cancellationToken)
        {
            List<string> errors = new List<string>();
            List<int> addedRows = new List<int>();

            List<Dictionary<string, string>> data = await ExcelToDictionary(file, 1);

            ServiceVM registerServiceVM = await _serviceService.GetRegisterService();

            if (registerServiceVM == null)
            {
                return BadRequest("Не е намерена услуга за вписване или формата асоциирана с нея");
            }

            var request = new NomenclaturePublicRequest()
            {
                RegisterId = 0,
                NomenclatureTypes = { "CL0057", "CL0009" },
            };

            NomenclaturePublicResponse response = await _nomenclatureGrpcClient.GetNomenclaturePublicAsync(request);

            if (response.ResultStatus.Code != ResultCodes.Ok)
            {
                errors.Add("Не може да извлече номенклатура");
                return new ContentResult
                {
                    Content = $"Не може да извлече номенклатура CL0057",
                    ContentType = "text/plain",
                    StatusCode = 200
                }; 
            }

            var nomenclatureFuelList = response.NomenclatureTypes.Last().CodeableConcepts;
            var nomenclatureEIKFromList = response.NomenclatureTypes.First().CodeableConcepts;

            string lastStorageNumber = string.Empty;
            int rowNumber = 1;
            for (int dataIndex = 0; dataIndex < data.Count; dataIndex++)
            {
                rowNumber++;
                var row = data[dataIndex];

                try
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return StatusCode(499, "Request was canceled by the client.");
                    }

                    FormViewModel viewModel =
                        await _formConfigurationPersistenceService.GetFormViewModel(registerServiceVM.FormParentId, true);

                    List<string> values = row.Values.ToList();

                    //todo
                    //if (lastStorageNumber != values[2])
                    //{

                    //}

                    if (string.IsNullOrWhiteSpace(values[0]))
                    {
                        continue;
                    }

                    //string oldIncomingNumber = values[0].Trim(new char[] { ' ', '№' });

                    //ProcessVM alreadyImported = await _processService.GetProcessByOldIncomingNumber(oldIncomingNumber);

                    //if (alreadyImported != null)
                    //{
                    //    continue;
                    //}


                    var emergencySuppliesField = viewModel.FormFields.First(f => f.Name == "emergencySupplies");
                    emergencySuppliesField.Value = "1";//Република България


                    var TypeoOfWarehouseField = viewModel.FormFields.First(f => f.Name == "TypeoOfWarehouse");
                    TypeoOfWarehouseField.Value = "1";//Съхранител със складове, регистрирани по члл. 38 от ЗЗНН

                    var warehouseNumberField = viewModel.FormFields.First(f => f.Name == "warehouseNumber");
                    warehouseNumberField.Value = values[2].Trim();

                    var StorekeeperField = viewModel.FormFields.First(f => f.Name == "Storekeeper");
                    var storekeeperValues = SplitAtLastSpace(values[3].Trim());

                    var storekeeperNameField = StorekeeperField.Fields.First(f => f.Name == "Storekeeper_companyNameImmutable");
                    storekeeperNameField.Value = storekeeperValues.Before;

                    var storekeeperLegalFormEIKField = StorekeeperField.Fields.First(f => f.Name == "Storekeeper_legalFormEIKImmutable");
                    var legalFormNom = nomenclatureEIKFromList.SingleOrDefault(l => l.Value.Equals(storekeeperValues.After, StringComparison.OrdinalIgnoreCase));

                    var storekeeperIdField = StorekeeperField.Fields.First(f => f.Name == "Storekeeper_companyNumberImmutable");
                    storekeeperIdField.Value = "1:" + values[12].Trim();

                    //var Storekeeper_companyNumberImmutableField = StorekeeperField.Fields.First(f => f.Name == "Storekeeper_companyNumberImmutable");
                    //Storekeeper_companyNumberImmutableField.Value = "1:";

                    if (legalFormNom == null)
                    {
                        errors.Add($"Ред {rowNumber}, {storekeeperValues.After} е непозната номенклатурна стойност");
                        continue;
                    }

                    storekeeperLegalFormEIKField.Value = legalFormNom.Code;
                    //

                    string warehouseAddressFull = values[4].Trim();

                    var warehouseAddressField = viewModel.FormFields.First(f => f.Name == "warehouseAddress");

                    var warehouseAddressStreet = warehouseAddressField.Fields.First(f => f.Name == "warehouseAddress_streetImmutable");
                    var warehouseAddressCountry = warehouseAddressField.Fields.First(f => f.Name == "warehouseAddress_countryImmutable");
                    var warehouseAddressSettlement = warehouseAddressField.Fields.First(f => f.Name == "warehouseAddress_settlementImmutable");

                    warehouseAddressSettlement.Value = values[13].Trim();

                    warehouseAddressStreet.Value = warehouseAddressFull.Substring(warehouseAddressFull.LastIndexOf(',') + 1).Trim();
                    warehouseAddressCountry.Value = "BG";

                    var storageNumberTechSchemaField = viewModel.FormFields.First(f => f.Name == "storageNumberTechSchema");
                    storageNumberTechSchemaField.Value = values[5].Trim();

                    var supplyTypeField = viewModel.FormFields.First(f => f.Name == "supplyType");

                    var supplyNom = nomenclatureFuelList.SingleOrDefault(l => l.Value.Equals(values[6].Trim(), StringComparison.OrdinalIgnoreCase));

                    if (supplyNom == null)
                    {
                        errors.Add($"Ред {rowNumber}, {values[6].Trim()} е непозната номенклатурна стойност");
                        continue;
                    }

                    supplyTypeField.Value = supplyNom.Code;

                    //повтроряеми
                    var NamePersonField = viewModel.FormFields.First(f => f.Name == "NamePerson");

                    if (!string.IsNullOrWhiteSpace(values[7].Trim()))
                    {
                        var NamePersonValues = SplitAtLastSpace(values[7].Trim());

                        var NamePersonNameField =
                            NamePersonField.Fields.First(n => n.Name == "NamePerson_companyNameImmutable");

                        var namePersonFormEIKField =
                            NamePersonField.Fields.First(f => f.Name == "NamePerson_legalFromEIKImmutable" ||
                                                              f.Name == "NamePerson_legalFormEIKImmutable");


                        if (values[7].Trim().StartsWith("ДА ДРВВЗ", StringComparison.InvariantCultureIgnoreCase))
                        {
                            NamePersonNameField.Value = "ДА ДРВВЗ";
                            var namePersonFormBulstatField =
                                NamePersonField.Fields.First(f => f.Name == "NamePerson_legalFromBulstatImmutable" ||
                                                                  f.Name == "NamePerson_legalFormBulstatImmutable");
                            namePersonFormBulstatField.Value = "1216";
                        }
                        else
                        {
                            var legalFormNamePersonNom = nomenclatureEIKFromList
                                .SingleOrDefault(l =>
                                    l.Value.Equals(NamePersonValues.After, StringComparison.OrdinalIgnoreCase));
                            if (legalFormNamePersonNom == null)
                            {
                                NamePersonNameField.Value = values[7].Trim();
                                errors.Add(
                                    $"Ред {rowNumber}, не може да извлече правна форма от {values[7].Trim()}, да се коригира ръчно");
                            }
                            else
                            {
                                namePersonFormEIKField.Value = legalFormNamePersonNom.Code;
                                NamePersonNameField.Value = NamePersonValues.Before;
                            }
                        }
                    }

                    var NamePersonSupplyField =
                            NamePersonField.Fields.First(n => n.Name == "NamePerson_supplyQuantityTons");
                    NamePersonSupplyField.Value = values[8].Replace(" ", "").Replace(",", ".");

                    var RemarkField = viewModel.FormFields.First(f => f.Name == "Remark");
                    RemarkField.Value = values[11].Trim();

                    var StockQuantityField = viewModel.FormFields.First(f => f.Name == "StockQuantity");
                    StockQuantityField.Value = values[9].Replace(" ", "").Replace(",", ".");

                    var coefficientField = viewModel.FormFields.First(f => f.Name == "coefficient");
                    coefficientField.Value = values[10].Replace(" ", "").Replace(",", ".");

                    int repetitionIndex = 1;
                    //while (dataIndex + 1 < data.Count && data[dataIndex + 1].Values.ToList()[2] == values[2])
                    while (dataIndex + 1 < data.Count && string.IsNullOrWhiteSpace(data[dataIndex + 1].Values.ToList()[2]))
                    {
                        var cloneValues = data[dataIndex + 1].Values.ToList();

                        var clone = 
                            NamePersonField.CreateRepeaterClone(RepeatedFormFieldHelperService.InsertBeforeFirstUnderscore(NamePersonField.Name, "#" + repetitionIndex));
                        clone.Fields = NamePersonField.Fields?.Select(f => f.CreateRepeaterClone(RepeatedFormFieldHelperService.InsertBeforeFirstUnderscore(f.Name, "#" + repetitionIndex))).ToList();

                        var NamePersonNameFieldClone =
                            clone.Fields.First(n => n.Name.EndsWith("_companyNameImmutable"));
                        var namePersonFormEIKFieldClone =
                            clone.Fields.First(f => f.Name.EndsWith("EIKImmutable"));

                        if (!string.IsNullOrWhiteSpace(cloneValues[7].Trim()))
                        {
                            var NamePersonValues = SplitAtLastSpace(cloneValues[7].Trim());
                            if (cloneValues[7].Trim().StartsWith("ДА ДРВВЗ", StringComparison.InvariantCultureIgnoreCase))
                            {
                                NamePersonNameFieldClone.Value = "ДА ДРВВЗ";
                                var namePersonFormBulstatField =
                                    clone.Fields.First(f => f.Name.EndsWith("BulstatImmutable"));
                                namePersonFormBulstatField.Value = "1216";
                            }
                            else
                            {
                                var legalFormNamePersonNom = nomenclatureEIKFromList
                                    .SingleOrDefault(l =>
                                        l.Value.Equals(NamePersonValues.After, StringComparison.OrdinalIgnoreCase));
                                if (legalFormNamePersonNom == null)
                                {
                                    NamePersonNameFieldClone.Value = cloneValues[7].Trim();
                                    errors.Add(
                                        $"Ред {rowNumber + 1}, не може да извлече правна форма от {cloneValues[7].Trim()}, да се коригира ръчно");
                                }
                                else
                                {
                                    namePersonFormEIKFieldClone.Value = legalFormNamePersonNom.Code;
                                    NamePersonNameFieldClone.Value = NamePersonValues.Before;
                                }
                            }
                        }

                        var NamePersonSupplyFieldClone =
                            clone.Fields.First(n => n.Name.EndsWith("_supplyQuantityTons"));
                        NamePersonSupplyFieldClone.Value = cloneValues[8].Replace(" ", "").Replace(",", ".");

                        NamePersonField.Repetitions.Add(clone);

                        RemarkField.Value += Environment.NewLine;
                        RemarkField.Value += cloneValues[11].Trim();

                        rowNumber++;
                        dataIndex++;
                        repetitionIndex++;
                    }

                    bool isViewModelValidationSuccess = await _formValidationService.ValidateViewModel(
                            viewModel,
                            _nomenclatureGrpcClient,
                            await _registerService.GetCurrentRegisterId(),
                            null,
                            true);

                    if (!isViewModelValidationSuccess)
                    {
                        string validationError = string.Join(',',
                            (await _formValidationService.GetValidatedFormFieldsErrors(viewModel)));
                        errors.Add($"ред {rowNumber}, {validationError}");
                        continue;
                    }

                    var serviceStep = registerServiceVM.Steps.Where(x => x.StatusId == (int)ProcessStatus.Registered)
                        .First();
                    var stepVM = await _processService.ToProcessStepVM(Guid.Empty, null, registerServiceVM.Id,
                        serviceStep.Id,
                        serviceStep.OrderNum, null, null, viewModel, false);
                    stepVM.ProcessInfo.PreferredResultDeliveryMethod = ChannelType.OnDesk;
                    (ProcessStepVM addedStep, _) = await _processService.AddStep(
                        stepVM,
                        "831913661");

                    addedRows.Add(rowNumber);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, nameof(ImportExcelFileForEmergencySituationsR0030));
                    errors.Add($"ред {rowNumber}, {e.Message} {e.InnerException?.Message}. Стойности на реда: {string.Join("; ", row.Values)}");
                }

                _processService.ClearTracker();
            }

            string errorMessage = string.Join(Environment.NewLine, errors);
            string addedRowsString = string.Join(", ", addedRows);

            _logger.LogInformation($"Добавени редове в {nameof(ImportExcelFileForEmergencySituationsR0030)} {addedRowsString}");

            return new ContentResult
            {
                Content = $"Добавени {addedRows.Count} записи. Грешки: {Environment.NewLine}{errorMessage}",
                ContentType = "text/plain",
                StatusCode = 200
            };
        }


        /// <summary>
        /// Импорт на данни за заявена услуга от файл [R00030]
        /// </summary>
        /// <param name="file">Pdf файл с json данни на заявена услуга.</param>
        [HttpPost("import-excel-file-for-storage-2")]
        [Display(Name = "Импорт на данни за заявена услуга от файл [R00030]")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportExcelFileForStorage2(IFormFile file, CancellationToken cancellationToken)
        {
            List<string> errors = new List<string>();
            List<int> addedRows = new List<int>();

            List<Dictionary<string, string>> data = await ExcelToDictionary(file);

            ServiceVM registerServiceVM = await _serviceService.GetRegisterService();

            if (registerServiceVM == null)
            {
                return BadRequest("Не е намерена услуга за вписване или формата асоциирана с нея");
            }

            var request = new NomenclaturePublicRequest()
            {
                RegisterId = 0,
                NomenclatureTypes = { "CL0057", "CL0009" },
            };

            NomenclaturePublicResponse response = await _nomenclatureGrpcClient.GetNomenclaturePublicAsync(request);

            if (response.ResultStatus.Code != ResultCodes.Ok)
            {
                errors.Add("Не може да извлече номенклатура");
                return new ContentResult
                {
                    Content = $"Не може да извлече номенклатура CL0057",
                    ContentType = "text/plain",
                    StatusCode = 200
                };
            }

            var nomenclatureFuelList = response.NomenclatureTypes.Last().CodeableConcepts;
            var nomenclatureEIKFromList = response.NomenclatureTypes.First().CodeableConcepts;

            int rowNumber = 1;
            for (int dataIndex = 0; dataIndex < data.Count; dataIndex++)
            {
                rowNumber++;
                var row = data[dataIndex];

                try
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return StatusCode(499, "Request was canceled by the client.");
                    }

                    FormViewModel viewModel =
                        await _formConfigurationPersistenceService.GetFormViewModel(registerServiceVM.FormParentId, true);

                    List<string> values = row.Values.ToList();

                    //todo
                    //if (lastStorageNumber != values[2])
                    //{

                    //}

                    if (string.IsNullOrWhiteSpace(values[0]))
                    {
                        continue;
                    }

                    //string oldIncomingNumber = values[0].Trim(new char[] { ' ', '№' });

                    //ProcessVM alreadyImported = await _processService.GetProcessByOldIncomingNumber(oldIncomingNumber);

                    //if (alreadyImported != null)
                    //{
                    //    continue;
                    //}


                    var RegnumberwarehouseField = viewModel.FormFields.First(f => f.Name == "warehouseNumber");
                    RegnumberwarehouseField.Value = values[0];

                    var EntryDateField = viewModel.FormFields.First(f => f.Name == "EntryDate");
                    EntryDateField.Value = values[1].Replace("г.", "").Trim();

                    var StorekeeperField = viewModel.FormFields.First(f => f.Name == "BasicData");
                    var storekeeperValues = SplitAtLastSpace(values[2].Trim());

                    var storekeeperNameField = StorekeeperField.Fields.First(f => f.Name == "BasicData_companyNameImmutable");
                    storekeeperNameField.Value = storekeeperValues.Before;

                    var storekeeperLegalFormEIKField = StorekeeperField.Fields.First(f => f.Name == "BasicData_legalFormEIKImmutable");
                    var legalFormNom = nomenclatureEIKFromList.SingleOrDefault(l => l.Value.Equals(storekeeperValues.After, StringComparison.OrdinalIgnoreCase));

                    var storekeeperIdField = StorekeeperField.Fields.First(f => f.Name == "BasicData_companyNumberImmutable");
                    storekeeperIdField.Value = "1:" + values[3].Trim();

                    var registrationNumberKeeperField = viewModel.FormFields.First(f => f.Name == "registrationNumberKeeper");
                    registrationNumberKeeperField.Value = values[4].Trim();

                    if (legalFormNom == null)
                    {
                        errors.Add($"Ред {rowNumber}, {storekeeperValues.After} е непозната номенклатурна стойност");
                        continue;
                    }

                    storekeeperLegalFormEIKField.Value = legalFormNom.Code;
                    
                    var warehouseAddressField = viewModel.FormFields.First(f => f.Name == "warehouseAddress");

                    var warehouseAddressStreet = warehouseAddressField.Fields.First(f => f.Name == "warehouseAddress_streetImmutable");
                    var warehouseAddressCountry = warehouseAddressField.Fields.First(f => f.Name == "warehouseAddress_countryImmutable");
                    var warehouseAddressSettlement = warehouseAddressField.Fields.First(f => f.Name == "warehouseAddress_settlementImmutable");
                    var warehouseAddressPostCode = warehouseAddressField.Fields.First(f => f.Name == "warehouseAddress_postalCodeImmutable");

                    warehouseAddressSettlement.Value = values[18].Trim();

                    warehouseAddressStreet.Value = values[7].Trim();
                    warehouseAddressCountry.Value = "BG";
                    warehouseAddressPostCode.Value = values[6].Trim();

                    var storageNumberTechSchemaField = viewModel.FormFields.First(f => f.Name == "storageNumberTechSchema");
                    storageNumberTechSchemaField.Value = values[8].Trim();

                    var RegisteredProjectCapacityField = viewModel.FormFields.First(f => f.Name == "RegisteredProjectCapacity");
                    RegisteredProjectCapacityField.Value = values[9].Replace(",", ".").Replace(" ", "").Replace(" ", "").Trim();

                    var registeredActualCapacityField = viewModel.FormFields.First(f => f.Name == "registeredActualCapacity");
                    registeredActualCapacityField.Value = values[10].Replace(",", ".").Replace(" ", "").Replace(" ", "").Trim();

                    var nonOperationalCapacityField = viewModel.FormFields.First(f => f.Name == "nonOperationalCapacity");
                    nonOperationalCapacityField.Value = values[11].Replace(",", ".").Replace(" ", "").Replace(" ", "").Trim();

                    var supplyTypeField = viewModel.FormFields.First(f => f.Name == "supplyType");

                    var supplyNom = nomenclatureFuelList.SingleOrDefault(l => l.Value.Equals(values[12].Trim(), StringComparison.OrdinalIgnoreCase));

                    if (supplyNom == null)
                    {
                        errors.Add($"Ред {rowNumber}, {values[12].Trim()} е непозната номенклатурна стойност");
                        continue;
                    }

                    supplyTypeField.Value = supplyNom.Code;

                    var ordernumberField = viewModel.FormFields.First(f => f.Name == "ordernumber");
                    ordernumberField.Value = values[13].Trim();

                    var DateofOrderField = viewModel.FormFields.First(f => f.Name == "DateofOrder");
                    DateofOrderField.Value = values[14].Replace("г.", "").Trim();

                    var lisencenumberField = viewModel.FormFields.First(f => f.Name == "lisencenumber");
                    lisencenumberField.Value = values[15].Trim();

                    var lisencedateField = viewModel.FormFields.First(f => f.Name == "lisencedate");
                    lisencedateField.Value = values[16].Replace("г.", "").Trim();

                    var RemarkField = viewModel.FormFields.First(f => f.Name == "Remark");
                    RemarkField.Value = values[17].Trim();

                    bool isViewModelValidationSuccess = await _formValidationService.ValidateViewModel(
                            viewModel,
                            _nomenclatureGrpcClient,
                            await _registerService.GetCurrentRegisterId(),
                            null,
                            true);

                    if (!isViewModelValidationSuccess)
                    {
                        string validationError = string.Join(',',
                            (await _formValidationService.GetValidatedFormFieldsErrors(viewModel)));
                        errors.Add($"ред {rowNumber}, {validationError}");
                        continue;
                    }

                    var serviceStep = registerServiceVM.Steps.Where(x => x.StatusId == (int)ProcessStatus.Registered)
                        .First();
                    var stepVM = await _processService.ToProcessStepVM(Guid.Empty, null, registerServiceVM.Id,
                        serviceStep.Id,
                        serviceStep.OrderNum, null, null, viewModel, false);
                    stepVM.ProcessInfo.PreferredResultDeliveryMethod = ChannelType.OnDesk;
                    (ProcessStepVM addedStep, _) = await _processService.AddStep(
                        stepVM,
                        "831913661");

                    addedRows.Add(rowNumber);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, nameof(ImportExcelFileForEmergencySituationsR0030));
                    errors.Add($"ред {rowNumber}, {e.Message} {e.InnerException?.Message}. Стойности на реда: {string.Join("; ", row.Values)}");
                }

                _processService.ClearTracker();
            }

            string errorMessage = string.Join(Environment.NewLine, errors);
            string addedRowsString = string.Join(", ", addedRows);

            _logger.LogInformation($"Добавени редове в {nameof(ImportExcelFileForEmergencySituationsR0030)} {addedRowsString}");

            return new ContentResult
            {
                Content = $"Добавени {addedRows.Count} записи. Грешки: {Environment.NewLine}{errorMessage}",
                ContentType = "text/plain",
                StatusCode = 200
            };
        }


        public static (string Before, string After) SplitAtLastSpace(string input)
        {
            if (string.IsNullOrEmpty(input))
                return (input, string.Empty);

            int lastSpaceIndex = input.LastIndexOf(' ');

            if (lastSpaceIndex == -1)
                return (input, string.Empty); // No space found

            string before = input.Substring(0, lastSpaceIndex);
            string after = input.Substring(lastSpaceIndex + 1);

            return (before, after);
        }
        private static string RemoveFirstWordAfterQuote(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Find the position of the closing quote „ or "
            int quoteIndex = input.IndexOf('„');
            if (quoteIndex == -1)
                quoteIndex = input.IndexOf('"');

            if (quoteIndex == -1 || quoteIndex >= input.Length - 1)
                return input;

            // Start searching after the closing quote
            int start = quoteIndex + 1;

            // Find the first word (sequence of letters) after the quote
            int wordStart = -1;
            int wordEnd = -1;

            for (int i = start; i < input.Length; i++)
            {
                char c = input[i];
                if (char.IsLetter(c))
                {
                    if (wordStart == -1)
                        wordStart = i; // Start of word
                    wordEnd = i; // Keep updating end
                }
                else if (wordStart != -1)
                {
                    // Word ended (non-letter encountered)
                    break;
                }
            }

            if (wordStart == -1)
                return input; // No word found after quote

            // Determine how much to remove: the word + optional space after it (if not at end)
            int removeStart = wordStart;
            int removeLength = (wordEnd - wordStart + 1); // The word itself

            // Check if there's a space after the word and it's not at the end
            if (wordEnd + 1 < input.Length && input[wordEnd + 1] == ' ')
            {
                removeLength++; // Include the space
            }

            // Remove the word (and space if applicable)
            return input.Remove(removeStart, removeLength);
        }

        ///// <summary>
        ///// Импорт на данни за заявена услуга от файл възстановяване на неправомерна и несъвместима държавна помощ
        ///// </summary>
        ///// <param name="file">Pdf файл с json данни на заявена услуга.</param>
        //[HttpPost("import-excel-file-for-unlawful-aid")]
        //[Display(Name = "Импорт на данни за заявена услуга от файл възстановяване")]
        //[Consumes("multipart/form-data")]
        //public async Task<IActionResult> ImportExcelFileForUnlawfulAid(IFormFile file, CancellationToken cancellationToken)
        //{
        //    List<string> errors = new List<string>();
        //    List<int> addedRows = new List<int>();

        //    List<Dictionary<string, string>> data = await ExcelToDictionary(file);

        //    ServiceVM registerServiceVM = await _serviceService.GetRegisterService();

        //    if (registerServiceVM == null)
        //    {
        //        return BadRequest("Не е намерена услуга за вписване или формата асоциирана с нея");
        //    }

        //    int rowNumber = 1;
        //    foreach (Dictionary<string, string> row in data)
        //    {
        //        rowNumber++;

        //        try
        //        {
        //            if (cancellationToken.IsCancellationRequested)
        //            {
        //                return StatusCode(499, "Request was canceled by the client.");
        //            }

        //            FormViewModel viewModel =
        //                await _formConfigurationPersistenceService.GetFormViewModel(registerServiceVM.FormParentId, true);

        //            List<string> values = row.Values.ToList();

        //            if (string.IsNullOrWhiteSpace(values[0]))
        //            {
        //                continue;
        //            }

        //            //string oldIncomingNumber = values[0].Trim(new char[] { ' ', '№' });

        //            //ProcessVM alreadyImported = await _processService.GetProcessByOldIncomingNumber(oldIncomingNumber);

        //            //if (alreadyImported != null)
        //            //{
        //            //    continue;
        //            //}

        //            var companyOrPerson = viewModel.FormFields.First(f => f.Name == "receiversName");

        //            string identifier = values[0].Trim();

        //            if(ValidationUtils.IsEGN(identifier))
        //            {
        //                var response = await _integrationGrpcClient.GetPersonInfoAsync(new GetPersonInfoRequest()
        //                {
        //                    Pid = identifier,
        //                    ContextInfo = GetRegixContextInfo()
        //                });
        //                //GetCompanyInfoResponse response = await _integrationGrpcClient.GetCompanyInfoAsync(request);

        //                var pidField =
        //                        companyOrPerson.Fields.First(f => f.Name == "receiversName_identifierImmutable");


        //                pidField.Value = "1:" + identifier;

        //                var firstName =
        //                    companyOrPerson.Fields.First(f => f.Name == "receiversName_firstNameImmutable");

        //                firstName.Value = response.FirstName;


        //                var secondName =
        //                    companyOrPerson.Fields.First(f => f.Name == "receiversName_middleNameImmutable");

        //                secondName.Value = response.MiddleName; 
                        
        //                var lastName =
        //                    companyOrPerson.Fields.First(f => f.Name == "receiversName_lastNameImmutable");

        //                lastName.Value = response.LastName;
        //            }
        //            else
        //            {
        //                bool companyImported = await ImportCompany(identifier, companyOrPerson, errors, rowNumber);

        //                if (!companyImported)
        //                {
        //                    continue;
        //                }
        //            }

        //            var eudecisionField = viewModel.FormFields.First(f => f.Name == "EUdecision");
        //            var decisiconDateField = viewModel.FormFields.First(f => f.Name == "decisiconDate");

        //            string columnDecisionMatch = @"^(C\(\d{4}\)\d+).*?от\s*(\d{2}\.\d{2}\.\d{4})";

        //            var decisionMatch = Regex.Match(values[2].Trim(),
        //                columnDecisionMatch);

        //            if (decisionMatch.Success)
        //            {
        //                eudecisionField.Value = decisionMatch.Groups[1].Value;   // "C(2014)6207"
        //                decisiconDateField.Value = decisionMatch.Groups[2].Value;   // "05.09.2014"
        //            }
        //            else
        //            {
        //                errors.Add($"Row {rowNumber} can't parse {values[2]}");
        //                continue;
        //            }

        //            var administrationField = viewModel.FormFields.First(f => f.Name == "Administration");
        //            administrationField.Value = values[3].Trim().Replace("-", String.Empty);

        //            string returnedSum = values[4].Trim().Replace("-", String.Empty).Replace(",", ".").Replace(";", String.Empty);

        //            var principalForRefundField = viewModel.FormFields.First(f => f.Name == "principalForRefund");
        //            var interestForRefundField = viewModel.FormFields.First(f => f.Name == "interestForRefund");

        //            principalForRefundField.Value = String.Empty;
        //            interestForRefundField.Value = String.Empty;

        //            string pattern =
        //                @"г[\p{L}]+?а[:\s]?\s*(?<principal>[\d\s]+[.,]?\d*)" +
        //                @"(?:\s*(?<interestPart>л[\p{L}]*?а|лихви)[:\s]?\s*(?<interest>[\d\s]+[.,]?\d*))?";

        //            Match match = Regex.Match(returnedSum, pattern, RegexOptions.IgnoreCase);

        //            if (match.Success)
        //            {
        //                string principalStr = match.Groups["principal"].Value;

        //                // Clean and parse principal (remove spaces)
        //                string principalClean = Regex.Replace(principalStr, @"\s+", "").Replace(",", ".");
        //                double principal = double.Parse(principalClean, NumberStyles.Any, CultureInfo.InvariantCulture);

        //                double interest = 0;
        //                if (match.Groups["interest"].Success && match.Groups["interestPart"].Success)
        //                {
        //                    string interestStr = match.Groups["interest"].Value;
        //                    string interestClean = Regex.Replace(interestStr, @"\s+", "").Replace(",", ".");
        //                    interest = double.Parse(interestClean, NumberStyles.Any, CultureInfo.InvariantCulture);
        //                }
        //                // If no лихва part → interest remains 0

        //                principalForRefundField.Value = "1:" + principal.ToString("F2", CultureInfo.InvariantCulture);
        //                interestForRefundField.Value = "1:" + interest.ToString("F2", CultureInfo.InvariantCulture);
        //            }
        //            else
        //            {
        //                errors.Add($"На ред {rowNumber} {returnedSum} е в неразпознавзем формат");
        //            }

        //            var deadlineField = viewModel.FormFields.First(f => f.Name == "Deadline");
        //            deadlineField.Value = ReadExcelDate(values[5]);


        //            var dateOfactField = viewModel.FormFields.First(f => f.Name == "dateOfact");
        //            dateOfactField.Value = ReadExcelDate(values[6]);

        //            string refundedSum = values[7].Trim().Replace("-", String.Empty).Replace(",", ".").Replace(";", String.Empty).Replace(" ", String.Empty); ;

        //            var principalRefundedField = viewModel.FormFields.First(f => f.Name == "principalRefunded");
        //            var interestRefundedField = viewModel.FormFields.First(f => f.Name == "interestRefunded");
        //            var taxesField = viewModel.FormFields.First(f => f.Name == "taxes");

        //            principalRefundedField.Value = String.Empty;
        //            interestRefundedField.Value = String.Empty; ;
        //            taxesField.Value = String.Empty; ;

        //            if (!string.IsNullOrWhiteSpace(refundedSum))
        //            {
        //                principalRefundedField.Value = "1:" + refundedSum;
        //            }

        //            string statut = values[8].Trim();
        //            var entryReasonField = viewModel.FormFields.First(f => f.Name == "statut");

        //            if (statut == "възстановява се")
        //            {
        //                entryReasonField.Value = "1";
        //            }
        //            else if (statut == "възстановена")
        //            {
        //                entryReasonField.Value = "2";
        //            }
        //            else if (statut is "невъзстановена")
        //            {
        //                entryReasonField.Value = "3";
        //            }
        //            else
        //            {
        //                errors.Add($"Непозната Номенклатурни стойности към Статут решения на ЕК {statut} ред {rowNumber}");
        //            }

        //            string remarks = values[9].Trim();
        //            var remarksField = viewModel.FormFields.First(f => f.Name == "Remark");
                    
        //            //remarksField.Value = remarks.ReplaceLineEndings(String.Empty);
        //            remarksField.Value = remarks;

        //            string remarksConfidential = values[10].Trim();
        //            var ConfidentialNoteField = viewModel.FormFields.FirstOrDefault(f => f.Name == "ConfidentialNote");

        //            if (ConfidentialNoteField != null)
        //            {
        //                //ConfidentialNoteField.Value = remarksConfidential.ReplaceLineEndings(String.Empty);
        //                ConfidentialNoteField.Value = remarksConfidential;
        //            }

        //            bool isViewModelValidationSuccess = await _formValidationService.ValidateViewModel(
        //                    viewModel,
        //                    _nomenclatureGrpcClient,
        //                    await _registerService.GetCurrentRegisterId(),
        //                    null,
        //                    true);

        //            if (!isViewModelValidationSuccess)
        //            {
        //                string validationError = string.Join(',',
        //                    (await _formValidationService.GetValidatedFormFieldsErrors(viewModel)));
        //                errors.Add($"ред {rowNumber}, {validationError}");
        //                continue;
        //            }

        //            var serviceStep = registerServiceVM.Steps.Where(x => x.StatusId == (int)ProcessStatus.Registered)
        //                .First();
        //            var stepVM = await _processService.ToProcessStepVM(Guid.Empty, null, registerServiceVM.Id,
        //                serviceStep.Id,
        //                serviceStep.OrderNum, null, null, viewModel, false);
        //            stepVM.ProcessInfo.PreferredResultDeliveryMethod = ChannelType.OnDesk;
        //            (ProcessStepVM addedStep, _) = await _processService.AddStep(
        //                stepVM,
        //                "000695406");

        //            addedRows.Add(rowNumber);
        //        }
        //        catch (Exception e)
        //        {
        //            _logger.LogError(e, nameof(ImportExcelFileForZZNNR0036));
        //            errors.Add($"ред {rowNumber}, {e.Message} {e.InnerException?.Message}. Стойности на реда: {string.Join("; ", row.Values)}");
        //        }

        //        _processService.ClearTracker();
        //    }

        //    string errorMessage = string.Join(Environment.NewLine, errors);
        //    string addedRowsString = string.Join(", ", addedRows);

        //    _logger.LogInformation($"Добавени редове в {nameof(ImportExcelFileForZZNNR0036)} {addedRowsString}");

        //    return new ContentResult
        //    {
        //        Content = $"Добавени {addedRows.Count} записи. Грешки: {Environment.NewLine}{errorMessage}",
        //        ContentType = "text/plain",
        //        StatusCode = 200
        //    };
        //}
    }
}
