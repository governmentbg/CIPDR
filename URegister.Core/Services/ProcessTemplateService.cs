using Amazon.Runtime.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using URegister.Core.Contracts;
using URegister.Core.Data;
using URegister.Core.Data.Models.Common;
using URegister.Core.Data.Models.Process;
using URegister.Core.Models.OpenData;
using URegister.Core.Models.Service;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Extensions;
using URegister.Infrastructure.Model.RegisterForms;
using URegister.ObjectsCatalog;
using static URegister.ObjectsCatalog.ObjectsCatalogGrpc;

namespace URegister.Core.Services
{
    public class ProcessTemplateService: BaseService, IProcessTemplateService
    {
        private readonly ObjectsCatalogGrpcClient objectsCatalogGrpcClient;
        private readonly IServiceService serviceService;
        private readonly IPublicFieldTemplateService publicFieldTemplateService;
        private readonly IFormConfigurationPersistenceService formConfigurationPersistenceService;
        private readonly IRegisterService _registerService;

        public ProcessTemplateService(
         IApplicationRepository repo,
         ILogger<ProcessTemplateService> logger,
         ObjectsCatalogGrpcClient objectsCatalogGrpcClient,
         IServiceService serviceService,
         IPublicFieldTemplateService publicFieldTemplateService,
         IFormConfigurationPersistenceService formConfigurationPersistenceService,
         IRegisterService registerService
        ) : base(repo, logger)
        {
            this.objectsCatalogGrpcClient = objectsCatalogGrpcClient;
            this.serviceService = serviceService;
            this.publicFieldTemplateService = publicFieldTemplateService;
            this.formConfigurationPersistenceService = formConfigurationPersistenceService;
            _registerService = registerService;
        }

        public string ReplaceFormFieldsInFieldTemplate(string blank, List<RegisterItem> registerItems, string prefix, int index)
        {
            foreach (var registerItem in registerItems)
            {
                var name = registerItem.Name.Replace($"{prefix}_", string.Empty);
                blank = blank.Replace($"{{{{{name}}}}}", string.IsNullOrEmpty(registerItem.ClValue) ? registerItem.Value : registerItem.ClValue);
                name = name.Replace($"{prefix}#{index}_", string.Empty);
                blank = blank.Replace($"{{{{{name}}}}}", string.IsNullOrEmpty(registerItem.ClValue) ? registerItem.Value : registerItem.ClValue);
            }
            return blank;
        }
        public string ReplaceFormFields(string blank, List<RegisterItem> registerItems, string prefix, List<FieldTemplateContentMessage> fieldTemplates, bool textMode)
        {
            foreach (var registerItem in registerItems)
            {
                //NOTE : възможни неочаквани или не актуални стойности, особено при номенклатури. За валути ще показва стойности в лева, ако така са записани
                //TODO : Взимането на данни да става чрез ResolveRegisterItemValues(IEnumerable<dynamic> fieldValues, Dictionary<string, FormField> valuesOfInterest, Dictionary<string, Dictionary<string, string>> cachedNomenclatures)
                var value = string.IsNullOrEmpty(registerItem.ClValue) ? registerItem.Value : registerItem.ClValue;

                if (registerItem.FieldTypeId == (int)SimpleFormFieldType.PersonIdentifier)//#401315
                {
                    value = FormFieldsLayoutService.MaskAfterColonKeepingFirstTwo(value);
                }
                else if(registerItem.FieldTypeId == (int)SimpleFormFieldType.BulgarianCurrency)//Временно решение #402707
                {
                    value = BGCurrencyService.RegistryItemValueToPublicText(registerItem.Value);
                }

                blank = blank.Replace($"{{{{{prefix}{registerItem.Name}}}}}", value);
                if (!string.IsNullOrEmpty(value))
                {
                    value = $"{registerItem.Label} {value}";
                }
                blank = blank.Replace($"{{{{{prefix}{registerItem.Name}_WithPrefix}}}}", value);
                foreach (var fieldTemplate in fieldTemplates)
                {
                    var template = $"{{{{{prefix}{registerItem.Name}_FieldTemplate{fieldTemplate.Id}}}}}";
                    if (blank.IndexOf(template) >= 0)
                    {
                        var fieldBlank = textMode ? fieldTemplate.ContentText : fieldTemplate.Content;
                        var cnt = registerItems.Where(x => x.ParentFieldId == registerItem.FieldId)
                                               .Max(x => (int?)x.Index) ?? 0;
                        var templateValue = string.Empty;
                        for (int i = 0; i <= cnt; i++)
                        {
                            var rItems = registerItems.Where(x => x.ParentFieldId == registerItem.FieldId && x.Index == i).ToList();
                            if (rItems.Any())
                            {
                                templateValue += string.IsNullOrEmpty(templateValue) ? string.Empty :
                                                 (textMode ? Environment.NewLine : "<br>");
                                templateValue += ReplaceFormFieldsInFieldTemplate(
                                    fieldBlank,
                                    rItems,
                                    registerItem.Name,
                                    i);
                            }
                        }
                        blank = blank.Replace(template, templateValue);
                    }
                }
            }

            return blank;
        }
        public string ReplaceProcessParam(string prefix, Process process, string blank)
        {
            blank = blank.Replace($"{{{{{prefix}Process_IncomingNumber}}}}", process.IncomingNumber);
            blank = blank.Replace($"{{{{{prefix}Process_RegisterNumber}}}}", process.RegisterNumber);
            blank = blank.Replace($"{{{{{prefix}Process_IncomingDate}}}}", process.IncomingDate.ToString(FormattingConstant.DateFormat));
            return blank;
        }

