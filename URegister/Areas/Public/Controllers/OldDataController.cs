using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
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
        [HttpPost("import-excel-file-for-r00001")]
        [Display(Name = "Импорт на данни за заявена услуга от файл [R00001]")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportExcelFileForR00001(IFormFile file, CancellationToken cancellationToken)
        {
            List<string> errors = new List<string>();
            List<int> addedRows = new List<int>();

            List<Dictionary<string, string>> data = await ExcelToDictionary(file);

            ServiceVM registerServiceVM = await _serviceService.GetRegisterService();

            if (registerServiceVM == null)
            {
                return BadRequest("Не е намерена услуга за вписване или формата асоциирана с нея");
            }

            int rowNumber = 1;
            foreach (Dictionary<string, string> row in data)
            {
                rowNumber++;

                try
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return StatusCode(499, "Request was canceled by the client.");
                    }

                    FormViewModel viewModel =
                        await _formConfigurationPersistenceService.GetFormViewModel(registerServiceVM.FormParentId, true);

                    List<string> values = row.Values.ToList();

                    if (string.IsNullOrWhiteSpace(values[0]))
                    {
                        continue;
                    }

                    string oldIncomingNumber = values[0].Trim(new char[] { ' ', '№' });

                    ProcessVM alreadyImported = await _processService.GetProcessByOldIncomingNumber(oldIncomingNumber);

                    if (alreadyImported != null)
                    {
                        continue;
                    }

                    var company = viewModel.FormFields.First(f => f.Name == "Designation");

                    bool companyImported = await ImportCompany(values, company, errors, rowNumber);

                    if (!companyImported)
                    {
                        continue;
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
                        serviceStep.OrderNum, oldIncomingNumber, null, viewModel, false);
                    stepVM.ProcessInfo.PreferredResultDeliveryMethod = ChannelType.OnDesk;
                    (ProcessStepVM addedStep, _) = await _processService.AddStep(
                        stepVM,
                        "000695160");

                    addedRows.Add(rowNumber);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, nameof(ImportExcelFileForR00001));
                    errors.Add($"ред {rowNumber}, {e.Message} {e.InnerException?.Message}");
                }

                _processService.ClearTracker();
            }

            string errorMessage = string.Join(Environment.NewLine, errors);
            string addedRowsString = string.Join(", ", addedRows);

            _logger.LogInformation($"Добавени редове в {nameof(ImportExcelFileForR00001)} {addedRowsString}");

            return new ContentResult
            {
                Content = $"Добавени {addedRows.Count} записи. Грешки: {Environment.NewLine}{errorMessage}",
                ContentType = "text/plain",
                StatusCode = 200
            };
        }

        private async Task<bool> ImportCompany(List<string> values, FormField company, List<string> errors, int rowNumber)
        {
            string sanitizedCid = Regex.Replace(values[2], "[^0-9]", "");
            SaveOperationResult companyInfoResult = await GetCompanyData(sanitizedCid);

            if (companyInfoResult.IsSuccess)
            {
                GetCompanyInfoResponse companyInfo = companyInfoResult.CustomObject as GetCompanyInfoResponse;

                var companyNumberField = company.Fields.FirstOrDefault(f => f.Name == company.Name + "_companyNumberImmutable");
                if (companyNumberField != null)
                {
                    companyNumberField.Value = (int)companyInfoResult.AddedObjectId + ":" + sanitizedCid;
                }

                if ((int)companyInfoResult.AddedObjectId == (int)CidTypes.BULSTAT)
                {
                    var legalFormBulstatField = company.Fields.FirstOrDefault(f => f.Name == company.Name + "_legalFormBulstatImmutable");
                    if (legalFormBulstatField != null)
                    {
                        legalFormBulstatField.Value = companyInfo.LegalFormCode.ToString();
                    }
                }
                else if ((int)companyInfoResult.AddedObjectId == (int)CidTypes.EIK)
                {
                    var legalFormEIKField = company.Fields.FirstOrDefault(f => f.Name == company.Name + "_legalFormEIKImmutable");
                    if (legalFormEIKField != null)
                    {
                        legalFormEIKField.Value = companyInfo.LegalFormCode.ToString();
                    }
                }

                var companyNameField = company.Fields.FirstOrDefault(f => f.Name == company.Name + "_companyNameImmutable");
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
                errors.Add($"ред {rowNumber}, БУЛСТАТ/ЕИК {values[2]}");
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

            var contextInfo = GetRegexContextInfo();

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

            return new SaveOperationResult(true, (int)cidType)
            {
                CustomObject = response
            };
        }

        private IntegrationServiceContextInfo GetRegexContextInfo()
        {
            return new IntegrationServiceContextInfo()
            {
                EmployeeAdministration = "Batch import",
                    //UserContext.AvailableAdministrations.FirstOrDefault(a => UserContext.AdministrationId.ToString() == a.Id)?.Name,
                EmployeeNames = "Batch import",//; UserContext.FirstName + " " + UserContext.LastName,
                EmployeePosition = "Batch import"//string.Join(", ", roles)
            };
        }

        private async Task<List<Dictionary<string, string>>> ExcelToDictionary(IFormFile file)
        {
            var data = new List<Dictionary<string, string>>();

            try
            {
                var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                
                using (memoryStream)
                {
                    using (var package = new ExcelPackage(memoryStream))
                    {
                        var sheet = package.Workbook.Worksheets[0];
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
                _logger.LogError(e, $"{nameof(ImportExcelFileForR00001)}");
            }

            return data;
        }
    }
}
