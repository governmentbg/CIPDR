using Microsoft.AspNetCore.Mvc;
using OpenDataClient;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using URegister.Core.Contracts;
using URegister.Core.Models.OpenData;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Model.RegisterForms;
using URegister.RegistersCatalog;

namespace URegister.Areas.Public.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Display(Name = "Заявени услуги")]
    public class ProcessController : BaseController
    {
        private readonly ILogger<ProcessController> _logger;
        private readonly IProcessService _processService;
        private readonly IProcessTemplateService processTemplateService;
        private readonly RegistersCatalogGrpc.RegistersCatalogGrpcClient _registerGrpcClient;
        private readonly IFormConfigurationPersistenceService _formConfigurationPersistenceService;
        private readonly IOpenDataClientService openDataService;
        private readonly IRegisterService registerService;

        public ProcessController(ILogger<ProcessController> logger,
            IProcessService processService,
            RegistersCatalogGrpc.RegistersCatalogGrpcClient registerGrpcClient,
            IFormConfigurationPersistenceService formConfigurationPersistenceService,
            IProcessTemplateService processTemplateService,
            IOpenDataClientService _openDataService,
            IRegisterService registerService)
        {
            _logger = logger;
            _processService = processService;
            _registerGrpcClient = registerGrpcClient;
            _formConfigurationPersistenceService = formConfigurationPersistenceService;
            this.processTemplateService = processTemplateService;
            openDataService = _openDataService;
            this.registerService = registerService;
        }

        ///// <summary>
        ///// Връща всички въведени от потребителят данни за дадена форма
        ///// </summary>
        ///// <param name="processId">Идентификатор на заявена услуга</param>
        ///// <returns></returns>
        //[HttpGet("form-data-submitted-by-person")]
        //[Display(Name = "Извличане на всички въведени от потребителят данни за дадена форма")]
        //public async Task<IActionResult> GetFormData(Guid processId)
        //{
        //    try
        //    {                            
        //        JsonResult result = 
        //            await _processService.GetFormData(processId);

        //        if (result == null)
        //        {
        //            return StatusCode(500);
        //        }

        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Проблем при изпълнението на {nameof(GetFormData)} с параметри {processId}.");
        //        return StatusCode(500);
        //    }
        //}

        /// <summary>
        /// Връща модел на форма от записани данни
        /// </summary>
        /// /// <param name="processId">Идентификатор на заявена услуга</param>
        /// <returns></returns>
        [HttpGet("get-form-model-for-saved-data")]
        [Display(Name = "Извличане на модел на форма от записани данни")]
        public async Task<IActionResult> GetFormModelForSavedData([FromQuery] Guid processId)
        {
            string loggedUserPid = null;// Да се взима от userContext или от Token, празно ако user не е логнат

            List<Guid> userMpris = new List<Guid>();

            if (!string.IsNullOrWhiteSpace(loggedUserPid))
            {

                var requestMPRI = new GetMasterPersonRecordIndexMessage();
                requestMPRI.Pid = loggedUserPid;
                var responseMPRI = await _registerGrpcClient.GetMasterPersonRecordIndexAsync(requestMPRI);

                if (responseMPRI.Status.Code != Common.ResultCodes.Ok)
                {
                    _logger.LogError($"Грешка {responseMPRI.Status.Code} при извикване на {nameof(_registerGrpcClient.GetMasterPersonRecordIndexAsync)} в {nameof(GetFormModelForSavedData)}.");
                    return StatusCode(500);
                }

                userMpris.AddRange(responseMPRI.Items.Select(r => new Guid(r.Id)));
            }
            
            try
            {              
                FormViewModel formModel =
                    await _formConfigurationPersistenceService.GetFormModelForSavedData(userMpris, processId);

                if (formModel == null)
                {
                    return StatusCode(500);
                }

                ConcatenateSubfields(formModel.FormFields);
                IEnumerable<FormField> formModelWithoutEmptyFields = RemoveEmptyFields(formModel.FormFields);

                formModel.FormFields = formModelWithoutEmptyFields.ToList();

                return new JsonResult(formModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Проблем при изпълнението на {nameof(GetFormModelForSavedData)} с параметри {processId}.");
                return StatusCode(500);
            }
        }

        private IEnumerable<FormField> RemoveEmptyFields(List<FormField> formModelFormFields)
        {
            return formModelFormFields.Where(f => !string.IsNullOrWhiteSpace(f.Value));
        }

        private void ConcatenateSubfields(IEnumerable<FormField> formFields)
        {
            foreach (FormField formModelFormField in formFields)
            {
                if (formModelFormField.Fields.Any())
                {
                    string concatValue = JoinSubFields(", ", formModelFormField.Fields.Where(f => !string.IsNullOrWhiteSpace(f.Value)));

                    formModelFormField.Type = SimpleFormFieldType.Text.ToString();
                    formModelFormField.Fields = new List<FormField>();
                    formModelFormField.Value = concatValue;
                }

                if (formModelFormField.Repetitions.Any())
                {
                    ConcatenateSubfields(formModelFormField.Repetitions);
                }
            }
        }

        private static string JoinSubFields(string separator, IEnumerable<FormField> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            if (separator == null)
                separator = string.Empty;

            using (var enumerator = values.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    return string.Empty;

                string firstValue = enumerator.Current?.Value ?? string.Empty;
                StringBuilder result = new StringBuilder(firstValue);
                string previousFieldName = enumerator.Current?.Name ?? string.Empty;

                while (enumerator.MoveNext())
                {
                    string currentValue = enumerator.Current?.Value ?? string.Empty;
                    string currentName = enumerator.Current?.Name ?? string.Empty;

                    // Use space as separator if previous label ends with "firstNameImmutable" and current label ends with "lastNameImmutable"
                    string effectiveSeparator = (previousFieldName.EndsWith(ComplexFieldsNameConstants.FirstNameImmutable, StringComparison.OrdinalIgnoreCase)
                                                 && currentName.EndsWith(ComplexFieldsNameConstants.LastNameImmutable, StringComparison.OrdinalIgnoreCase))
                        ? " "
                        : separator;

                    result.Append(effectiveSeparator);
                    result.Append(currentValue);
                    previousFieldName = currentName;
                }

                return result.ToString();
            }
        }

        /// <summary>
        /// Връща списък с всички услуги
        /// </summary>
        /// <param name="skip">Колко записа да бъдат пропуснати</param>
        /// <param name="take">Колко записа да бъдат взети</param>
        /// <returns></returns>
        [HttpGet("get-all-services")]
        [Display(Name = "Извличане на списък с всички услуги")]
        //[Authorize]
        public async Task<IActionResult> GetAllServiceList(int skip, int take)
        {
            var result = await _formConfigurationPersistenceService.GetAllServiceList(skip, take);

            if (result == null)
            {
                return StatusCode(500);
            }

            return result;
        }

        /// <summary>
        /// Връща списък с приключени заявени услуги от тип Вписване, всяко подполе е колона
        /// </summary>
        /// /// <param name="administrationId">Идентификатор на администрация</param>
        /// <param name="skip">Колко записа да бъдат пропуснати</param>
        /// <param name="take">Колко записа да бъдат взети</param>
        /// <param name="searchKey">Критерии за търсене</param>
        /// <param name="searchPattern">Низ зая търсене</param>
        /// <param name="toDate">До дата на вписване, включително</param>
        /// <param name="fromDate">От дата на вписване, включително</param>
        /// <returns></returns>
        [HttpGet("get-registration-processes-subfields-columns")]
        [Display(Name = "Извличане на списък с приключени заявени услуги от тип 'Вписване'")]
        //[Authorize]
        public async Task<IActionResult> GetRegistrationProcessListSubfieldsColumns(
            Guid administrationId, 
            int skip, 
            int take, 
            string searchKey = "", 
            string searchPattern = "",
            DateTime? toDate = null,
            DateTime? fromDate= null)
        {                            
            var result = await _formConfigurationPersistenceService.GetRegistrationProcessListWhereSubfieldsAreColumns(
                administrationId, skip, take, searchKey, searchPattern, toDate, fromDate);

            if (result == null)
            {
                return StatusCode(500);
            }

            return result;
        }

        /// <summary>
        /// Връща списък с приключени заявени услуги от тип Вписване
        /// </summary>
        /// /// <param name="administrationId">Идентификатор на администрация</param>
        /// <param name="skip">Колко записа да бъдат пропуснати</param>
        /// <param name="take">Колко записа да бъдат взети</param>
        /// <param name="searchKey">Критерии за търсене</param>
        /// <param name="searchPattern">Низ зая търсене</param>
        /// <param name="toDate">До дата на вписване, включително</param>
        /// <param name="fromDate">От дата на вписване, включително</param>
        /// <param name="toSearchDate">До дата за търсене по критерии от тип дата</param>
        /// <param name="fromSearchDate">От дата за търсене по критерии от тип дата</param>
        /// <returns></returns>
        [HttpGet("get-registration-processes")]
        [Display(Name = "Извличане на списък с приключени заявени услуги от тип 'Вписване'")]
        //[Authorize]
        public async Task<IActionResult> GetRegistrationProcessList(
            Guid administrationId,
            int skip,
            int take,
            string searchKey = "",
            string searchPattern = "",
            DateTime? toDate = null,
            DateTime? fromDate = null,
            DateTime? toSearchDate = null,
            DateTime? fromSearchDate = null)
        {
            _logger.LogInformation(
                string.Format("Method: {0}, Parameters: administrationId={1}, skip={2}, take={3}, searchKey={4}, searchPattern={5}, toDate={6}, fromDate={7}, searchToDate={8}, searchFromDate={9}",
                nameof(GetRegistrationProcessList),
                administrationId,
                skip,
                take,
                searchKey,
                searchPattern,
                toDate?.ToString("o") ?? "null",
                fromDate?.ToString("o") ?? "null",
                toSearchDate?.ToString("o") ?? "null",
                fromSearchDate?.ToString("o") ?? "null"));

            (var resultTemplates, _, _) = await processTemplateService.GetRegistrationProcessList(
             administrationId, skip, take, searchKey, searchPattern, toDate, fromDate, toSearchDate, fromSearchDate);
            if (resultTemplates != null)
            {
                return resultTemplates;
            }

            (var result, _, _) = await _formConfigurationPersistenceService.GetRegistrationProcessListWhereSubfieldsAreConcatenated(
                administrationId, skip, take, searchKey, searchPattern, toDate, fromDate, toSearchDate, fromSearchDate);

            if (result == null)
            {
                return StatusCode(500);
            }

            return result;
        }

        /// <summary>
        /// Извличане на списък с вписани услуги за OpenData
        /// </summary>
        /// /// <param name="administrationId">Идентификатор на администрация</param>
        /// <returns></returns>
        [HttpGet("get-opendata-processes")]
        [Display(Name = "Извличане на списък с вписани услуги за OpenData")]
        //[Authorize]
        public async Task<IActionResult> GetProcessListOpenData(Guid administrationId, bool redirect)
        {
            _logger.LogInformation(
                string.Format("Method: {0}, Parameters: administrationId={1}",
                nameof(GetRegistrationProcessList),
                administrationId));
            int skip = 0;
            int take = 10000000;
            string searchKey = "";
            string searchPattern = "";
            DateTime? toDate = null;
            DateTime? fromDate = null;
            DateTime? toSearchDate = null;
            DateTime? fromSearchDate = null;
            IEnumerable<IEnumerable<string>> request;
            (var resultTemplates, var data, var templates) = await processTemplateService.GetRegistrationProcessList(
             administrationId, skip, take, searchKey, searchPattern, toDate, fromDate, toSearchDate, fromSearchDate);
            if (resultTemplates != null)
            {
                request = processTemplateService.ProcessForOpenData(data, templates.Select(x => new OpenDataColVM { Key = x.FieldName, Label = x.Label }).ToList());
            }
            else
            {
                (_, var dataConcatenated, var fields) = await _formConfigurationPersistenceService.GetRegistrationProcessListWhereSubfieldsAreConcatenated(
                    administrationId, skip, take, searchKey, searchPattern, toDate, fromDate, toSearchDate, fromSearchDate);
                request = processTemplateService.ProcessForOpenData(dataConcatenated, fields.Select(x => new OpenDataColVM { Key = x.Key, Label = x.Value.Label}).ToList());
            }
            var register = await registerService.GetCurrentRegister();
            var response = await _registerGrpcClient.GetOpenDataParamAsync(new OpenDataParamRequest
            {
                AdministrationId = administrationId.ToString(),
                RegisterId = register.Id
            });
            if (request == null)
            {
                return StatusCode(500);
            }
            var dataSetId = response.Data.DataSetId;
            if (string.IsNullOrEmpty(response.Data.DataSetId))
            {
                dataSetId = await openDataService.AddDatasetAsync(response.Data.OrganisationId, register.Name, register.Code, response.Data.CategoryId, 1);
                var responseDataSet = await _registerGrpcClient.SaveOpenDataRegisterAdministrationMetaAsync(new OpenDataRegisterAdministrationMetaSaveRequest
                {
                    AdministrationId = administrationId.ToString(),
                    RegisterId = register.Id,
                    DataSetId = dataSetId,
                });
            }
            var resourceMetaId = response.Data.ResourceMetaId;
            if (string.IsNullOrEmpty(response.Data.ResourceMetaId))
            {
                var metResponse = await openDataService.AddResourceMetadataAsync(dataSetId ?? string.Empty, $"Вписвания", $"Registered");
                resourceMetaId = metResponse.Data.Uri; 
                var responseMeta = await _registerGrpcClient.SaveOpenDataRegisterAdministrationMetaAsync(new OpenDataRegisterAdministrationMetaSaveRequest
                {
                    AdministrationId = administrationId.ToString(),
                    RegisterId = register.Id,
                    DataSetId = dataSetId,
                    ResourceMetaId = metResponse.Data.Uri,
                });
                await openDataService.AddResourceDataAsync(resourceMetaId, request);
            } else
            {
                await openDataService.UpdateResourceDataAsync(resourceMetaId, request);
            }

            if (redirect)
            {
                TempData[MessageConstant.SuccessMessage] = "Успешeн запис в OpenData";
                return RedirectToAction("OpenDataAdministration", "Register", new { area = "Admin", administrationId });
            }
            return StatusCode(200);
        }

        /// <summary>
        /// Връща списък със заявени услуги за текущо логнатия потребител
        /// </summary>
        /// <param name="roleInProcessType"></param>
        /// <param name="skip">Колко записа да бъдат пропуснати</param>
        /// <param name="take">Колко записа да бъдат взети</param>
        /// <returns></returns>
        [HttpGet("get-processes")]
        [Display(Name = "Извличане на списък със заявени услуги за текущо логнатия потребител")]
        //[Authorize]
        public async Task<IActionResult> GetProcessList(int roleInProcessType, int skip, int take)
        {
            string loggedUserPid = "831641791";// Да се взима от userContext или от Token

            var requestMPRI = new GetMasterPersonRecordIndexMessage();
            requestMPRI.Pid = loggedUserPid;
            var responseMPRI = await _registerGrpcClient.GetMasterPersonRecordIndexAsync(requestMPRI);

            if (responseMPRI.Status.Code != Common.ResultCodes.Ok) 
            {
                _logger.LogError($"Грешка {responseMPRI.Status.Code} при извикване на {nameof(_registerGrpcClient.GetMasterPersonRecordIndexListAsync)} в {nameof(GetProcessList)}.");
                return StatusCode(500);
            }

            var result = await _formConfigurationPersistenceService.GetProcessList(responseMPRI.Items.Select(r =>new Guid(r.Id)),
                roleInProcessType, skip, take);

            if (result == null) 
            {
                return StatusCode(500);
            }

            return result;
        }

        /// <summary>
        /// Връща списък със завършени заявени услуги за собственик на партида, за потребител, който не е логнат
        /// </summary>
        /// <param name="pid">Идентификатор на лице, при непосочен връща всички</param>
        /// <param name="serviceId">Идентификатор на услугата, за която се извличат данни</param>
        /// <param name="skip">Колко записа да бъдат пропуснати</param>
        /// <param name="take">Колко записа да бъдат взети</param>
        /// <returns></returns>
        [HttpGet("get-process-list-for-master-person-record")]
        [Display(Name = "Извличане на списък със завършени заявени услуги за собственик на партида, за потребител, който не е логнат")]
        public async Task<IActionResult> GetProcessListForMasterPersonRecord(string? pid, int serviceId, int skip, int take)
        {        
            List<Guid> mpris = new List<Guid>();

            if (!String.IsNullOrEmpty(pid))
            {              
                var requestMPRI = new GetMasterPersonRecordIndexMessage();
                requestMPRI.Pid = pid;
                var responseMPRI = await _registerGrpcClient.GetMasterPersonRecordIndexAsync(requestMPRI);

                if (responseMPRI.Status.Code != Common.ResultCodes.Ok)
                {
                    _logger.LogError($"Грешка {responseMPRI.Status.Code} при извикване на {nameof(_registerGrpcClient.GetMasterPersonRecordIndexAsync)} в {nameof(GetProcessListForMasterPersonRecord)}.");
                    return StatusCode(500);
                }

                mpris.AddRange(responseMPRI.Items.Select(i => new Guid(i.Id)));
            }
            var result = await _formConfigurationPersistenceService.GetDataForPublicTable(mpris, serviceId, skip, take);

            if (result == null)
            {
                return StatusCode(500);
            }

            return result;
        }

        /// <summary>
        /// Връща историята на заявена услуга
        /// </summary>
        /// <param name="processId">Идентификатор на процесс</param>
        /// <returns></returns>
        [HttpGet("get-processes-history")]
        [Display(Name = "Преглед на история на заявена услуга")]
        //[Authorize]
        public async Task<IActionResult> GetProcessHistory(Guid processId)
        {
            try
            {
                return await _processService.GetProcessHistory(processId);
            }
            catch (Exception e)
            {
                return StatusCode(500);
            }
        }

    }
}