        public (string, List<BlanksTemplateParamVM>) ReplaceFormFieldsNotFound(string blank, List<BlanksTemplateParamVM> templateParams, string prefix)
        {
            List<BlanksTemplateParamVM> templateParamsErr = new();
            foreach (var item in templateParams)
            {
                if (blank.Contains($"{{{{{prefix}{item.Name}}}}}"))
                {
                    blank = blank.Replace($"{{{{{prefix}{item.Name}}}}}", string.Empty);
                    templateParamsErr.Add(item);
                } else
                {
                    if (item.Templates?.Any() == true)
                    {
                        List<BlanksTemplateParamVM> templateParamsErrItem;
                        (blank, templateParamsErrItem) = ReplaceFormFieldsNotFound(blank, item.Templates, $"{item.Name}_");
                        templateParamsErr.AddRange(templateParamsErrItem);
                    }
                }
            }
            return (blank, templateParamsErr);
        }
        public async Task<string> GetProcessCertificateHtml(Process process, Process processCertificate, int serviceIdCertificate, List<RegisterItem> registerItemsCertificate, List<RegisterItem> registerItems)
        {
            var response = await objectsCatalogGrpcClient.GetFieldTemplateContentListAsync(new Google.Protobuf.WellKnownTypes.Empty());
            var fieldTemplates = response.FieldTemplates.ToList();
            var blank = await Repo.AllReadonly<BlanksTemplate>()
                                               .Where(x => x.ServiceId == serviceIdCertificate)
                                               .Select(x => x.Content)
                                               .FirstAsync() ?? string.Empty;
            blank = ReplaceFormFields(blank, registerItems, string.Empty, fieldTemplates, false);
            blank = ReplaceFormFields(blank, registerItemsCertificate, "certificate.", fieldTemplates, false);
            blank = ReplaceProcessParam(string.Empty, process, blank);
            blank = ReplaceProcessParam("certificate.", processCertificate, blank);

            return blank;
        }

        public async Task<string> GetProcessCertificateOnRegisterHtml(Process process, List<RegisterItem> registerItems, BlanksTemplate blanksTemplate)
        {
            var response = await objectsCatalogGrpcClient.GetFieldTemplateContentListAsync(new Google.Protobuf.WellKnownTypes.Empty());
            var fieldTemplates = response.FieldTemplates.ToList();
            var blank = blanksTemplate.Content ?? string.Empty;
            blank = ReplaceFormFields(blank, registerItems, string.Empty, fieldTemplates, false);
            blank = ReplaceProcessParam(string.Empty, process, blank);
            return blank;
        }

