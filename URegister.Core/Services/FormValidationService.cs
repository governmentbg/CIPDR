using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.RegularExpressions;
using URegister.Core.Services;
using Google.Protobuf.WellKnownTypes;
using URegister.Common;
using URegister.Core.Contracts;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Extensions;
using URegister.Infrastructure.Model.RegisterForms;
using URegister.NomenclaturesCatalog;
using Enum = System.Enum;

namespace URegister.Core.Services
{
    /// <summary>
    /// Сервиз за back end валидация на данните въведени във форма
    /// </summary>
    public class FormValidationService : IFormValidationService
    {
        private readonly ILogger<FormFieldsLayoutService> _logger;

        private static readonly Dictionary<string, List<string>> fileExtensionsHexSignatures = new Dictionary<string, List<string>>
            {
                { ".xml", new List<string> { "3C 3F 78 6D 6C 20" } },
                { ".pdf", new List<string> { "25 50 44 46 2D" } },
                { ".doc", new List<string> { "D0 CF 11 E0 A1 B1 1A E1" } },
                { ".sxw", new List<string> { "50 4B 03 04", "50 4B 05 06", "50 4B 07 08" } },
                { ".txt", new List<string> { "EF BB BF", "FF FE", "FE FF", "FF FE 00 00", "00 00 FE FF", "" } },
                { ".rtf", new List<string> { "7B 5C 72 74 66 31" } },
                { ".jpg", new List<string> { "FF D8 FF DB", "FF D8 FF E0", "FF D8 FF EE", "FF D8 FF E1" } },
                { ".jpeg", new List<string> { "FF D8 FF DB", "FF D8 FF E0", "FF D8 FF EE", "FF D8 FF E1" } },
                { ".j2k", new List<string> { "00 00 00 0C 6A 50 20 20 0D 0A 87 0A", "FF 4F FF 51" } },
                { ".jpx", new List<string> { "00 00 00 0C 6A 50 20 20 0D 0A 87 0A", "FF 4F FF 51" } },
                { ".jp2", new List<string> { "00 00 00 0C 6A 50 20 20 0D 0A 87 0A", "FF 4F FF 51" } },
                { ".png", new List<string> { "89 50 4E 47 0D 0A 1A 0A" } },
                { ".gif", new List<string> { "47 49 46 38 37 61", "47 49 46 38 39 61" } },
                { ".tiff", new List<string> { "49 49 2A 00", "4D 4D 00 2A" } },
                { ".p7s", new List<string> { "30 82" } }
            };

