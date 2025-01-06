using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.RegularExpressions;
using MHRegistries.Core.Services;
using URegister.Common;
using URegister.Core.Contracts;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Extensions;
using URegister.Infrastructure.Model.RegisterForms;
using URegister.NomenclaturesCatalog;

namespace URegister.Core.Services
{
    /// <summary>
    /// Сервиз за back end валидация на данните въведени във форма
    /// </summary>
    public class FormValidationService : IFormValidationService
    {
        private readonly ILogger<FormFieldsLayoutService> _logger;
        const string DateOnlyFormat = "dd.MM.yyyy";
        const string DateTimeFormat = "dd.MM.yyyy HH:mm";
        private static readonly Dictionary<string, List<string>> fileExtensionsHexSignatures = new Dictionary<string, List<string>>
            {
                { ".xml", new List<string> { "3C 3F 78 6D 6C 20" } },
                { ".pdf", new List<string> { "25 50 44 46 2D" } },
                { ".doc", new List<string> { "D0 CF 11 E0 A1 B1 1A E1" } },
                { ".sxw", new List<string> { "50 4B 03 04", "50 4B 05 06", "50 4B 07 08" } },
                { ".txt", new List<string> { "EF BB BF", "FF FE", "FE FF", "FF FE 00 00", "00 00 FE FF" } },
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
        /// <param name="nomenclatureGrpcClient">GRPC клиент за номенклатуро</param>
        /// <param name="registerId">Идентификатор на регистъра</param>
        /// <returns>Всички стойности ли са валидни</returns>
        public async Task<bool> ValidateViewModel(FormViewModel viewModel,
            NomenclatureGrpc.NomenclatureGrpcClient nomenclatureGrpcClient,
            int registerId)
        {
            return await ValidateViewModelFields(viewModel.FormFields, nomenclatureGrpcClient, registerId);
        }

        private async Task<bool> ValidateViewModelFields(IEnumerable<FormField> formFieldsForValidation,
            NomenclatureGrpc.NomenclatureGrpcClient nomenclatureGrpcClient,
            int registerId,
            bool validSoFar = true)
        {
            foreach (FormField field in formFieldsForValidation)
            {
                if ((string.IsNullOrWhiteSpace(field.Value) || 
                     (field.Type == "PersonIdentifier" && field.Value.Trim().Split(':').Any(p => string.IsNullOrWhiteSpace(p))))
                       && field.Type != "File"
                       && (field.Fields == null || field.Fields.IsEmpty()))
                {
                    if (field.Type == "Boolean")
                    {
                        field.ValidationError = MessageConstant.InvalidValue;
                        validSoFar = false;
                        continue;
                    }
                    if (field.Type == "PersonIdentifier")
                    {
                        field.Value = string.Empty;
                    }
                    if (field.IsRequired)
                    {
                        field.ValidationError = MessageConstant.FieldIsRequiredNoParam;
                        validSoFar = false;
                    }
                    continue;
                }

                if (string.IsNullOrWhiteSpace(field.Value) && field.Type != "File")
                {
                    //Всички сложни типове
                    if (field.Fields != null && field.Fields.Any())
                    {
                        validSoFar = await ValidateViewModelFields(field.Fields, nomenclatureGrpcClient, registerId) && validSoFar;
                    }

                    switch (field.Type)
                    {
                        case "IndividualIdentifier":
                            validSoFar = ValidateIndividualIdentifier(field) && validSoFar;
                            break;
                        case "Address":
                            validSoFar = await ValidateAddress(field, nomenclatureGrpcClient, registerId) && validSoFar;
                            break;
                        default:
                            break;
                    }
                    continue;
                }

                switch (field.Type)
                {
                    case "Number":
                        validSoFar = ValidateNumber(field) && validSoFar;
                        break;
                    case "Text":
                    case "TextArea":
                    case "Email":
                    case "Phone":
                    case "Url":
                        validSoFar = ValidateText(field) && validSoFar;
                        break;
                    case "File":
                        validSoFar = await ValidateFile(field) && validSoFar;
                        break;
                    case "Date":
                    case "DateTime":
                        validSoFar = await ValidateDate(field) && validSoFar;
                        break;
                    case "Boolean":
                        validSoFar = await ValidateBoolean(field) && validSoFar;
                        break;
                    case "Select":
                    case "Autocomplete":
                    case "MultiSelect":
                        validSoFar = await ValidateSelect(field, nomenclatureGrpcClient, registerId) && validSoFar;
                        break;
                    case "City":
                        validSoFar = await ValidateCity(field, nomenclatureGrpcClient) && validSoFar;
                        break;
                    case "PersonIdentifier":
                        validSoFar = await ValidatePid(field, nomenclatureGrpcClient, registerId) && validSoFar;
                        break;
                }

                if (field.Repetitions != null)
                {
                    validSoFar = await ValidateViewModelFields(field.Repetitions, nomenclatureGrpcClient, registerId) && validSoFar;
                }
            }

            return validSoFar;
        }

        private bool ValidateIndividualIdentifier(FormField field)
        {
            var birthCountrySubfield =
                field.Fields!.SingleOrDefault(f =>
                    f.Name.Contains("birthCountryImmutable", StringComparison.InvariantCultureIgnoreCase));

            if (birthCountrySubfield == null)
            {
                field.ValidationError = "Невалиден шаблон за идентификация";
                return false;
            }

            if (birthCountrySubfield.Value == "BG")
            {
                var birthPlaceBg =
                    field.Fields!.SingleOrDefault(f =>
                        f.Name.Contains("birthPlaceBgImmutable", StringComparison.InvariantCultureIgnoreCase));

                if (birthPlaceBg == null)
                {
                    field.ValidationError = "Невалиден шаблон за идентификация";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(birthPlaceBg.Value))
                {
                    birthPlaceBg.ValidationError = "Въведете място на раждане";
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
                    field.ValidationError = "Невалиден шаблон за идентификация";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(birthPlaceAbroad.Value))
                {
                    birthPlaceAbroad.ValidationError = "Въведете място на раждане";
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
                field.ValidationError = "Невалиден формат на стойността";
                return false;
            }

            if (!int.TryParse(valueComponents[0], out int parsedPidType))
            {
                field.ValidationError = "Невалиден тип идентификатор";
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
                field.ValidationError = "Неуспешна валидация, проблем с връзката, опитайте пак";
                return false;
            }

            if (!areNomenclatureCodesAllowedResponse.AreAllowed)
            {
                field.ValidationError = "Непознат тип идентификатор";
                return false;
            }

            if (!PidValidateService.ValidatePersonalId(valueComponents[1], parsedPidType))
            {
                field.ValidationError = "Невалиден идентификатор";
                return false;
            }

            return true;
        }

        private async Task<bool> ValidateAddress(FormField field,
            NomenclatureGrpc.NomenclatureGrpcClient nomenclatureGrpcClient, int registerId)
        {
            var settlementImmutable = field.Fields.SingleOrDefault(f => f.Name.Contains("settlementImmutable", StringComparison.InvariantCultureIgnoreCase));

            var streetField = field.Fields.SingleOrDefault(f => f.Name.Contains("streetImmutable", StringComparison.InvariantCultureIgnoreCase));

            if (streetField != null && !string.IsNullOrWhiteSpace(streetField.Value))
            {
                AreNomenclatureCodesAllowedRequest areNomenclatureCodesAllowedRequest = new AreNomenclatureCodesAllowedRequest
                {
                    RegisterId = registerId,
                    NomenclatureType = NomenclatureTypes.EkStreet,
                    NomenclatureCodes = { streetField.Value },
                    Holder = settlementImmutable!.Value
                };

                AreNomenclatureCodesAllowedResponse areNomenclatureCodesAllowedResponse =
                    await nomenclatureGrpcClient.AreNomenclatureCodesAllowedAsync(areNomenclatureCodesAllowedRequest);

                if (areNomenclatureCodesAllowedResponse.ResultStatus.Code != ResultCodes.Ok)
                {
                    _logger.LogError($"GetNomenclaturePublicAsync неуспешен в {nameof(ValidatePid)}");
                    streetField.ValidationError = "Неуспешна валидация, проблем с връзката, опитайте пак";
                    return false;
                }

                if (!areNomenclatureCodesAllowedResponse.AreAllowed)
                {
                    streetField.ValidationError = "Непозната улица за населеното място";
                    return false;
                }
            }

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
                    regionField.ValidationError = "Неуспешна валидация, проблем с връзката, опитайте пак";
                    return false;
                }

                if (!areNomenclatureCodesAllowedResponse.AreAllowed)
                {
                    regionField.ValidationError = "Непознат район за населеното място";
                    return false;
                }
            }

            var districtField = field.Fields.SingleOrDefault(f => f.Name.Contains("districtImmutable", StringComparison.InvariantCultureIgnoreCase));

            if (districtField != null && !string.IsNullOrWhiteSpace(districtField.Value))
            {
                AreNomenclatureCodesAllowedRequest areNomenclatureCodesAllowedRequest = new AreNomenclatureCodesAllowedRequest
                {
                    RegisterId = registerId,
                    NomenclatureType = NomenclatureTypes.EkKvartal,
                    NomenclatureCodes = { districtField.Value },
                    Holder = settlementImmutable!.Value
                };

                AreNomenclatureCodesAllowedResponse areNomenclatureCodesAllowedResponse =
                    await nomenclatureGrpcClient.AreNomenclatureCodesAllowedAsync(areNomenclatureCodesAllowedRequest);

                if (areNomenclatureCodesAllowedResponse.ResultStatus.Code != ResultCodes.Ok)
                {
                    _logger.LogError($"GetNomenclaturePublicAsync неуспешен в {nameof(ValidatePid)}");
                    districtField.ValidationError = "Неуспешна валидация, проблем с връзката, опитайте пак";
                    return false;
                }

                if (!areNomenclatureCodesAllowedResponse.AreAllowed)
                {
                    districtField.ValidationError = "Непознат квартал за населеното място";
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> ValidateFile(FormField field)
        {
            if (field.File == null && field.IsRequired)
            {
                field.ValidationError = MessageConstant.FieldIsRequiredNoParam;
                return false;
            }

            if (field.File == null)
            {
                return true;
            }

            if (field.AllowedFileExtensions != null && !field.AllowedFileExtensions.Contains(Path.GetExtension(field.File.FileName)))
            {
                field.ValidationError = $"Форматът на файла е неприемлив. Изберете {String.Join("; ", field.AllowedFileExtensions)}";
                return false;
            }

            if (!(await IsFileAcceptableFormat(field.File)))
            {
                field.ValidationError = $"Разширението на файла не отговаря на съдържанието му.";
                return false;
            }

            if (field.File.Length > (field.AllowedFileSizeInMB * 1024 * 1024))
            {
                field.ValidationError = $"Файлът е по-голям от {field.AllowedFileSizeInMB} MB! Файлът не е записан";
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
                field.ValidationError = "Изборът е задължителен";
                return false;
            }

            return true;
        }

        private async Task<bool> ValidateDate(FormField field)
        {
            bool success = DateTime.TryParseExact(field.Value,
                field.Type == "Date" ? DateOnlyFormat : DateTimeFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime parsedDate);

            if (!success)
            {
                field.ValidationError = MessageConstant.RegexFail;
                return false;
            }

            if (field.Type == "Date")
            {
                if (!field.AllowFutureDates && parsedDate > DateTime.Now.Date)
                {
                    field.ValidationError = "Изберете отминала дата";
                    return false;
                }
                if (!field.AllowPastDates && parsedDate < DateTime.Now.Date)
                {
                    field.ValidationError = "Изберете не отминала дата";
                    return false;
                }
            }
            else
            {
                if (!field.AllowFutureDates && parsedDate > DateTime.Now)
                {
                    field.ValidationError = "Изберете отминала дата";
                    return false;
                }
                if (!field.AllowPastDates && parsedDate < DateTime.Now)
                {
                    field.ValidationError = "Изберете не отминала дата";
                    return false;
                }
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
                field.ValidationError = $"Стойността е по-малка от минимума {field.MinValue}";
                return false;
            }
            if (field.MaxValue.HasValue &&
                number > field.MaxValue)
            {
                field.ValidationError = $"Стойността е по-голяма от максимума {field.MaxValue}";
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
                field.ValidationError = "Неуспешна валидация, проблем с връзката, опитайте пак";
                return false;
            }

            if (!nomenclatureIsCodeAllowedResponse.AreAllowed)
            {
                field.ValidationError = "Невалидна стойност за номенклатура";
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
                field.ValidationError = "Неуспешна валидация, проблем с връзката, опитайте пак";
                return false;
            }

            var nomenclatureType = nomenclatureResult.NomenclatureTypes.First();

            if (nomenclatureType == null)
            {
                field.ValidationError = "Невалиден тип на EKATTE";
                return false;
            }

            foreach (string selectedValue in field.Value.Split(','))
            {
                if (nomenclatureType.CodeableConcepts.All(cc => cc.Code != selectedValue))
                {
                    field.ValidationError = "Невалидна стойност";
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
    }
}