        /// <summary>
        /// Връща списък с приключени заявени услуги от тип Вписване, като стойностите на сложните полета са конкатинирани стойности на подполетата им
        /// </summary>
        /// ///
        /// <param name="administrationId">Идентификатор на администрация</param>
        /// <param name="skip">Колко записа да бъдат пропуснати</param>
        /// <param name="take">Колко записа да бъдат взети</param>
        /// <param name="searchKey"></param>
        /// <param name="searchPattern"></param>
        /// <param name="toDate">До дата на вписване, включително</param>
        /// <param name="fromDate">От дата на вписване, включително</param>
        /// <param name="searchToDate">До дата за търсене по критерии от тип дата</param>
        /// <param name="searchFromDate">От дата за търсене по критерии от тип дата</param>
        /// <returns></returns>
        public async Task<(JsonResult?, List<Dictionary<string, object>>, List<PublicFieldTemplate>)> GetRegistrationProcessList(
            Guid administrationId, 
            int skip,
            int take, 
            string searchKey,
            string searchPattern, 
            DateTime? toDate, 
            DateTime? fromDate,
            DateTime? searchToDate, 
            DateTime? searchFromDate)
        {
            var templates = await publicFieldTemplateService.GetTemplates();
            List<Dictionary<string, object>> result = new();
            if (!templates.Any()) {
                return (null, result, templates);
            }
            var registerService = await serviceService.GetRegisterService();
            var formModel = await formConfigurationPersistenceService.GetFormViewModel(registerService.FormParentId);
            int pageNumber;
            int perPage;
            if (take == 0)
            {
                perPage = take = 20;
                skip = 0;
                pageNumber = 1;
            }
            else
            {
                perPage = take;
                pageNumber = skip / take + 1;
            }

            Dictionary<string, FormField> valuesOfInterest = new Dictionary<string, FormField>();
            FormConfigurationPersistenceService.AddValuesOfInterestToDictionary(formModel.FormFields,
                valuesOfInterest,
                true,
                false,
                false);

            var processesOfInterest = await GetRegistrationProcessListEntity(
                administrationId,
                searchKey,
                searchPattern,
                toDate?.AddDays(1).ToUniversalTime(),
                fromDate?.ToUniversalTime(),
                searchToDate?.AddDays(1).ToUniversalTime(),
                searchFromDate?.ToUniversalTime(),
                valuesOfInterest
            );
            var processesForPage = processesOfInterest
                .Skip(skip)
                .Take(take);
            var columnData = templates
                 .Select(v => new { label = v.Label, fieldName = v.FieldName }).ToList();
            columnData.Insert(0, new { label = "Дата на последна промяна", fieldName = "ModifiedOn" });
            columnData.Insert(0, new { label = "Номер на вписване", fieldName = "RegisterNumber" });
            
            var response = await objectsCatalogGrpcClient.GetFieldTemplateContentListAsync(new Google.Protobuf.WellKnownTypes.Empty());
            var templateParams = await publicFieldTemplateService.GetTemplateParam(formModel, string.Empty);
            var fieldTemplates = response.FieldTemplates.ToList();
            
            foreach (var process in processesForPage)
            {
                result.Add(ProcessToPublicTemplateField(process, templates, fieldTemplates, templateParams));
            }

            bool historyNotPublic = (await _registerService.GetCurrentRegister()).HistoryNotPublic;

            var combinedData = new
            {
                searchOptions = valuesOfInterest
                    .Select(v => new { label = v.Value.Label, fieldName = v.Key, type = v.Value.Type }).ToArray(),
                columnData = columnData,
                data = result,
                metadata = new
                {
                    totalRecordsFiltered = processesOfInterest.Count(),
                    pageNumber = pageNumber,
                    perPage = perPage,
                    historyNotPublic
                }
            };
            return (new JsonResult(combinedData), result, templates);
        }