        public FormValidationService(
            ILogger<FormFieldsLayoutService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Валидира стойностите на полетата във формата
        /// </summary>
        /// <param name="viewModel">Моделът за валидация</param>
        /// <param name="nomenclatureGrpcClient">GRPC клиент за номенклатура</param>
        /// <param name="registerId">Идентификатор на регистъра</param>
        /// <param name="processRegistrationDateUtc">Дата на създаване на заяявената услуга</param>
        /// <param name="skipRequiredTest">Да се пропусне ли проверка за задължителни полета</param>
        /// <returns>Всички стойности ли са валидни</returns>
        public async Task<bool> ValidateViewModel(FormViewModel viewModel,
            NomenclatureGrpc.NomenclatureGrpcClient nomenclatureGrpcClient,
            int registerId, DateTime? processRegistrationDateUtc = null, 
            bool skipRequiredTest = false)
        {
            return await ValidateViewModelFields(
                viewModel.FormFields, 
                nomenclatureGrpcClient,
                processRegistrationDateUtc,
                registerId,
                viewModel.UserTimeZoneOffsetInMinutes,
                true,
                skipRequiredTest);
        }

        private async Task<bool> ValidateViewModelFields(IEnumerable<FormField> formFieldsForValidation,
            NomenclatureGrpc.NomenclatureGrpcClient nomenclatureGrpcClient,
            DateTime? processRegistrationDateUtc,
            int registerId,
            int userTimeZoneOffsetInMinutes,
            bool validSoFar = true,
            bool skipRequiredTest = false)
        {
            foreach (FormField field in formFieldsForValidation)
            {
                if (field.Repetitions != null && field.Repetitions.Any())
                {
                    validSoFar = await ValidateViewModelFields(field.Repetitions, 
                        nomenclatureGrpcClient, 
                        processRegistrationDateUtc,
                        registerId, 
                        userTimeZoneOffsetInMinutes, 
                        validSoFar, 
                        skipRequiredTest) && validSoFar;
                }

                if (
                    (string.IsNullOrWhiteSpace(field.Value) || 
                     ((field.Type == SimpleFormFieldType.PersonIdentifier.ToString() || field.Type == SimpleFormFieldType.CompanyIdentifier.ToString())
                      && field.Value.Trim().Split(':').Any(p => string.IsNullOrWhiteSpace(p)))
                     )//идентификатор
                    && (field.Fields == null || field.Fields.IsEmpty()))//Не е сложен тип
                {
                    if (field.Type == SimpleFormFieldType.Boolean.ToString() && !skipRequiredTest)
                    {
                        field.ValidationError = MessageConstant.InvalidValue;
                        validSoFar = false;
                        continue;
                    }
                    if (field.Type == SimpleFormFieldType.PersonIdentifier.ToString() ||
                        field.Type == SimpleFormFieldType.CompanyIdentifier.ToString())
                    {
                        field.Value = string.Empty;
                    }
                    if (field.IsRequired && !skipRequiredTest)
                    {
                        field.ValidationError = MessageConstant.FieldIsRequiredNoParam;
                        validSoFar = false;
                    }
                    continue;
                }

                if (string.IsNullOrWhiteSpace(field.Value) && field.Type != SimpleFormFieldType.File.ToString())
                {
                    //Всички сложни типове
                    if (field.Fields != null && field.Fields.Any())
                    {
                        validSoFar = await ValidateViewModelFields(field.Fields, 
                            nomenclatureGrpcClient,
                            processRegistrationDateUtc,
                            registerId, 
                            userTimeZoneOffsetInMinutes, 
                            validSoFar, 
                            !field.IsRequired || skipRequiredTest) && validSoFar;
                    }

                    if (field.Type == SimpleFormFieldType.IndividualIdentifier.ToString())
                    {
                        validSoFar = ValidateIndividualIdentifier(field) && validSoFar;
                    }
                    else if (field.Type == SimpleFormFieldType.Address.ToString())
                    {
                        validSoFar = await ValidateAddress(field, nomenclatureGrpcClient, registerId) && validSoFar;
                    }
                    else if (field.Type == SimpleFormFieldType.Company.ToString())
                    {
                        validSoFar = await ValidateCompany(field, skipRequiredTest) && validSoFar;
                    }

                    continue;
                }

                if (Enum.TryParse(field.Type, out SimpleFormFieldType fieldType))
                {
                    switch (fieldType)
                    {
                        case SimpleFormFieldType.Number:
                            validSoFar = ValidateNumber(field) && validSoFar;
                            break;
                        case SimpleFormFieldType.Text:
                        case SimpleFormFieldType.TextArea:
                        case SimpleFormFieldType.Email:
                        case SimpleFormFieldType.Phone:
                        case SimpleFormFieldType.Url:
                            validSoFar = ValidateText(field) && validSoFar;
                            break;
                        case SimpleFormFieldType.File:
                            validSoFar = ValidateFileField(field) && validSoFar;
                            break;
                        case SimpleFormFieldType.Date:
                        case SimpleFormFieldType.DateTime:
                            validSoFar = await ValidateDate(field, userTimeZoneOffsetInMinutes, processRegistrationDateUtc) && validSoFar;
                            break;
                        case SimpleFormFieldType.Boolean:
                            validSoFar = await ValidateBoolean(field) && validSoFar;
                            break;
                        case SimpleFormFieldType.Select:
                        case SimpleFormFieldType.Autocomplete:
                        case SimpleFormFieldType.MultiSelect:
                        case SimpleFormFieldType.AutocompleteWithCategory:
                            validSoFar = await ValidateSelect(field, nomenclatureGrpcClient, registerId) && validSoFar;
                            break;
                        case SimpleFormFieldType.City:
                            validSoFar = await ValidateCity(field, nomenclatureGrpcClient) && validSoFar;
                            break;
                        case SimpleFormFieldType.PersonIdentifier:
                            validSoFar = await ValidatePid(field, nomenclatureGrpcClient, registerId) && validSoFar;
                            break;
                        case SimpleFormFieldType.CompanyIdentifier:
                            validSoFar = await ValidateCid(field, nomenclatureGrpcClient, registerId) && validSoFar;
                            break;
                        case SimpleFormFieldType.Time:
                            validSoFar = await ValidateTime(field) && validSoFar;
                            break;
                    }
                }
                else
                {
                    _logger.LogError($"Непознат тип на просто поле {field.Type} в {nameof(ValidateViewModelFields)}");
                }
            }

            return validSoFar;
        }

        private async Task<bool> ValidateCompany(FormField field, bool skipRequiredTest = false)
        {
            if (!field.IsRequired || skipRequiredTest)
            {
                return true;
            }

            var legalFormEIKField =
            field.Fields!.SingleOrDefault(f =>
                f.Name.Contains("legalFormEIKImmutable", StringComparison.InvariantCultureIgnoreCase));
            
            var legalFormBulstatField =
                field.Fields!.SingleOrDefault(f =>
                    f.Name.Contains("legalFormBulstatImmutable", StringComparison.InvariantCultureIgnoreCase));

            //TODO : да се проверява за празно само полето спрямо избрания идентификатор

            if (string.IsNullOrWhiteSpace(legalFormEIKField.Value) &&
                string.IsNullOrWhiteSpace(legalFormBulstatField.Value))
            {
                field.ValidationError =
                legalFormEIKField.ValidationError = 
                    legalFormBulstatField.ValidationError = 
                        MessageConstant.FieldIsRequiredNoParam;
                return false;
            }

            return true;
        }

        private bool ValidateIndividualIdentifier(FormField field)
        {
            var birthCountrySubfield =
                field.Fields!.SingleOrDefault(f =>
                    f.Name.Contains("birthCountryImmutable", StringComparison.InvariantCultureIgnoreCase));

            if (birthCountrySubfield == null)
            {
                field.ValidationError = MessageConstant.InvalidIDTemplate;
                return false;
            }

            if (birthCountrySubfield.Value == "BG")
            {
                var birthPlaceBg =
                    field.Fields!.SingleOrDefault(f =>
                        f.Name.Contains("birthPlaceBgImmutable", StringComparison.InvariantCultureIgnoreCase));

                if (birthPlaceBg == null)
                {
                    field.ValidationError = MessageConstant.InvalidIDTemplate;
                    return false;
                }

                if (string.IsNullOrWhiteSpace(birthPlaceBg.Value))
                {
                    birthPlaceBg.ValidationError = MessageConstant.EnterPlaceOfBirth;
                    return false;
                }
            }
            else
            {
                var birthPlaceAbroad =
                    field.Fields!.SingleOrDefault(f =>
                        f.Name.Contains("birthPlaceAbroadImmutable", StringComparison.InvariantCultureIgnoreCase));

                if (birthPlaceAbroad == null)
                {
                    field.ValidationError = MessageConstant.InvalidIDTemplate;
                    return false;
                }

                if (string.IsNullOrWhiteSpace(birthPlaceAbroad.Value))
                {
                    birthPlaceAbroad.ValidationError = MessageConstant.EnterPlaceOfBirth;
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> ValidatePid(FormField field,
            NomenclatureGrpc.NomenclatureGrpcClient nomenclatureGrpcClient, int registerId)
        {
            var valueComponents = field.Value.Split(':', StringSplitOptions.TrimEntries);

            if (valueComponents.Length != 2)
            {
                field.ValidationError = MessageConstant.InvalidValueFormat;
                return false;
            }

            if (!int.TryParse(valueComponents[0], out int parsedPidType))
            {
                field.ValidationError = MessageConstant.InvalidIdentifierType;
                return false;
            }

            AreNomenclatureCodesAllowedRequest areNomenclatureCodesAllowedRequest = new AreNomenclatureCodesAllowedRequest
            {
                RegisterId = registerId,
                NomenclatureType = NomenclatureTypes.PidType,
                NomenclatureCodes = { valueComponents[0] }
            };

            AreNomenclatureCodesAllowedResponse areNomenclatureCodesAllowedResponse =
                await nomenclatureGrpcClient.AreNomenclatureCodesAllowedAsync(areNomenclatureCodesAllowedRequest);

            if (areNomenclatureCodesAllowedResponse.ResultStatus.Code != ResultCodes.Ok)
            {
                _logger.LogError($"GetNomenclaturePublicAsync неуспешен в {nameof(ValidatePid)}");
                field.ValidationError = MessageConstant.ValidationFailConnectionIssue;
                return false;
            }

            if (!areNomenclatureCodesAllowedResponse.AreAllowed)
            {
                field.ValidationError = MessageConstant.UnknownIdentifierType;
                return false;
            }

            if (!PidValidateService.ValidatePersonalId(valueComponents[1], parsedPidType))
            {
                field.ValidationError = MessageConstant.InvalidIdentifier;
                return false;
            }

            return true;
        }

        private async Task<bool> ValidateCid(FormField field,
            NomenclatureGrpc.NomenclatureGrpcClient nomenclatureGrpcClient, int registerId)
        {
            var valueComponents = field.Value.Split(':', StringSplitOptions.TrimEntries);

            if (valueComponents.Length != 2)
            {
                field.ValidationError = MessageConstant.InvalidValueFormat;
                return false;
            }

            if (!int.TryParse(valueComponents[0], out int parsedCidType))
            {
                field.ValidationError = MessageConstant.InvalidIdentifierType;
                return false;
            }

            AreNomenclatureCodesAllowedRequest areNomenclatureCodesAllowedRequest = new AreNomenclatureCodesAllowedRequest
            {
                RegisterId = registerId,
                NomenclatureType = NomenclatureTypes.CidType,
                NomenclatureCodes = { valueComponents[0] }
            };

            AreNomenclatureCodesAllowedResponse areNomenclatureCodesAllowedResponse =
                await nomenclatureGrpcClient.AreNomenclatureCodesAllowedAsync(areNomenclatureCodesAllowedRequest);

            if (areNomenclatureCodesAllowedResponse.ResultStatus.Code != ResultCodes.Ok)
            {
                _logger.LogError($"GetNomenclaturePublicAsync неуспешен в {nameof(ValidateCid)}");
                field.ValidationError = MessageConstant.ValidationFailConnectionIssue;
                return false;
            }

            if (!areNomenclatureCodesAllowedResponse.AreAllowed)
            {
                field.ValidationError = MessageConstant.UnknownIdentifierType;
                return false;
            }

            if (!PidValidateService.ValidateCompanyId(valueComponents[1], parsedCidType))
            {
                field.ValidationError = MessageConstant.InvalidIdentifier;
                return false;
            }

            return true;
        }

        private async Task<bool> ValidateAddress(FormField field,
            NomenclatureGrpc.NomenclatureGrpcClient nomenclatureGrpcClient, int registerId)
        {
            if (field.IsRequired)
            {
                var countrySubfield =
                    field.Fields!.SingleOrDefault(f =>
                        f.Name.Contains("countryImmutable", StringComparison.InvariantCultureIgnoreCase));

                if (countrySubfield == null)
                {
                    field.ValidationError = MessageConstant.InvalidFieldConfig;
                    return false;
                }

                if (countrySubfield.Value == "BG")
                {
                    var bgSettlementField = field.Fields!.SingleOrDefault(f =>
                        f.Name.Contains("settlementImmutable", StringComparison.InvariantCultureIgnoreCase));

                    if (bgSettlementField == null)
                    {
                        field.ValidationError = MessageConstant.InvalidFieldConfig;
                        return false;
                    }

                    if (string.IsNullOrWhiteSpace(bgSettlementField.Value))
                    {
                        bgSettlementField.ValidationError = MessageConstant.FieldIsRequired;
                        return false;
                    }
                }
                else
                {
                    var foreignAddress = field.Fields!.SingleOrDefault(f =>
                        f.Name.Contains("addressAbroadImmutable", StringComparison.InvariantCultureIgnoreCase));

                    if (foreignAddress == null)
                    {
                        field.ValidationError = MessageConstant.InvalidFieldConfig;
                        return false;
                    }

                    if (string.IsNullOrWhiteSpace(foreignAddress.Value))
                    {
                        foreignAddress.ValidationError = MessageConstant.FieldIsRequired;
                        return false;
                    }
                }
            }


            var settlementImmutable = field.Fields.SingleOrDefault(f => f.Name.Contains("settlementImmutable", StringComparison.InvariantCultureIgnoreCase));

            //var streetField = field.Fields.SingleOrDefault(f => f.Name.Contains("streetImmutable", StringComparison.InvariantCultureIgnoreCase));

            //if (streetField != null && !string.IsNullOrWhiteSpace(streetField.Value))
            //{
            //    AreNomenclatureCodesAllowedRequest areNomenclatureCodesAllowedRequest = new AreNomenclatureCodesAllowedRequest
            //    {
            //        RegisterId = registerId,
            //        NomenclatureType = NomenclatureTypes.EkStreet,
            //        NomenclatureCodes = { streetField.Value },
            //        Holder = settlementImmutable!.Value
            //    };

            //    AreNomenclatureCodesAllowedResponse areNomenclatureCodesAllowedResponse =
            //        await nomenclatureGrpcClient.AreNomenclatureCodesAllowedAsync(areNomenclatureCodesAllowedRequest);

            //    if (areNomenclatureCodesAllowedResponse.ResultStatus.Code != ResultCodes.Ok)
            //    {
            //        _logger.LogError($"GetNomenclaturePublicAsync неуспешен в {nameof(ValidatePid)}");
            //        streetField.ValidationError = MessageConstant.ValidationFailConnectionIssue;
            //        return false;
            //    }

            //    if (!areNomenclatureCodesAllowedResponse.AreAllowed)
            //    {
            //        streetField.ValidationError = MessageConstant.UnknownStreetForSettlement;
            //        return false;
            //    }
            //}

            var regionField = field.Fields.SingleOrDefault(f => f.Name.Contains("regionImmutable", StringComparison.InvariantCultureIgnoreCase));

            if (regionField != null && !string.IsNullOrWhiteSpace(regionField.Value))
            {
                AreNomenclatureCodesAllowedRequest areNomenclatureCodesAllowedRequest = new AreNomenclatureCodesAllowedRequest
                {
                    RegisterId = registerId,
                    NomenclatureType = NomenclatureTypes.EkRaion,
                    NomenclatureCodes = { regionField.Value },
                    Holder = settlementImmutable!.Value
                };

                AreNomenclatureCodesAllowedResponse areNomenclatureCodesAllowedResponse =
                    await nomenclatureGrpcClient.AreNomenclatureCodesAllowedAsync(areNomenclatureCodesAllowedRequest);

                if (areNomenclatureCodesAllowedResponse.ResultStatus.Code != ResultCodes.Ok)
                {
                    _logger.LogError($"GetNomenclaturePublicAsync неуспешен в {nameof(ValidatePid)}");
                    regionField.ValidationError = MessageConstant.ValidationFailConnectionIssue;
                    return false;
                }

                if (!areNomenclatureCodesAllowedResponse.AreAllowed)
                {
                    regionField.ValidationError = MessageConstant.UnknownDistrictForSettlement;
                    return false;
                }
            }

            //var districtField = field.Fields.SingleOrDefault(f => f.Name.Contains("districtImmutable", StringComparison.InvariantCultureIgnoreCase));

            //if (districtField != null && !string.IsNullOrWhiteSpace(districtField.Value))
            //{
            //    AreNomenclatureCodesAllowedRequest areNomenclatureCodesAllowedRequest = new AreNomenclatureCodesAllowedRequest
            //    {
            //        RegisterId = registerId,
            //        NomenclatureType = NomenclatureTypes.EkKvartal,
            //        NomenclatureCodes = { districtField.Value },
            //        Holder = settlementImmutable!.Value
            //    };

            //    AreNomenclatureCodesAllowedResponse areNomenclatureCodesAllowedResponse =
            //        await nomenclatureGrpcClient.AreNomenclatureCodesAllowedAsync(areNomenclatureCodesAllowedRequest);

            //    if (areNomenclatureCodesAllowedResponse.ResultStatus.Code != ResultCodes.Ok)
            //    {
            //        _logger.LogError($"GetNomenclaturePublicAsync неуспешен в {nameof(ValidatePid)}");
            //        districtField.ValidationError = MessageConstant.ValidationFailConnectionIssue;
            //        return false;
            //    }

            //    if (!areNomenclatureCodesAllowedResponse.AreAllowed)
            //    {
            //        districtField.ValidationError = MessageConstant.UnknownNeighborhoodForSettlement;
            //        return false;
            //    }
            //}

            return true;
        }

        /// <summary>
        /// Валидира качен файл
        /// </summary>
        /// <param name="field">Полето за валидация</param>
        /// <param name="file">Файл за валидация</param>
        /// <returns></returns>
        public async Task<bool> ValidateFile(FormField field, IFormFile file)
        {
            if (file == null && field.IsRequired)
            {
                field.ValidationError = MessageConstant.FieldIsRequiredNoParam;
                return false;
            }

            if (file == null)
            {
                return true;
            }

            if (field.AllowedFileExtensions != null &&
                field.AllowedFileExtensions.Any() &&
                !field.AllowedFileExtensions.Contains(Path.GetExtension(file.FileName)))
            {
                field.ValidationError = string.Format(MessageConstant.Values.FileTypeRejected, string.Join("; ", field.AllowedFileExtensions));
                return false;
            }

            if (file.Length == 0)
            {
                field.ValidationError = MessageConstant.Values.FileIsEmpty;
                return false;
            }

            if (!(await IsFileAcceptableFormat(file)))
            {
                field.ValidationError = MessageConstant.Values.FileTypeMismatch;
                return false;
            }

            if (file.Length > (field.AllowedFileSizeInMB * 1024 * 1024))
            {
                field.ValidationError =
                    string.Format(MessageConstant.Values.FileExceedsLimit, field.AllowedFileSizeInMB);
                return false;
            }

            //TODO : да се добави проверка с антивирусна програма преди качване

            return true;
        }

        private async Task<bool> ValidateBoolean(FormField field)
        {          
            if (!Boolean.TryParse(field.Value, out bool result))
            {
                field.ValidationError = MessageConstant.InvalidValue;
                return false;
            }

            if (field.IsRequired && !result)
            {
                field.ValidationError = MessageConstant.SelectionIsRequired;
                return false;
            }

            return true;
        }

        private async Task<bool> ValidateDate(FormField field, int userTimeZoneOffsetInMinutes, DateTime? processCreationDateUtc)
        {
            DateTime parsedDate;

            bool success = DateTime.TryParseExact(field.Value,
                field.Type == SimpleFormFieldType.Date.ToString() ? FormattingConstant.NormalDateFormat : FormattingConstant.DateTimeFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out parsedDate);

            //При импорт от е-форми
            if (!success)
            {
                success = DateTime.TryParseExact(field.Value,
                    FormattingConstant.EFormDateFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out parsedDate);

                field.Value = field.Type == SimpleFormFieldType.Date.ToString()
                    ? parsedDate.ToString(FormattingConstant.NormalDateFormat)
                    : parsedDate.ToString(FormattingConstant.DateTimeFormat);
            }

            if (!success)
            {
                field.ValidationError = MessageConstant.RegexFail;
                return false;
            }

            if (field.Type == SimpleFormFieldType.Date.ToString())
            {
                if (!field.AllowFutureDates && parsedDate > (processCreationDateUtc?.Date ?? DateTime.Now.Date))
                {
                    field.ValidationError = MessageConstant.SelectPastDate;
                    return false;
                }
                if (!field.AllowPastDates && parsedDate < (processCreationDateUtc?.Date ?? DateTime.Now.Date))
                {
                    field.ValidationError = MessageConstant.SelectFutureDate;
                    return false;
                }
            }
            else //DateTime
            {
                DateTime parsedDateInUtc = parsedDate.AddMinutes(userTimeZoneOffsetInMinutes);

                if (!field.AllowFutureDates && parsedDateInUtc > (processCreationDateUtc ?? DateTime.UtcNow))
                {
                    field.ValidationError = MessageConstant.SelectPastDate;
                    return false;
                }
                if (!field.AllowPastDates && parsedDateInUtc < (processCreationDateUtc ?? DateTime.UtcNow))
                {
                    field.ValidationError = MessageConstant.SelectFutureDate;
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> ValidateTime(FormField field)
        {
            bool success = DateTime.TryParseExact(field.Value,
                FormattingConstant.NormalTimeFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime parsedTime);

            if (!success)
            {
                field.ValidationError = MessageConstant.RegexFail;
                return false;
            }
           
            return true;
        }

        private bool ValidateText(FormField field)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(field.Pattern) &&
                    !Regex.IsMatch(field.Value, field.Pattern))
                {
                    field.ValidationError = MessageConstant.RegexFail;
                    return false;
                }
            }
            catch (RegexParseException ex)
            {
                _logger.LogError(ex, $"Изразът {field.Pattern} е неприемлив шаблон за Regex за поле {field.Name} в {nameof(ValidateText)}");
                return true;
            }

            return true;
        }
        
        private bool ValidateFileField(FormField field)
        {
            if (!Guid.TryParse(field.Value, out var result))
            {
                field.ValidationError = "Ключът на файла е в невалиден формат";
                return false;
            }

            return true;
        }

        private bool ValidateNumber(FormField field)
        {
            decimal number = 0;
            //парсваме и . и ,
            string numberWithDotSeparator = field.Value.Replace(',', '.');
            if (!decimal.TryParse(numberWithDotSeparator, CultureInfo.InvariantCulture, out number))
            {
                field.ValidationError = MessageConstant.InvalidValue;
                return false;
            }
            if (field.MinValue.HasValue &&
                number < field.MinValue)
            {
                field.ValidationError = $"{MessageConstant.ValueBelowMinimum} {field.MinValue}";
                return false;
            }
            if (field.MaxValue.HasValue &&
                number > field.MaxValue)
            {
                field.ValidationError = $"{MessageConstant.ValueExceedsMaximum} {field.MaxValue}";
                return false;
            }

            return true;
        }

        private async Task<bool> ValidateSelect(FormField field,
            NomenclatureGrpc.NomenclatureGrpcClient nomenclatureGrpcClient, int registerId)
        {
            AreNomenclatureCodesAllowedRequest areNomenclatureCodesAllowedRequest = new AreNomenclatureCodesAllowedRequest
            {
                RegisterId = registerId,
                NomenclatureType = field.NomenclatureType,
            };

            areNomenclatureCodesAllowedRequest.NomenclatureCodes.AddRange(field.Value.Split(','));

            AreNomenclatureCodesAllowedResponse nomenclatureIsCodeAllowedResponse =
                await nomenclatureGrpcClient.AreNomenclatureCodesAllowedAsync(areNomenclatureCodesAllowedRequest);

            if (nomenclatureIsCodeAllowedResponse.ResultStatus.Code != ResultCodes.Ok)
            {
                _logger.LogError($"IsNomenclatureCodeAllowedAsync неуспешен в {nameof(ValidateSelect)}");
                field.ValidationError = MessageConstant.ValidationFailConnectionIssue;
                return false;
            }

            if (!nomenclatureIsCodeAllowedResponse.AreAllowed)
            {
                field.ValidationError = MessageConstant.InvalidNomenclatureValue;
                return false;
            }

            return true;
        }

        private async Task<bool> ValidateCity(FormField field,
            NomenclatureGrpc.NomenclatureGrpcClient nomenclatureGrpcClient)
        {
            NomenclaturePublicRequest getNomenclaturesRequest = new NomenclaturePublicRequest
            {
                RegisterId = 0,
                NomenclatureTypes = { NomenclatureTypes.Ekatte }
            };

            NomenclaturePublicResponse nomenclatureResult =
                await nomenclatureGrpcClient.GetNomenclaturePublicAsync(getNomenclaturesRequest);

            if (nomenclatureResult.ResultStatus.Code != ResultCodes.Ok)
            {
                _logger.LogError($"GetNomenclaturePublicAsync неуспешен в {nameof(ValidateCity)}");
                field.ValidationError = MessageConstant.ValidationFailConnectionIssue;
                return false;
            }

            var nomenclatureType = nomenclatureResult.NomenclatureTypes.First();

            if (nomenclatureType == null)
            {
                field.ValidationError = MessageConstant.InvalidEKATTEType;
                return false;
            }

            foreach (string selectedValue in field.Value.Split(','))
            {
                if (nomenclatureType.CodeableConcepts.All(cc => cc.Code != selectedValue))
                {
                    field.ValidationError = MessageConstant.InvalidValue;
                    return false;
                }
            }

            return true;
        }

        private bool StartsWith(byte[] array, byte[] prefix)
        {
            if (prefix.Length > array.Length)
                return false;

            for (int i = 0; i < prefix.Length; i++)
            {
                if (array[i] != prefix[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Проверка дали съдържанието на файл отговаря на разширението
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<bool> IsFileAcceptableFormat(IFormFile file)
        {
            var fileExtension = Path.GetExtension(file.FileName);
            if (!fileExtensionsHexSignatures.ContainsKey(fileExtension))
            {
                return false;
            }
            
            byte[] filesAsBytes = [];
            using MemoryStream ms = new MemoryStream();
            await file.CopyToAsync(ms);
            ms.Position = 0; // Връщаме MemoryStream на положение 0 иначе гърми с грешка OException: PDF header not found 
            filesAsBytes = ms.ToArray();
            
            foreach (string potentialHeader in fileExtensionsHexSignatures[fileExtension])
            {
                if (string.IsNullOrEmpty(potentialHeader))
                {
                    return true;
                }
                var sign = potentialHeader.Split(' '); //"FF D8 FF DB" го правим в list с име sign и в случая с 4 елемента. FF D8 FF DB - това e hex формат
                var signatureBytes = new byte[sign.Length];  // arr е временен byte масив в десетичен вид, който приема hex стойностите от sign
                for (int i = 0; i < signatureBytes.Length; i++)
                {
                    signatureBytes[i] = (byte)Convert.ToInt32(sign[i], 16);
                }

                if (StartsWith(filesAsBytes,signatureBytes))
                {
                    return true;
                }
            }

            return false;                      
        }

        /// <summary>
        /// Връща колекция от всички грешки при валидация на полета
        /// </summary>
        /// <param name="model">Валидираният модел</param>
        /// <returns></returns>
        public async Task<Dictionary<string, string>> GetValidatedFormFieldsErrors(FormViewModel model)
        {
            Dictionary<string, string> formFieldErrors = new Dictionary<string, string>();
            await FillFormFieldErrors(model.FormFields, formFieldErrors);

            return formFieldErrors;
        }

        private async Task FillFormFieldErrors(IEnumerable<FormField> validatedFormFields, Dictionary<string, string> formFieldErrors)
        {
            foreach (var formField in validatedFormFields)
            {
                if (!String.IsNullOrEmpty(formField.ValidationError))
                {
                    formFieldErrors.Add(formField.Name, formField.ValidationError);
                }

                if (formField.Repetitions.Any())
                {
                    await FillFormFieldErrors(formField.Repetitions, formFieldErrors);
                }

                if (formField.Fields.Any())
                {
                    await FillFormFieldErrors(formField.Fields, formFieldErrors);
                }
            }
        }
    }
}