        private async Task<List<Process>> GetRegistrationProcessListEntity(Guid administrationId,
            string? searchKey,
            string? searchPattern,
            DateTime? toDateUTC,
            DateTime? fromDateUTC,
            DateTime? searchToDateUTC,
            DateTime? searchFromDateUTC,
            Dictionary<string, FormField> valuesOfInterest)
        {
            var services = await Repo.AllReadonly<Service>()
                                     .Where(x => x.ServiceTypeId == (int)ServiceTypes.Register ||
                                                 x.ServiceTypeId == (int)ServiceTypes.Change ||
                                                 x.ServiceTypeId == (int)ServiceTypes.AskForCorrectionError)
                                     .Select(x => x.Id)
                                     .ToListAsync();
            var query = Repo.AllReadonly<Process>()
               .Include(x => x.RegisterItems)
               .TagWith(nameof(GetRegistrationProcessListEntity))
               .Where(x => services.Contains(x.ServiceId))
               .Where(x => x.RegisterItems.Any())
               .Where(p => p.StatusId == (int)ProcessStatus.Registered)
               .Where(p => p.TenantId == administrationId);

            if (fromDateUTC.HasValue)
            {
                query = query.Where(p => p.ModifiedOn >= fromDateUTC);
            }

            if (toDateUTC.HasValue)
            {
                query = query.Where(p => p.ModifiedOn < toDateUTC);
            }

            if (!string.IsNullOrEmpty(searchKey) && !string.IsNullOrEmpty(searchPattern)
                                                 || searchToDateUTC != null || searchFromDateUTC != null)
            {
                var cachedNomenclatures = 
                    await formConfigurationPersistenceService.CacheNomenclaturesForValuesOfInterest(valuesOfInterest);

               
                //Търси по поле тип дата
                if (searchFromDateUTC != null || searchToDateUTC != null)
                {
                    query = query.Where(p => p.RegisterItems.Any(ri => ri.Name == searchKey &&
                                                                       ri.DateTimeValue != null &&
                                                                       (searchToDateUTC == null ||  ri.DateTimeValue < searchToDateUTC) &&
                                                                       (searchFromDateUTC == null || ri.DateTimeValue >= searchFromDateUTC)
                    ));
                }
                else
                {
                    bool isSearchPatternValid = FormConfigurationPersistenceService.TryDetermineSearchPattern(searchKey,
                        searchPattern,
                        valuesOfInterest,
                        cachedNomenclatures,
                        out List<string> searchPatterns);

                    if (!isSearchPatternValid)
                    {
                        return new AutoConstructedList<Process>();
                    }

                    query = query.Where(p => p.RegisterItems.Any(ri => ri.Name == searchKey &&
                                                                       (!searchPatterns.Any() ||
                                                                        searchPatterns.Any(sp =>
                                                                            ri.Name == searchKey &&
                                                                            EF.Functions.ILike(ri.Value, sp)
                                                                        ))));
                }
            }

            return await query
                .OrderByDescending(p => p.ModifiedOn)
                .ToListAsync();
        }

        public Dictionary<string, object> ProcessToPublicTemplateField(
            Process process, 
            List<PublicFieldTemplate> templates, 
            List<FieldTemplateContentMessage> fieldTemplates,
            List<BlanksTemplateParamVM> templateParams)
        {
            Dictionary<string, object> jsonFields = templates.ToDictionary(k => k.FieldName, k => (object)string.Empty);

            foreach (var template in templates.Where(t => !string.IsNullOrWhiteSpace(t.Content)))
            {
                var blank = template.Content ?? string.Empty;
                blank = ReplaceFormFields(blank, process.RegisterItems, string.Empty, fieldTemplates, true);
                blank = ReplaceProcessParam(string.Empty, process, blank);
                (blank, var templateParamErr) = ReplaceFormFieldsNotFound(blank, templateParams, string.Empty);
                jsonFields[template.FieldName] = blank;
            }
            if (!jsonFields.ContainsKey("ProcessId"))
            {
                jsonFields.Add("ProcessId", process.Id.ToString());
            }

            if (!jsonFields.ContainsKey(nameof(process.RegisterNumber)))
            {
                jsonFields.Add(nameof(process.RegisterNumber), process.RegisterNumber);
            } 
            
            if (!jsonFields.ContainsKey(nameof(process.ModifiedOn)))
            {
                jsonFields.Add(nameof(process.ModifiedOn), process.ModifiedOn.ConvertUtcToBGTime());
            }

            return jsonFields;
        }


        public IEnumerable<IEnumerable<string>> ProcessForOpenData(List<Dictionary<string, object>> data,  List<OpenDataColVM> cols)
        {
            List<List<string>> result = new ();
            var resultItem = new List<string>();
            foreach (var col in cols)
            {
                resultItem.Add(col.Label ?? string.Empty);
            }
            result.Add(resultItem);
            foreach (var row in data)
            {
                resultItem = new List<string>();
                foreach (var col in cols)
                {
                    if (row.ContainsKey(col.Key!))
                    {
                        resultItem.Add(row[col.Key!]?.ToString() ?? string.Empty);
                    } else
                    {
                        resultItem.Add(string.Empty);
                    }
                }
                result.Add(resultItem);
            }
            return result;
        }
    }
}
