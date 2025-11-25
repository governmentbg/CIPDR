using DataTables.AspNet.Core;
using Google.Protobuf.WellKnownTypes;
using IO.SignTools.Contracts;
using IO.SignTools.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Query;
using OfficeOpenXml.Drawing.Chart;
using URegister.Common;
using URegister.Core.Contracts;
using URegister.Core.Data;
using URegister.Core.Data.Models.Common;
using URegister.Core.Data.Models.Process;
using URegister.Core.Models.Common;
using URegister.Core.Models.Process;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Contracts;
using URegister.Infrastructure.Extensions;
using URegister.Infrastructure.Model.EDelivery;
using URegister.Infrastructure.Model.RegisterForms;
using URegister.Infrastucture.Extensions;
using URegister.IntegrationsCatalog;
using URegister.NomenclaturesCatalog;
using URegister.NumberGenerator;
using URegister.ObjectsCatalog;
using URegister.RegistersCatalog;
using URegister.Users;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static URegister.ObjectsCatalog.ObjectsCatalogGrpc;
using static URegister.Users.AppUserManager;
using Process = URegister.Core.Data.Models.Process.Process;
using RegisterItem = URegister.Core.Data.Models.Process.RegisterItem;

namespace URegister.Core.Services
{
    public class ProcessService : BaseService, IProcessService
    {
        private readonly NumberGenerator.NumberGenerator.NumberGeneratorClient numberGeneratorClient;
        private readonly IRegisterService registerService;
        private readonly RegistersCatalogGrpc.RegistersCatalogGrpcClient registerGrpcClient;
        private readonly IFormConfigurationPersistenceService formConfigurationPersistenceService;
        private readonly NomenclatureGrpc.NomenclatureGrpcClient nomenclatureGrpcClient;
        private readonly IObjectStoreService _objectStoreService;
        private readonly IIOSignToolsService signToolsService;
        private readonly IUserContext _userContext;
        private readonly IConfiguration configuraion;
        private readonly ObjectsCatalogGrpcClient objectsCatalogGrpcClient;
        private readonly AppUserManagerClient appUserManagerClient;
        private readonly IntegrationGrpc.IntegrationGrpcClient integrationGrpcClient;
        private readonly IHttpRequester httpRequester;
        private readonly IServiceService serviceService;

        public ProcessService(
         IApplicationRepository repo,
         NumberGenerator.NumberGenerator.NumberGeneratorClient numberGeneratorClient,
         RegistersCatalogGrpc.RegistersCatalogGrpcClient registerGrpcClient,
         IFormConfigurationPersistenceService formConfigurationPersistenceService,
         IRegisterService registerService,
         NomenclatureGrpc.NomenclatureGrpcClient nomenclatureGrpcClient,
         ILogger<BaseService> logger,
         IObjectStoreService objectStoreService,
         IConfiguration configuraion,
         IIOSignToolsService signToolsService,
         ObjectsCatalogGrpcClient objectsCatalogGrpcClient,
         AppUserManagerClient appUserManagerClient,
         IntegrationGrpc.IntegrationGrpcClient integrationGrpcClient,
         IHttpRequester httpRequester,
         IUserContext userContext,
         IServiceService serviceService
        ) : base(repo, logger)
        {
            this.registerService = registerService;
            this.numberGeneratorClient = numberGeneratorClient;
            this.registerGrpcClient = registerGrpcClient;
            this.formConfigurationPersistenceService = formConfigurationPersistenceService;
            this.nomenclatureGrpcClient = nomenclatureGrpcClient;
            this.configuraion = configuraion;
            this.signToolsService = signToolsService;
            this.objectsCatalogGrpcClient = objectsCatalogGrpcClient;
            this.appUserManagerClient = appUserManagerClient;
            this.integrationGrpcClient = integrationGrpcClient;
            this.httpRequester = httpRequester;
            _objectStoreService = objectStoreService;
            _userContext = userContext;
            this.serviceService = serviceService;
        }


        private async Task SetInstructionResponseReceived(Guid processId)
        {
            var responses = await Repo.All<InstructionResponse>()
                                      .Where(x => x.Instruction.ProcessId == processId &&
                                                  x.ReceivedBy == null)
                                      .ToListAsync();
            foreach (var response in responses)
            {
                response.ReceivedBy = _userContext.UserId;
                response.ReceivedOn = DateTime.UtcNow;
            }
        }
        public async Task<(ProcessStepVM, Process)> AddStep(ProcessStepVM model,
            string targetAdministrationUic = "",
            Guid? eFormRegisteredServiceNumber = null)
        {
            Process process;
            if (model.ProcessId == Guid.Empty)
            {
                var processId = Guid.NewGuid();
                var response = await numberGeneratorClient.GetNumberAsync(new NumberRequest
                {
                    InitialDocumentId = processId.ToString(),
                    Register = (await registerService.GetCurrentRegister()).Code,
                });
                if (response.Status.Code != ResultCodes.Ok)
                {
                    throw new Exception("Проблем при номериране " + response.Status.Message);
                }

                Guid tenantIdGuid = _userContext.AdministrationId;
                if (!string.IsNullOrWhiteSpace(targetAdministrationUic))
                {

                    var registerId = await registerService.GetCurrentRegisterId();
                    AdministrationListRequest administrationListRequest = new AdministrationListRequest()
                    {
                        RegisterId = registerId,
                        DataTableRequest = new DatatableRequest
                        {
                            Start = 0,
                            Length = int.MaxValue
                        }
                    };

                    AdministrationListResponse administrationListResponse = await registerGrpcClient.GetAdministrationListAsync(administrationListRequest);

                    var targetAdministration =
                        administrationListResponse.Data.SingleOrDefault(a => a.Uic == targetAdministrationUic);

                    if (targetAdministration == null)
                    {
                        throw new ArgumentException($"За регистъра не е намерена администрация с идентификатор {targetAdministrationUic}");
                    }

                    tenantIdGuid = Guid.Parse(targetAdministration.AdministrationId);
                }
                Guid? fromProcessId = processId;
                if (model.FromProcessId != null)
                {
                    fromProcessId = await Repo.AllReadonly<Process>()
                                              .Where(x => x.Id == model.FromProcessId)
                                              .Select(x => x.FromProcessId)
                                              .FirstOrDefaultAsync();
                }

                process = new Process
                {
                    Id = processId,
                    FromProcessId = fromProcessId ?? processId,
                    ServiceId = model.ServiceId,
                    IncomingNumber = response.Number.ToString(),
                    IncomingDate = DateTime.UtcNow,
                    TenantId = tenantIdGuid,
                    ReceivedChannelId = model.ProcessInfo.ReceivedChannelId,
                    EFormRegisteredServiceNumber = eFormRegisteredServiceNumber,
                };
                //Ако формата има файлове, в fileMetadata вписваме processId
                await FillProcessIdInFileMetadata(process.Id, model.FormFields);
                await Repo.AddAsync(process);
            }
            else
            {
                process = await Repo.All<Process>()
                                     .Where(x => x.Id == model.ProcessId)
                                     .TagWith(nameof(AddStep))
                                     .FirstAsync();
                await SetInstructionResponseReceived(process.Id);
                process.AssignedToUser = null;
            }
            process.ModifiedByUserId = _userContext.UserId;
            process.PreferredResultDeliveryMethod = model.ProcessInfo.PreferredResultDeliveryMethod;
            process.DeadlineId = model.ProcessInfo.DeadlineId;
            process.OldIncomingDate = model.ProcessInfo.OldIncomingDate?.ConvertToUtcIfUnspecified();
            process.OldIncomingNumber = model.ProcessInfo.OldIncomingNumber;
            //process.DeadlineDay = 
            process.DeadlineDate = DateTime.UtcNow; //TODO: Да се смята срока

            var form = await Repo.All<Form>()
                                 .Where(x => x.ParentId == model.FormParentId)
                                 .FirstAsync();
            var stepData = JsonSerializer.Serialize(model.FormFields);
            var processStep = new ProcessStep
            {
                ProcessId = process.Id,
                ServiceStepId = model.ServiceStepId,
                OrderNum = model.OrderNum,
                StepData = stepData,
                ModifiedByUserId = _userContext.UserId,
                CoordinationStatusId = model.ProcessInfo.CoordinationStatusId,
                CoordinationMotive = model.ProcessInfo.CoordinationMotive,
                ModifiedOn = DateTime.UtcNow,
            };
            process.LastServiceStepId = processStep.ServiceStepId;
            process.FormId = form.Id;


            var serviceStep = await Repo.AllReadonly<ServiceStep>()
                                        .Where(x => x.Id == model.ServiceStepId)
                                        .TagWith(nameof(AddStep))
                                        .FirstAsync();
            process.StatusId = serviceStep.StatusId;
            if (serviceStep.StepId == (int)ServiceSteps.Coordination && processStep.CoordinationStatusId != (int)ProcessStatus.Coordination)
            {
                process.StatusId = processStep.CoordinationStatusId;
                var processSteps = await Repo.AllReadonly<ProcessStep>()
                                             .Where(x => x.ProcessId == process.Id)
                                             .OrderBy(x => x.ModifiedOn)
                                             .ToListAsync();
                if (processSteps.Count >= 2)
                {
                    process.LastServiceStepId = processSteps[0].ServiceStepId;
                    process.AssignedToUser = processSteps[1].ModifiedByUserId;
                }
            }
            await Repo.AddAsync(processStep);
            if (process.StatusId == (int)ProcessStatus.Registered)
            {
                await RegisterStep(processStep, process, model.UserTimeZoneOffsetInMinutes);
            }
            if (process.StatusId == (int)ProcessStatus.Certificate)
            {
                await CertificateStep(processStep, process, model.FileId ?? Guid.Empty);
            }
            await Repo.SaveChangesAsync();

            ProcessStepVM savedInfo = new ProcessStepVM()
            {
                ProcessId = process.Id,
                IncomingNumber = process.IncomingNumber,
                IncomingDate = process.IncomingDate,
                FormId = process.FormId,
            };

            return (savedInfo, process);
        }

        private async Task FillProcessIdInFileMetadata(Guid processId, IEnumerable<FormField> modelFormFields, List<FileMetadata> fileMetadata = null)
        {
            fileMetadata ??= await Repo.All<FileMetadata>()
                    .Where(m => m.ProcessId == null)
                    .Where(m => m.ModifiedByUserId == _userContext.UserId)
                    .Where(m => m.ModifiedOn >= DateTime.UtcNow.AddDays(-1))
                    .TagWith(nameof(FillProcessIdInFileMetadata))
                    .ToListAsync();

            foreach (FormField formField in modelFormFields)
            {
                if (formField.Type == SimpleFormFieldType.File.ToString())
                {
                    if (string.IsNullOrWhiteSpace(formField.Value))//Не е качен файл
                    {
                        continue;
                    }

                    var fieldFileMetadata =
                        fileMetadata.SingleOrDefault(f => f.FileId.ToString() == formField.Value);

                    if (fieldFileMetadata == null)
                    {
                        Logger.LogError($"Не е намерена FileMetadata за файл с идентификатор {formField.Value}");
                        continue;
                    }

                    fieldFileMetadata.ProcessId = processId;
                }
                else
                {
                    if (formField.Fields.Any())
                    {
                        await FillProcessIdInFileMetadata(processId, formField.Fields, fileMetadata);
                    }

                    if (formField.Repetitions.Any())
                    {
                        await FillProcessIdInFileMetadata(processId, formField.Repetitions, fileMetadata);
                    }
                }
            }
        }

        public async Task<IQueryable<Process>> AddPersonIdentifierFilter(IQueryable<Process> query, PersonIdentifierVM personIdentifier, bool isPartida)
        {
            if (!string.IsNullOrEmpty(personIdentifier.Pid))
            {
                var response = await registerGrpcClient.GetMasterPersonRecordIndexAsync(new GetMasterPersonRecordIndexMessage
                {
                    Pid = personIdentifier.Pid,
                    PidType = personIdentifier.PidType
                });
                var ids = new List<Guid>();
                foreach (var item in response.Items)
                {
                    var id = Guid.Empty;
                    Guid.TryParse(item.Id, out id);
                    ids.Add(id);
                }
                if (isPartida)
                {
                    query = query.Where(x => ids.Contains(x.MpriId));
                }
                else
                {
                    query = query.Where(x => ids.Contains(x.MpriApplicantId));
                }
            }
            return query;
        }

        private async Task<List<CodeableConceptPublicResponse>> GetStatusDDL()
        {
            var requestStatus = new NomenclaturePublicRequest();
            requestStatus.NomenclatureTypes.Add(NomenclatureTypes.Status);
            var resultStatus = await nomenclatureGrpcClient.GetNomenclaturePublicAsync(requestStatus);
            return resultStatus.NomenclatureTypes.First().CodeableConcepts.ToList();
        }

        public async Task<IActionResult> GetProcessList(IDataTablesRequest request, ProcessFilterVM filter)
        {
            var queryWhere = Repo.AllReadonly<Process>()
                                 .IgnoreQueryFilters();

            if (filter.FromProcessId != null)
            {
                var process = await Repo.AllReadonly<Process>()
                                        .IgnoreQueryFilters()
                                        .Where(x => x.Id == filter.FromProcessId)
                                        .FirstAsync();
                queryWhere = queryWhere.Where(x => x.FromProcessId == process.FromProcessId);
            }
            else
            {
                queryWhere = queryWhere.Where(x => x.IsActive);
            }

            if (filter.ForDeAssignUser)
            {
                queryWhere = queryWhere.Where(x => x.StatusId != (int)ProcessStatus.Registered &&
                                                   x.StatusId != (int)ProcessStatus.Refused &&
                                                   x.AssignedToUser != null &&
                                                   x.AssignedToUser != Guid.Empty);
            }

            queryWhere = queryWhere.Where(x => x.TenantId == _userContext.AdministrationId);

            if (filter.AssignedToUserId != null)
            {
                queryWhere = queryWhere.Where(x => x.AssignedToUser == filter.AssignedToUserId);
                var status = new int[]{
                    (int)ProcessStatus.Coordination,
                    (int)ProcessStatus.ForCoordination,
                    (int)ProcessStatus.Instruction,
                    (int)ProcessStatus.Send,
                    (int)ProcessStatus.InWork,
                    (int)ProcessStatus.ForCoordination,
                };
                queryWhere = queryWhere.Where(x => status.Contains(x.StatusId));
            }

            if (!string.IsNullOrEmpty(filter.IncomingNumber))
            {
                queryWhere = queryWhere.Where(x => x.IncomingNumber == filter.IncomingNumber);
            }
            if (!string.IsNullOrEmpty(filter.RegisterNumber))
            {
                queryWhere = queryWhere.Where(x => x.RegisterNumber == filter.RegisterNumber);
            }
            //if (!string.IsNullOrEmpty(filter.FromRegisterNumber))
            //{
            //    queryWhere = queryWhere.Where(x => x.RegisterNumber == filter.FromRegisterNumber ||
            //                                       x.FromProcess!.RegisterNumber == filter.FromRegisterNumber);
            //}
            if (filter.IncomingDateFrom != null)
            {
                queryWhere = queryWhere.Where(x => x.IncomingDate >= filter.IncomingDateFrom.Value.ToUniversalTime());
            }
            if (filter.IncomingDateTo != null)
            {
                queryWhere = queryWhere.Where(x => x.IncomingDate <= filter.IncomingDateTo.Value.ToUniversalTime().AddDays(1));
            }
            if (filter.ServiceId > 0)
            {
                queryWhere = queryWhere.Where(x => x.ServiceId == filter.ServiceId);
            }
            if (filter.StatusId > 0)
            {
                queryWhere = queryWhere.Where(x => x.StatusId == filter.StatusId);
            }
            if (filter.StepId > 0)
            {
                queryWhere = queryWhere.Where(x => x.LastServiceStep.StepId == filter.StepId);
            }

            queryWhere = await AddPersonIdentifierFilter(queryWhere, filter.PersonIdentifier, true);
            queryWhere = await AddPersonIdentifierFilter(queryWhere, filter.PersonIdentifierApplicant, false);
            var queryMetaData = Repo.AllReadonly<FileMetadata>()
                                     .Where(x => x.FileSourceTypeId == (int)FileSourceType.Certificate);
            var query = queryWhere.Select(x => new ProcessListItemVM
            {
                Id = x.Id,
                IncomingNumber = x.IncomingNumber,
                IncomingDate = x.IncomingDate,//.ConvertUtcToBGTime(),
                OldIncomingDate = x.OldIncomingDate,
                RegisterNumber = x.RegisterNumber,
                OldIncomingNumber = x.OldIncomingNumber,
                ServiceName = x.Service.Title,
                StepName = x.LastServiceStep.Title,
                StepId = x.LastServiceStepId ?? 0,
                StatusId = x.StatusId,
                ServiceId = x.ServiceId,
                MpriId = x.MpriId,
                MpriApplicantId = x.MpriApplicantId,
                FromName = x.FromProcess!.RegisterNumber,
                RejectionNumber = x.RejectionNumber,
                HasInstruction = x.Instructions.Any(),
                HasCertificate = queryMetaData.Any(m => m.ProcessId == x.Id),
                AssignedToUserId = x.AssignedToUser,
            })
            .TagWith(nameof(GetProcessList));
            var countAll = 0;
            (query, countAll) = request.GetResponseData(query, null, null, true, nameof(Process.IncomingDate), false);
            var data = await query.ToListAsync();
            List<string> ids = new();
            foreach (var item in data)
            {
                if (!ids.Any(x => x == item.MpriId.ToString()))
                {
                    ids.Add(item.MpriId.ToString());
                }
                if (!ids.Any(x => x == item.MpriApplicantId.ToString()))
                {
                    ids.Add(item.MpriApplicantId.ToString());
                }
            }
            var statusDDL = await GetStatusDDL();

            var requestMPRI = new GetMPRIListMessage();
            requestMPRI.IdList.AddRange(ids);
            var responseMPRI = await registerGrpcClient.GetMasterPersonRecordIndexListAsync(requestMPRI);

            var serviceSteps = await Repo.AllReadonly<ServiceStep>()
                                        .ToListAsync();
            var serviceId = await Repo.AllReadonly<Service>()
                                      .Where(x => x.ServiceTypeId == (int)ServiceTypes.Register)
                                      .Select(x => x.Id)
                                      .FirstOrDefaultAsync();

            foreach (var item in data)
            {
                var mpri = responseMPRI.Items.Where(x => x.Id == item.MpriId.ToString()).FirstOrDefault();
                item.Partida = $"{mpri?.Pid} {mpri?.Name}";
                mpri = responseMPRI.Items.Where(x => x.Id == item.MpriApplicantId.ToString()).FirstOrDefault();
                item.Applicant = $"{mpri?.Pid} {mpri?.Name}";
                item.Status = statusDDL.Where(x => x.Code == item.StatusId.ToString()).Select(x => x.Value).FirstOrDefault()
                              + (string.IsNullOrWhiteSpace(item.RejectionNumber) ? String.Empty : $" ({item.RejectionNumber})");
                item.HasNextStep = item.StatusId != (int)ProcessStatus.Registered && item.StatusId != (int)ProcessStatus.Refused && item.StatusId != (int)ProcessStatus.Certificate;
                item.HasClose = item.StatusId != (int)ProcessStatus.Registered &&
                                item.StatusId != (int)ProcessStatus.Refused &&
                                item.StatusId != (int)ProcessStatus.Certificate;
                item.HasDeletion = item.StatusId == (int)ProcessStatus.Registered;
                item.HasChange = item.StatusId == (int)ProcessStatus.Registered;
                if (filter.AssignedToUserId == null || filter.AssignedToUserId != item.AssignedToUserId)
                {
                    // item.HasNextStep = false;
                    // item.HasClose = false;
                }
                if (filter.ForDeAssignUser)
                {
                    item.HasNextStep = false;
                    item.HasDeletion = false;
                    item.HasChange = false;
                    item.HasClose = false;
                    item.HasDeAssignUser = true;
                }
                item.HasDelivery = item.StatusId == (int)ProcessStatus.Refused || item.HasInstruction || item.HasCertificate;
                item.HasDelivery = false;
                item.HasInstruction = item.HasInstruction || ((item.StatusId == (int)ProcessStatus.Send || item.StatusId == (int)ProcessStatus.InWork) && item.ServiceId == serviceId);
                var serviceStep = serviceSteps.Where(x => x.ServiceId == item.ServiceId &&
                                                          x.Id == item.StepId)
                                              .OrderBy(x => x.OrderNum)
                                              .FirstOrDefault();
                var orderNum = serviceStep?.OrderNum ?? 0;
                orderNum = serviceSteps.Where(x => x.ServiceId == item.ServiceId &&
                                                 x.OrderNum > orderNum)
                                       .Min(x => (int?)x.OrderNum) ?? 0;
                item.NextStep = string.Join(' ', serviceSteps.Where(x => x.ServiceId == item.ServiceId &&
                                             x.OrderNum == orderNum)
                                 .Select(x => x.Title)
                                 .ToList());
                //var nextStep = serviceSteps.Where(x => x.ServiceId == item.ServiceId &&
                //                                       x.OrderNum == orderNum)
                //                           .FirstOrDefault();
                //if (nextStep?.StatusId == (int)ProcessStatus.Registered)
                //{
                //}
                if (!_userContext.IsInRole(UserRoles.Editor))
                {
                    item.HasNextStep = false;
                    item.HasClose = false;
                    item.HasInstruction = false;
                }
            }
            return request.GetResponseJson(data.AsQueryable(), countAll);
        }


        public async Task<IActionResult> GetInstructionList(IDataTablesRequest request, InstructionFilterVM filter)
        {
            var queryWhere = Repo.AllReadonly<Instruction>()
                                 .IgnoreQueryFilters()
                                 .Where(x => x.IsActive)
                                 .Where(x => x.ProcessId == filter.ProcessId);

            var query = queryWhere.Select(x => new InstructionVM
            {
                Id = x.Id,
                Content = x.Content,
                InstructionDate = x.ModifiedOn,//.ConvertUtcToBGTime(),
                UserId = x.ModifiedByUserId,
                HasResponse = x.InstructionResponses.Any(),
                CanAdd = x.ClosedOn == null,
            })
            .TagWith(nameof(GetInstructionList));
            var countAll = 0;
            (query, countAll) = request.GetResponseData(query);
            var data = await query.ToListAsync();

            var usersGuids = data.Select(item => item.UserId.ToString()).Distinct().ToList();
            if (usersGuids.Any())
            {
                var requestUsers = new UserGuidsRequest
                {
                    UserGuids = { usersGuids }
                };
                var resultUsers = await appUserManagerClient.GetUserNamesByGuidsAsync(requestUsers);

                // Map user names to audit log items
                var userNameDict = resultUsers.UserNamesByGuid.ToDictionary(
                    u => u.Guid,
                    u => string.Join(" ", new[] { u.FirstName, u.MiddleName, u.LastName }.Where(s => !string.IsNullOrEmpty(s)).Select(s => s.Trim())).Trim()
                );

                foreach (var item in data)
                {
                    item.UserName = userNameDict.TryGetValue(item.UserId.ToString(), out var userFullName) ? userFullName : string.Empty;
                }
            }

            return request.GetResponseJson(data.AsQueryable(), countAll);
        }

        private async Task SetViewModelFrom(ProcessStep? processStep, FormViewModel formModel)
        {
            if (processStep != null)
            {

                List<RegisterItem> registryItemsOfTheStep = await Repo.AllReadonly<RegisterItem>()
                    .Where(r => r.ProcessStepId == processStep.Id)
                    .TagWith(nameof(SetViewModelFrom))
                    .ToListAsync();

                if (registryItemsOfTheStep.Any())
                {
                    formConfigurationPersistenceService.DistributeRegisterItemValuesToFormViewModel(
                        registryItemsOfTheStep, formModel);
                }
                else
                {
                    var formFields = JsonSerializer.Deserialize<List<FormField>>(processStep.StepData)!;
                    var formFieldsFrom = formModel.FormFields.ToList();
                    formModel.FormFields.Clear();
                    foreach (var formFieldFrom in formFieldsFrom)
                    {
                        var formField = formFields.Where(x => x.Identifier == formFieldFrom.Identifier).FirstOrDefault();
                        if (formField == null)
                        {
                            formModel.FormFields.Add(formFieldFrom);
                        }
                        else
                        {
                            formModel.FormFields.Add(formField);
                        }
                    }
                }
            }
        }

        public async Task<(ProcessStepVM, Process)> GetFormViewModel(Guid processId, bool preview)
        {
            var process = await Repo.AllReadonly<Process>()
                        .IgnoreQueryFilters()
                        .Include(x => x.ProcessSteps)
                        .Include(x => x.Form)
                        .Include(x => x.LastServiceStep)
                        .Where(x => x.Id == processId)
                        .TagWith(nameof(GetFormViewModel))
                        .FirstAsync();
            ServiceStep serviceStep;
            if (preview)
            {
                serviceStep = process.LastServiceStep;
            }
            else
            {
                var orderNum = await Repo.AllReadonly<ServiceStep>()
                                            .Where(x => x.ServiceId == process.ServiceId &&
                                                        x.OrderNum > process.LastServiceStep.OrderNum)
                                            .MinAsync(x => (int?)x.OrderNum) ?? 0;
                serviceStep = await Repo.AllReadonly<ServiceStep>()
                                        .Where(x => x.ServiceId == process.ServiceId &&
                                                    x.OrderNum == orderNum)
                                        .FirstAsync();
            }
            var service = await Repo.AllReadonly<Service>()
                                    .Where(x => x.Id == process.ServiceId)
                                    .FirstAsync();
            var formModel = await formConfigurationPersistenceService.GetFormViewModelByFormId(process.FormId);
            var processStep = await Repo.AllReadonly<ProcessStep>()
                                        .IgnoreQueryFilters()
                                        .Where(x => x.ProcessId == processId)
                                        .OrderByDescending(x => x.ServiceStep.OrderNum)
                                        .TagWith(nameof(GetFormViewModel))
                                        .FirstOrDefaultAsync();
            await SetViewModelFrom(processStep, formModel);
            var processStepVM = await ToProcessStepVM(processId, null, service.Id, serviceStep.Id, serviceStep.OrderNum, process?.OldIncomingNumber, process?.OldIncomingDate, formModel, preview);
            return (processStepVM, process);
        }

        public async Task<ProcessStepVM> GetFormViewModelFrom(Guid fromProcessId, int serviceId)
        {
            var service = await Repo.AllReadonly<Service>()
                                   .Include(x => x.ServiceSteps)
                                   .Where(x => x.Id == serviceId)
                                   .FirstOrDefaultAsync();
            var serviceStep = service.ServiceSteps.OrderBy(x => x.OrderNum).FirstOrDefault();
            var formModel = await formConfigurationPersistenceService.GetFormViewModel(service.FormParentId);
            var processStep = await Repo.AllReadonly<ProcessStep>()
                                        .IgnoreQueryFilters()
                                        .Where(x => x.ProcessId == fromProcessId)
                                        .OrderByDescending(x => x.ServiceStep.OrderNum)
                                        .TagWith(nameof(GetFormViewModel))
                                        .FirstOrDefaultAsync();
            await SetViewModelFrom(processStep, formModel);
            return await ToProcessStepVM(Guid.Empty, fromProcessId, serviceId, serviceStep.Id, serviceStep.OrderNum, null, null, formModel, false);
        }
        public async Task<ProcessStepVM> GetFormViewModel(int serviceId, string? OldIncomingNumber, DateTime? OldIncomingDate, bool isOld)
        {
            var service = await Repo.AllReadonly<Service>()
                                    .Include(x => x.ServiceSteps)
                                    .Where(x => x.Id == serviceId)
                                    .FirstOrDefaultAsync();
            var serviceStep = service.ServiceSteps.OrderBy(x => x.OrderNum).FirstOrDefault();
            if (isOld)
            {
                serviceStep = service.ServiceSteps.OrderBy(x => x.OrderNum).LastOrDefault();
            }
            var formModel = await formConfigurationPersistenceService.GetFormViewModel(service.FormParentId);
            return await ToProcessStepVM(Guid.Empty, null, serviceId, serviceStep.Id, serviceStep.OrderNum, OldIncomingNumber, OldIncomingDate, formModel, false);
        }

        private FormField? FindMasterPerson(List<FormField> formFields, PersonRole roleId)
        {
            foreach (var formField in formFields)
            {
                if (roleId == PersonRole.Partida && formField.IsBatchOwner)
                {
                    return formField;
                }
                if (roleId == PersonRole.Applicant && formField.IsSubmitter)
                {
                    return formField;
                }
                if (formField.Fields?.Any() == true)
                {
                    var result = FindMasterPerson(formField.Fields, roleId);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            return null;
        }

        private string[] ParsePidFieldValue(string value)
        {
            if (!value.Contains(':'))
            {
                string errorMessage =
                    $"Стойността {value} на идентификатор не е в правилния формат 'тип:идентификатор'";
                Logger.LogError(errorMessage);

                //TODO : Позволяваме засега празна партида. Редно е да хвърляме грешка
                return ":".Split(":");
            }

            return value.Split(":");
        }

        private string GetPidFieldValue(string[] values, int index)
        {
            if (index < values.Length)
                return values[index];

            Logger.LogError($"Грешни параметри в {nameof(GetPidFieldValue)}. Values: {string.Join(';', values)}, index: {index}");
            throw new ArgumentException(
                $"Грешни параметри в {nameof(GetPidFieldValue)}. Values: {string.Join(';', values)}, index: {index}");
        }

        /// <summary>
        /// Връща данни за собственик на партида
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="formFields"></param>
        /// <returns>Тип идентификатор, Идентификатор, Наименование</returns>
        /// <exception cref="Exception"></exception>
        public (string?, string?, string?) GetMPRIData(PersonRole roleId, List<FormField> formFields)
        {
            //TODO: да се премахте след установяване на проблема
            //Logger.LogInformation($"Състояние на полета преди извличане на партида за роля {roleId.GetDescription()}: " + JsonSerializer.Serialize(formFields));

            var field = FindMasterPerson(formFields, roleId);
            if (field == null)
            {
                Logger.LogError($"Не е намерено поле Партида/Заявител, за роля {roleId.GetDescription()}");
                throw new Exception("Не е намерено поле Партида/Заявител");
            }

            //Logger.LogInformation($"Сложно поле на партида за роля {roleId.GetDescription()}: " + JsonSerializer.Serialize(field));

            var pidType = string.Empty;
            var pid = string.Empty;
            var name = string.Empty;

            bool mprEntityIsCompany = false;
            bool mprEntityIsPerson = false;

            if(field.Type is nameof(SimpleFormFieldType.MPREntity))
            {
                var companyIpField = field.Fields!
                    .FirstOrDefault(x => x.Name.EndsWith(ComplexFieldsNameConstants.CompanyNumberImmutable));

                if (companyIpField != null && !string.IsNullOrWhiteSpace(companyIpField.Value))
                {
                    mprEntityIsCompany = true;
                }
                else
                {
                    //NOTE : позволяваме засега празни партиди
                    mprEntityIsPerson = true;
                }
            }

            if ((field.Type is nameof(SimpleFormFieldType.Company) or nameof(SimpleFormFieldType.CompanyWithAddress)) ||
                mprEntityIsCompany)
            {
                var simpleMPRField = field.Fields!
                    .FirstOrDefault(x => x.Name.EndsWith(ComplexFieldsNameConstants.CompanyNumberImmutable));

                if (simpleMPRField == null)
                {
                    Logger.LogError($"Не е намерено просто подполе за Партида/Заявител, за роля {roleId.GetDescription()}, с име завършващо на {ComplexFieldsNameConstants.CompanyNumberImmutable}");
                }

                //Logger.LogInformation($"Просто подполе на партида за роля {roleId.GetDescription()}: " + JsonSerializer.Serialize(simpleMPRField));
                pidType = GetMPRIForCompany(simpleMPRField, field, out pid, out name);
            }
            else if ((field.Type is nameof(SimpleFormFieldType.Person) or nameof(SimpleFormFieldType.namePosition)) 
                     || mprEntityIsPerson)
            {
                var simpleMPRField = field.Fields!
                    .FirstOrDefault(x => x.Type == nameof(SimpleFormFieldType.PersonIdentifier))!;

                if (simpleMPRField == null)
                {
                    Logger.LogError($"Не е намерено просто подполе за Партида/Заявител, за роля {roleId.GetDescription()}, от тип {nameof(SimpleFormFieldType.PersonIdentifier)}");
                }

                //Logger.LogInformation($"Просто подполе на партида за роля {roleId.GetDescription()}: " + JsonSerializer.Serialize(simpleMPRField));
                pidType = GetMPRIForPerson(simpleMPRField, field, out pid, out name);

            }
            else if (field.Type == nameof(SimpleFormFieldType.authorizedOfficial))
            {
                var simpleMPRField = field.Fields!
                    .FirstOrDefault(x => x.Name.EndsWith(ComplexFieldsNameConstants.CompanyNumberImmutable));

                if (simpleMPRField != null && string.IsNullOrWhiteSpace(simpleMPRField.Value))
                {
                    pidType = GetMPRIForCompany(simpleMPRField, field, out pid, out name);
                }
                else
                {
                    simpleMPRField = field.Fields!
                        .FirstOrDefault(x => x.Type == nameof(SimpleFormFieldType.PersonIdentifier))!;

                    if (simpleMPRField != null && string.IsNullOrWhiteSpace(simpleMPRField.Value))
                    {
                        pidType = GetMPRIForPerson(simpleMPRField, field, out pid, out name);
                    }
                    else
                    {
                        Logger.LogError($"Не е намерено просто подполе за Партида/Заявител, за роля {roleId.GetDescription()}, за поле на {field.Name}");
                    }
                }
            }
            else
            {
                Logger.LogError($"Не е извлечен Партида/Заявител от поле {field.Name} тип {field.Type}, за роля {roleId.GetDescription()}");
            }
                
            return (pidType, pid, name);
        }

        private string GetMPRIForPerson(FormField? simpleMPRField, FormField field, out string pid, out string name)
        {
            string pidType;
            var pidValues = ParsePidFieldValue(simpleMPRField.Value);

            //Logger.LogInformation($"Стойности на просто подполе на партида за роля {roleId.GetDescription()}: " + JsonSerializer.Serialize(pidValues));

            pidType = GetPidFieldValue(pidValues, 0);
            pid = GetPidFieldValue(pidValues, 1);
            var firstName = field.Fields!.Where(x => x.Name.EndsWith(ComplexFieldsNameConstants.FirstNameImmutable)).Select(x => x.Value).FirstOrDefault();
            var middleName = field.Fields!.Where(x => x.Name.EndsWith(ComplexFieldsNameConstants.MiddleNameImmutable)).Select(x => x.Value).FirstOrDefault();
            var lastName = field.Fields!.Where(x => x.Name.EndsWith(ComplexFieldsNameConstants.LastNameImmutable)).Select(x => x.Value).FirstOrDefault();
            name = firstName ?? string.Empty;
            if (!string.IsNullOrEmpty(middleName))
                name += $" {middleName}";
            if (!string.IsNullOrEmpty(lastName))
                name += $" {lastName}";
            return pidType;
        }

        private string GetMPRIForCompany(FormField? simpleMPRField, FormField field, out string pid, out string? name)
        {
            string pidType;
            var pidValues = ParsePidFieldValue(simpleMPRField.Value);

            //Logger.LogInformation($"Стойности на просто подполе на партида за роля {roleId.GetDescription()}: " + JsonSerializer.Serialize(pidValues));

            pidType = GetPidFieldValue(pidValues, 0);
            pid = GetPidFieldValue(pidValues, 1);
            name = field.Fields!.Where(x => x.Name.EndsWith(ComplexFieldsNameConstants.CompanyNameImmutable))
                .Select(x => x.Value)
                .FirstOrDefault();
            return pidType;
        }

        /// <summary>
        /// Връща историята на заявена услуга
        /// </summary>
        /// <param name="processId">Идентификатор на процесс</param>
        /// <returns></returns>
        public async Task<IActionResult> GetProcessHistory(Guid processId)
        {
            try
            {
                bool historyNotPublic = (await registerService.GetCurrentRegister()).HistoryNotPublic;

                if (historyNotPublic)
                {
                    Logger.LogError($"Неоторизиран опит за достъп до не публична история в {nameof(GetProcessHistory)}");
                    throw new AccessViolationException(
                        $"Неоторизиран опит за достъп до не публична история в {nameof(GetProcessHistory)}");
                }

                var process = await Repo.AllReadonly<Process>()
                    .IgnoreQueryFilters()
                    .Where(x => x.Id == processId)
                    .FirstAsync();

                var queryWhere = Repo.AllReadonly<Process>()
                    .TagWith(nameof(GetProcessHistory))
                    .IgnoreQueryFilters()
                    .Where(p => p.FromProcessId == process.FromProcessId);

                var query = queryWhere.Select(x => new ProcessListItemVM
                {
                    Id = x.Id,
                    IncomingNumber = x.IncomingNumber,
                    IncomingDate = x.IncomingDate.ConvertUtcToBGTime(),
                    RegisterNumber = x.RegisterNumber,
                    ServiceName = x.Service.Title,
                    StepName = x.LastServiceStep.Title,
                    StepId = x.LastServiceStepId ?? 0,
                    StatusId = x.StatusId,
                    ServiceId = x.ServiceId,
                    MpriId = x.MpriId,
                    MpriApplicantId = x.MpriApplicantId,
                    FromName = x.FromProcess!.RegisterNumber,
                    RejectionNumber = x.RejectionNumber
                });

                var data = await query.ToListAsync();
                List<string> ids = new();
                foreach (var item in data)
                {
                    if (!ids.Any(x => x == item.MpriId.ToString()))
                    {
                        ids.Add(item.MpriId.ToString());
                    }

                    if (!ids.Any(x => x == item.MpriApplicantId.ToString()))
                    {
                        ids.Add(item.MpriApplicantId.ToString());
                    }
                }

                var statusDDL = await GetStatusDDL();

                var requestMPRI = new GetMPRIListMessage();
                requestMPRI.IdList.AddRange(ids);
                var responseMPRI = await registerGrpcClient.GetMasterPersonRecordIndexListAsync(requestMPRI);

                var serviceSteps = await Repo.AllReadonly<ServiceStep>()
                    .ToListAsync();

                foreach (var item in data)
                {
                    var mpri = responseMPRI.Items.Where(x => x.Id == item.MpriId.ToString()).FirstOrDefault();
                    item.Partida = $"{mpri?.Pid} {mpri?.Name}";
                    mpri = responseMPRI.Items.Where(x => x.Id == item.MpriApplicantId.ToString()).FirstOrDefault();
                    item.Applicant = $"{mpri?.Pid} {mpri?.Name}";
                    item.Status = statusDDL.Where(x => x.Code == item.StatusId.ToString()).Select(x => x.Value)
                                      .FirstOrDefault()
                                  + (string.IsNullOrWhiteSpace(item.RejectionNumber)
                                      ? String.Empty
                                      : $" ({item.RejectionNumber})");

                }

                var columnData = new List<dynamic>();

                columnData.Add(new
                { label = "Входящ номер", fieldName = Decapitalize(nameof(ProcessListItemVM.IncomingNumber)) });
                columnData.Add(new
                {
                    label = "Номер на вписване",
                    fieldName = Decapitalize(nameof(ProcessListItemVM.RegisterNumber))
                });
                columnData.Add(
                    new { label = "Услуга", fieldName = Decapitalize(nameof(ProcessListItemVM.ServiceName)) });
                columnData.Add(new
                { label = "Изпълнена стъпка", fieldName = Decapitalize(nameof(ProcessListItemVM.StepName)) });
                columnData.Add(new { label = "Статус", fieldName = Decapitalize(nameof(ProcessListItemVM.Status)) });
                columnData.Add(new
                { label = "Дата на вписване", fieldName = Decapitalize(nameof(ProcessListItemVM.IncomingDate)) });

                var combinedData = new
                {
                    columnData = columnData,
                    data = data,
                };

                return new JsonResult(combinedData);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Проблем при извличане на история на процес с идентификатор {processId} в {nameof(GetProcessHistory)}");
                throw e;
            }
        }

        private static string Decapitalize(string input)
        {
            if (string.IsNullOrEmpty(input) || char.IsLower(input[0]))
                return input;

            return char.ToLower(input[0]) + input.Substring(1);
        }

        public async Task<Guid?> AddMPRI(PersonRole roleId, List<FormField> formFields)
        {
            var register = await registerService.GetCurrentRegister();
            if (roleId == PersonRole.Applicant && register.TypeEntry != RegisterTypeEntry.Applicant)
            {
                return null;
            }
            (var pidType, var pid, var name) = GetMPRIData(roleId, formFields);
            var responseMpriAdd = await registerGrpcClient.AddMasterPersonRecordIndexAsync(new MasterPersonRecordIndexAddMessage
            {
                PidType = pidType,
                Pid = pid,
                Name = name,
                RegisterId = register.Id,
                RoleId = (int)roleId
            });

            if (responseMpriAdd.Status.Code != ResultCodes.Ok)
            {
                Logger.LogError($"Проблем при запис на партида. Pid: {pid}, PidType {pidType}. {responseMpriAdd.Status.Message}");
                throw new ArgumentException($"Проблем при запис на партида. Pid: {pid}, PidType {pidType}. {responseMpriAdd.Status.Message}");
            }

            if (Guid.TryParse(responseMpriAdd.Id, out Guid result))
            {
                return result;
            }
            else
            {
                Logger.LogError($"Не може да парсне GUID {responseMpriAdd.Id}, за индетификатор {pid}");
                throw new ArgumentException($"Не може да парсне GUID {responseMpriAdd.Id}, за индетификатор {pid}");
            }
        }

        /// <summary>
        /// Връща заявена услуга за вписване по списък от полета от форма
        /// </summary>
        /// <param name="formFields">Списък от полета от форма</param>
        /// <returns></returns>
        public async Task<Process?> GetProcessForCertificate(List<FormField> formFields)
        {
            (var pidType, var pid, var name) = GetMPRIData(PersonRole.Partida, formFields);
            MPRIListMessage response = await registerGrpcClient.GetMasterPersonRecordIndexAsync(new GetMasterPersonRecordIndexMessage { Pid = pid, PidType = pidType });
            if (!response.Items.Any())
                return null;
            var mpriId = Guid.Parse(response.Items.First().Id);
            return await Repo.AllReadonly<Process>()
                             .Include(x => x.RegisterItems)
                             .Where(x => x.MpriId == mpriId &&
                                         x.Service.ServiceTypeId == (int)ServiceTypes.Register
                             )
                             .FirstOrDefaultAsync();
        }
        public async Task<Process?> GetProcessForCertificateOnRegister(Guid processId)
        {
            return await Repo.AllReadonly<Process>()
                             .IgnoreQueryFilters()
                             .Include(x => x.RegisterItems)
                             .Where(x => x.Id == processId)
                             .FirstOrDefaultAsync();
        }


        private async Task RegisterStep(ProcessStep processStep, Process process, int userTimeZoneOffsetInMinutes)
        {
            var formFields = JsonSerializer.Deserialize<List<FormField>>(processStep.StepData)!;
            process.MpriId = (await AddMPRI(PersonRole.Partida, formFields)) ?? Guid.Empty;
            process.MpriApplicantId = (await AddMPRI(PersonRole.Applicant, formFields)) ?? Guid.Empty;
            process.RegisteredStepId = processStep.Id;
            if (process.Id == process.FromProcessId)
            {
                var response = await numberGeneratorClient.GetNumberAsync(new NumberRequest
                {
                    InitialDocumentId = process.Id.ToString(),
                    Register = (await registerService.GetCurrentRegister()).Code,
                });
                if (response.Status.Code != ResultCodes.Ok)
                {
                    throw new Exception("Проблем при номериране " + response.Status.Message);
                }
                process.RegisterNumber = response.Number.ToString();
            }
            else
            {
                var processes = await Repo.All<Process>()
                                          .IgnoreQueryFilters()
                                          .TagWith(nameof(RegisterStep))
                                          .Where(x => x.FromProcessId == process.FromProcessId &&
                                                      x.Id != process.Id)
                                          .ToListAsync();
                process.RegisterNumber = processes.First().RegisterNumber;
                processes.ForEach(x => x.IsActive = false);
            }
            var registerItems = await AddRegisterItems(process, formFields, processStep.Id, userTimeZoneOffsetInMinutes);
            foreach (var registerItem in registerItems)
            {
                await Repo.AddAsync(registerItem);
            }
        }

        private async Task CertificateStep(ProcessStep processStep, Process process, Guid fileId)
        {
            var formFields = JsonSerializer.Deserialize<List<FormField>>(processStep.StepData)!;
            process.MpriId = (await AddMPRI(PersonRole.Partida, formFields)) ?? Guid.Empty;
            process.MpriApplicantId = (await AddMPRI(PersonRole.Applicant, formFields)) ?? Guid.Empty;
            process.RegisteredStepId = processStep.Id;
            if (process.Id == process.FromProcessId)
            {
                var response = await numberGeneratorClient.GetNumberAsync(new NumberRequest
                {
                    InitialDocumentId = process.Id.ToString(),
                    Register = (await registerService.GetCurrentRegister()).Code,
                });
                if (response.Status.Code != ResultCodes.Ok)
                {
                    throw new Exception("Проблем при номериране " + response.Status.Message);
                }
                process.RegisterNumber = response.Number.ToString();
            }
            else
            {
                var processes = await Repo.All<Process>()
                                          .TagWith(nameof(RegisterStep))
                                          .Where(x => x.FromProcessId == process.FromProcessId &&
                                                      x.Id != process.Id)
                                          .ToListAsync();
                process.RegisterNumber = processes.First().RegisterNumber;
                processes.ForEach(x => x.IsActive = false);
            }
        }
        public FormField GetFormField(string fieldName, List<FormField> formFields)
        {
            foreach (var formField in formFields)
            {
                if (fieldName == formField.Name)
                {
                    return formField;
                }
                if (formField.Fields?.Any() == true)
                {
                    var aFormField = GetFormField(fieldName, formField.Fields);
                    if (aFormField != null)
                    {
                        return aFormField;
                    }
                }
                if (formField.Repetitions?.Any() == true)
                {
                    var aFormField = GetFormField(fieldName, formField.Repetitions);
                    if (aFormField != null)
                    {
                        return aFormField;
                    }
                }
            }
            return null;
        }
        public async Task<List<RegisterItem>> AddRegisterItems(Process process, List<FormField> formFields, Guid processStepId, int userTimeZoneOffsetInMinutes)
        {
            CatalogFieldsListRequest request = new CatalogFieldsListRequest();
            var response = await objectsCatalogGrpcClient.GetFieldsListAsync(request);
            var fieldTypes = response.FieldTypes.ToList();
            var registerItems = new List<RegisterItem>();
            foreach (var formField in formFields)
            {
                registerItems.AddRange(await AddRegisterItem(process, formField, formField.Identifier, 0, processStepId, userTimeZoneOffsetInMinutes, fieldTypes));
                if (formField.Repetitions?.Any() == true)
                {
                    foreach (var clonedField in formField.Repetitions!)
                    {
                        int index = -1;
                        var pos = clonedField.Name.IndexOf('#');
                        var indexStr = clonedField.Name.Substring(pos + 1);
                        index = int.Parse(indexStr);
                        if (clonedField.Identifier == Guid.Empty)
                        {
                            clonedField.Identifier = formField.Identifier;
                        }
                        if (clonedField.Fields?.Any() == true)
                        {
                            foreach (var subFormField in clonedField.Fields!.Where(x => x.Identifier == Guid.Empty))
                            {
                                var subField0 = formField.Fields?.Where(x => x.Name == subFormField.Name.Replace($"#{index}", string.Empty)).FirstOrDefault();
                                subFormField.Identifier = subField0?.Identifier ?? Guid.Empty;
                            }
                        }
                        registerItems.AddRange(await AddRegisterItem(process, clonedField, formField.Identifier, index, processStepId, userTimeZoneOffsetInMinutes, fieldTypes));
                    }
                }
            }
            await formConfigurationPersistenceService.ResolveFormFieldsViewModelValues(formFields);
            foreach (var registerItem in registerItems)
            {
                var aFormField = GetFormField(registerItem.Name, formFields);
                registerItem.ClValue = aFormField.Value;
            }
            formFields.Clear();
            return registerItems;
        }
        private async Task<List<RegisterItem>> AddRegisterItem(
            Process process,
            FormField formField,
            Guid parentId,
            int index,
            Guid processStepId,
            int userTimeZoneOffsetInMinutes,
            List<CatalogFieldType> fieldTypes)
        {
            var registerItems = new List<RegisterItem>();
            var fieldTypeId = fieldTypes.Where(x => x.Type == formField.Type)
                                        .FirstOrDefault()?.FieldTypeId ?? 0;
            var registerItem = new RegisterItem
            {
                TenantId = process.TenantId,
                Name = formField.Name,
                FieldId = formField.Identifier,
                ParentFieldId = parentId,
                Index = index,
                IsComplex = formField.Fields?.Any() == true,
                ProcessId = process.Id,
                Value = formField.Value,
                IsPublic = formField.IsPublic,
                MpriId = process.MpriId,
                RegisterNumber = process.RegisterNumber!,
                ProcessStepId = processStepId,
                FieldTypeId = fieldTypeId
            };
            registerItems.Add(registerItem);
            if (!string.IsNullOrWhiteSpace(formField.NomenclatureType))
            {
                registerItem.NomenclatureType = formField.NomenclatureType;
            }
            else if (formField.NomenclatureType == SimpleFormFieldType.City.ToString())
            {
                registerItem.NomenclatureType = NomenclatureTypes.Ekatte;
            }
            registerItem.Label = formField.Label;
            registerItem.ClValue = formField.Label;
            AddTypifiedValuesToRegisterItem(registerItem, formField, userTimeZoneOffsetInMinutes);

            foreach (FormField subFormField in formField.Fields!)
            {
                registerItems.AddRange(await AddRegisterItem(process, subFormField, parentId, index, processStepId, userTimeZoneOffsetInMinutes, fieldTypes));
            }
            return registerItems;
        }

        private void AddTypifiedValuesToRegisterItem(RegisterItem registerItem, FormField formField,
            int userTimeZoneOffsetInMinutes)
        {
            if (string.IsNullOrWhiteSpace(formField.Value))
            {
                return;
            }

            //Очакваме че стойностите са минали валидация и парсваме без проверки

            switch ((SimpleFormFieldType)(registerItem.FieldTypeId))
            {
                case SimpleFormFieldType.Boolean:
                    registerItem.BoolValue = bool.Parse(formField.Value);
                    return;
                case SimpleFormFieldType.Number:
                    string numberWithDotSeparator = formField.Value.Replace(',', '.');
                    registerItem.DecimalValue = decimal.Parse(numberWithDotSeparator, CultureInfo.InvariantCulture);
                    return;
                case SimpleFormFieldType.BulgarianCurrency:
                    //Записваме винаги в евро
                    if (string.IsNullOrWhiteSpace(formField.Value))
                    {
                        return;
                    }
                    var valuesArray = formField.Value.Split(':');

                    if (int.Parse(valuesArray[0]) == (int)Currency.EUR)
                    {
                        registerItem.DecimalValue = decimal.Parse(valuesArray[1], CultureInfo.InvariantCulture);
                        return;
                    }

                    if (int.Parse(valuesArray[0]) == (int)Currency.BGN)
                    {                    
                        var parsedDecimalValue = decimal.Parse(valuesArray[1], CultureInfo.InvariantCulture);
                        registerItem.DecimalValue = parsedDecimalValue / ValueConstants.EURInBGN;
                    }
                    return;
                case SimpleFormFieldType.Date:
                    DateTime parsedDate = DateTime.ParseExact(registerItem.Value!,
                        FormattingConstant.NormalDateFormat,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None);

                    DateTime parsedDateInUtc = parsedDate.AddMinutes(userTimeZoneOffsetInMinutes);
                    registerItem.DateTimeValue = DateTime.SpecifyKind(parsedDateInUtc, DateTimeKind.Utc);
                    return;
                case SimpleFormFieldType.DateTime:
                    DateTime parsedDateTime = DateTime.ParseExact(registerItem.Value!,
                        FormattingConstant.DateTimeFormat,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None);

                    DateTime parsedDateTimeInUtc = parsedDateTime.AddMinutes(userTimeZoneOffsetInMinutes);
                    registerItem.DateTimeValue = DateTime.SpecifyKind(parsedDateTimeInUtc, DateTimeKind.Utc);
                    return;
            }
        }

        /// <summary>
        /// Записва файл в хранилището
        /// </summary>
        /// <param name="file">Файлът за запис</param>
        /// <param name="key">Ключ на файла при презапис</param>
        /// <param name="eformId">Идентификатор на е-форма, ако файлът е заявление подадено чрез е-форма</param>
        /// <param name="eformDateOfFill">Дата на попълване на е-формата, ако файлът е заявление подадено чрез е-форма</param>
        /// <returns>Ключа на записания файл</returns>
        public async Task<SaveOperationResult> SaveUploadedFile(IFormFile file, Guid key, Guid? eformId = null, DateTime? eformDateOfFill = null)
        {
            try
            {
                byte[] filesAsBytes = [];
                using MemoryStream ms = new MemoryStream();
                await file.CopyToAsync(ms);
                ms.Position = 0; // Връщаме MemoryStream на положение 0 иначе гърми с грешка OException: PDF header not found 
                filesAsBytes = ms.ToArray();
                string fileKey;
                if (Guid.Empty == key)
                {
                    fileKey = await _objectStoreService.SaveObject(file.FileName, filesAsBytes, file.ContentType, null);
                }
                else
                {
                    fileKey = await _objectStoreService.SaveObject(file.FileName, filesAsBytes, key.ToString(), file.ContentType, null);
                }

                Guid fileId = Guid.Parse(fileKey);

                FileMetadata existingFileMetadata =
                    await Repo.All<FileMetadata>().Where(f => f.FileId == fileId).SingleOrDefaultAsync();

                if (existingFileMetadata != null)
                {
                    existingFileMetadata.FileName = file.FileName;
                    existingFileMetadata.ModifiedByUserId = _userContext.UserId;
                    existingFileMetadata.FileId = fileId;
                    existingFileMetadata.ModifiedOn = DateTime.UtcNow;
                    existingFileMetadata.EFormDateOfFill = eformDateOfFill;
                }
                else
                {
                    FileMetadata newFileMetadata = new FileMetadata()
                    {
                        FileName = file.FileName,
                        ModifiedByUserId = _userContext.UserId,
                        FileId = fileId,
                        ModifiedOn = DateTime.UtcNow,
                        EFormId = eformId,
                        EFormDateOfFill = eformDateOfFill
                        //todo:
                    };
                    await Repo.AddAsync(newFileMetadata);
                }

                await Repo.SaveChangesAsync();
                return new SaveOperationResult(true, fileId);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Проблем при качване на файл в {nameof(SaveUploadedFile)}, за файл {file.FileName}");
                return new SaveOperationResult(MessageConstant.Values.FileUploadFailed);
            }
        }

        /// <summary>
        /// Премахва качен файл по ключ на файла
        /// </summary>
        /// <param name="key">Ключ на файла</param>
        /// <returns></returns>
        public async Task<OperationResult> DeleteFile(string key)
        {
            var keyAsGuid = Guid.Parse(key);
            FileMetadata fileMetadata = await Repo.All<FileMetadata>()
                .TagWith(nameof(DeleteFile))
                .SingleOrDefaultAsync(f => f.FileId == keyAsGuid);

            if (fileMetadata == null)
            {
                Logger.LogError($"Не е намерена FileMetadata за файлов ключ {key} в {nameof(DeleteFile)}");
            }
            else
            {
                fileMetadata.ModifiedByUserId = _userContext.UserId;
                await Repo.DeleteAsync<FileMetadata>(fileMetadata.Id);
            }

            bool success = await _objectStoreService.DeleteObject(key);

            if (!success)
            {
                Logger.LogError($"Неуспешно изтриване на файл с ключ {key} в {nameof(DeleteFile)}");
                return new OperationResult(MessageConstant.Values.DeleteFailed);
            }

            await Repo.SaveChangesAsync();
            return new OperationResult();
        }

        public async Task<ProcessStepVM> ToProcessStepVM(
            Guid processId,
            Guid? fromProcessId,
            int serviceId,
            int serviceStepId,
            int orderNum,
            string? oldIncomingNumber,
            DateTime? oldIncomingDate,
            FormViewModel formModel,
            bool preview)
        {
            var process = await Repo.AllReadonly<Process>()
                                    .Where(x => x.Id == processId)
                                    .FirstOrDefaultAsync();
            var result = new ProcessStepVM
            {
                ProcessId = processId,
                FromProcessId = fromProcessId,
                ServiceId = serviceId,
                ServiceStepId = serviceStepId,
                FormFields = formModel.FormFields,
                FormParentId = formModel.FormParentId,
                FormTitle = formModel.FormTitle,
                Purpose = formModel.Purpose,
                SelectedType = formModel.SelectedType,
                OrderNum = orderNum,
                ProcessInfo = new ProcessInfoVM(),
                ConditionTree = formModel.ConditionTree
            };
            var serviceStep = await Repo.AllReadonly<ServiceStep>()
                                        .Where(x => x.Id == serviceStepId)
                                        .FirstAsync();
            if (process != null)
            {
                result.ProcessInfo = new ProcessInfoVM
                {
                    ReceivedChannelId = process.ReceivedChannelId,
                    PreferredResultDeliveryMethod = process.PreferredResultDeliveryMethod,
                    DeadlineDate = process.DeadlineDate,
                    DeadlineDay = process.DeadlineDay,
                    DeadlineId = process.DeadlineId,
                    ServiceStepId = serviceStep.StepId,
                };
                result.ProcessInfo.EFormFile = await Repo.AllReadonly<FileMetadata>()
                                                         .Where(x => x.ProcessId == processId &&
                                                                     x.FileSourceTypeId == (int)FileSourceType.EFormApplication)
                                                         .Select(x => new FileVM
                                                         {
                                                             Description = x.Description,
                                                             FileName = x.FileName,
                                                             MetaFileId = x.Id,
                                                         })
                                                         .FirstOrDefaultAsync();
            }
            else
            {
                result.ProcessInfo.OldIncomingDate = oldIncomingDate;
                result.ProcessInfo.OldIncomingNumber = oldIncomingNumber;
            }
            if (preview)
            {
                result.FormTitle = string.Empty;
            }
            if (!string.IsNullOrEmpty(process?.RegisterNumber))
            {
                result.FormTitle += $"Рег. № {process.RegisterNumber}";
            }
            if (!string.IsNullOrEmpty(process?.IncomingNumber))
            {
                result.FormTitle += $" Вх. № {process.IncomingNumber} от {process.IncomingDate.ConvertUtcToBGTime().ToString(FormattingConstant.DateTimeFormat)}";
            }
            if (process?.IncomingDate != null)
            {
                result.IncomingDate = process.IncomingDate;
            }
            if (!preview && serviceStep != null)
            {
                result.FormTitle += $" {serviceStep.Title}";
            }
            if (preview)
            {
                var statusDDL = await GetStatusDDL();
                result.FormTitle += " " + statusDDL.Where(x => x.Code == process?.StatusId.ToString()).Select(x => x.Value).FirstOrDefault() ?? string.Empty;
            }
            return result;
        }

        public void FillProcessInfoVM(IFormCollection form, ProcessInfoVM processInfo)
        {
            processInfo.ReceivedChannelId = form[$"ProcessInfo.{nameof(processInfo.ReceivedChannelId)}"].ToString();
            processInfo.PreferredResultDeliveryMethod = form[$"ProcessInfo.{nameof(processInfo.PreferredResultDeliveryMethod)}"].ToString();
            var deadlineStr = form[$"ProcessInfo.{nameof(processInfo.DeadlineId)}"].ToString();
            if (!string.IsNullOrEmpty(deadlineStr))
            {
                processInfo.DeadlineId = int.Parse(deadlineStr);
            }
            processInfo.OldIncomingNumber = form[$"ProcessInfo.{nameof(processInfo.OldIncomingNumber)}"].ToString();
            processInfo.CoordinationMotive = form[$"ProcessInfo.{nameof(processInfo.CoordinationMotive)}"].ToString();
            var coordinationStatusIdStr = form[$"ProcessInfo.{nameof(processInfo.CoordinationStatusId)}"].ToString();
            if (!string.IsNullOrEmpty(coordinationStatusIdStr))
            {
                processInfo.CoordinationStatusId = int.Parse(coordinationStatusIdStr);
            }
            DateTime? oldIncomingDate = null;
            var oldIncomingDateStr = form[$"ProcessInfo.{nameof(processInfo.OldIncomingDate)}"].ToString();
            if (!string.IsNullOrEmpty(oldIncomingDateStr))
            {
                oldIncomingDate = DateTime.ParseExact(oldIncomingDateStr, "dd.MM.yyyy", null);
            }
            processInfo.OldIncomingDate = oldIncomingDate;
        }

        ///// <summary>
        ///// Връща всички въведени от потребителят данни за дадена форма
        ///// </summary>    
        ///// <param name="processId">Идентификатор на заявена услуга</param>       
        ///// <returns></returns>
        //public async Task<JsonResult> GetFormData(Guid processId)
        //{
        //    try
        //    {
        //        string pid = "177208082"; //TODO: Трябва да се взима от userContext
        //        GetMasterPersonRecordIndexMessage request = new GetMasterPersonRecordIndexMessage();
        //        request.Pid = pid;

        //        MPRIListMessage response = await registerGrpcClient.GetMasterPersonRecordIndexAsync(request);
        //        if (response.Status.Code != Common.ResultCodes.Ok)
        //        {
        //            Logger.LogError($"Проблем при изпълнението на {nameof(registerGrpcClient.GetMasterPersonRecordIndexAsync)}. Код на грешката {response.Status.Code}.");
        //        }

        //        var mpris = response.Items.Select(i => new Guid(i.Id));

        //        List<Data.Models.Process.RegisterItem> registerItems = await Repo.AllReadonly<Data.Models.Process.RegisterItem>()
        //                        .TagWith(nameof(GetFormData))
        //                        .Where(ri => ri.ProcessId == processId)
        //                        .Where(ri => mpris.Contains(ri.MpriId) || ri.IsPublic)
        //                        .ToListAsync();

        //        return new JsonResult(registerItems);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogError(ex, $"Проблем при извличане на данни в {nameof(GetFormData)}");
        //        return null;
        //    }
        //}

        public async Task<Process> Refuse(Guid processId, string reasonForRejection)
        {
            var process = await Repo.All<Process>()
                                   .IgnoreQueryFilters()
                                   .Include(x => x.Form)
                                   .Where(x => x.Id == processId)
                                   .FirstAsync();
            if (process.StatusId == (int)ProcessStatus.Refused || process.StatusId == (int)ProcessStatus.Registered)
            {
                throw new Exception("Неприложима стъпка");
            }
            var rejectionNumberResponse = await numberGeneratorClient.GetNumberAsync(new NumberRequest
            {
                InitialDocumentId = processId.ToString(),
                Register = (await registerService.GetCurrentRegister()).Code,
            });
            if (rejectionNumberResponse.Status.Code != ResultCodes.Ok)
            {
                throw new Exception("Проблем при номериране " + rejectionNumberResponse.Status.Message);
            }
            process.StatusId = (int)ProcessStatus.Refused;
            process.ReasonForRejection = reasonForRejection;
            process.RejectionNumber = rejectionNumberResponse.Number.ToString();
            process.AssignedToUser = null;
            await Repo.SaveChangesAsync();
            return process;
        }

        /// <summary>
        /// Списък на заявени услуги по статус за Dashboard
        /// </summary>
        /// <param name="request"></param>
        /// <param name="statusId">Идентификатор на статус</param>
        /// <returns></returns>
        public async Task<IActionResult> GetProcessListDashboard(IDataTablesRequest request, int statusId)
        {
            var processes = Repo.AllReadonly<Process>()
                                .Where(p => statusId == -1 || p.StatusId == statusId)
                                .Select(x => new ProcessListItemVM
                                {
                                    Id = x.Id,
                                    IncomingNumber = x.IncomingNumber,
                                    IncomingDate = x.IncomingDate,
                                    RegisterNumber = x.RegisterNumber,
                                    ServiceName = x.Service.Title,
                                    StepName = x.LastServiceStep.Title,
                                    StepId = x.LastServiceStepId ?? 0,
                                    StatusId = x.StatusId,
                                    ServiceId = x.ServiceId,
                                    MpriId = x.MpriId,
                                    MpriApplicantId = x.MpriApplicantId,
                                    FromName = x.FromProcess!.RegisterNumber
                                }
                                ).TagWith(nameof(GetProcessListDashboard));

            var countAll = 0;
            (processes, countAll) = request.GetResponseData(processes, null, null, true, nameof(Process.IncomingDate), false);
            //(processes, countAll) = request.GetResponseData(processes);
            var data = await processes.ToListAsync();
            List<string> ids = new();
            foreach (var item in data)
            {
                item.IncomingDate = item.IncomingDate.ConvertUtcToBGTime();
                if (!ids.Any(x => x == item.MpriId.ToString()))
                {
                    ids.Add(item.MpriId.ToString());
                }
                if (!ids.Any(x => x == item.MpriApplicantId.ToString()))
                {
                    ids.Add(item.MpriApplicantId.ToString());
                }
            }
            var statusDDL = await GetStatusDDL();

            var requestMPRI = new GetMPRIListMessage();
            requestMPRI.IdList.AddRange(ids);
            var responseMPRI = await registerGrpcClient.GetMasterPersonRecordIndexListAsync(requestMPRI);

            var serviceSteps = await Repo.AllReadonly<ServiceStep>()
                                        .ToListAsync();


            foreach (var item in data)
            {
                var mpri = responseMPRI.Items.Where(x => x.Id == item.MpriId.ToString()).FirstOrDefault();
                item.Partida = $"{mpri?.Pid} {mpri?.Name}";
                mpri = responseMPRI.Items.Where(x => x.Id == item.MpriApplicantId.ToString()).FirstOrDefault();
                item.Applicant = $"{mpri?.Pid} {mpri?.Name}";
                item.Status = statusDDL.Where(x => x.Code == item.StatusId.ToString()).Select(x => x.Value).FirstOrDefault();
                item.HasNextStep = item.StatusId != (int)ProcessStatus.Registered && item.StatusId != (int)ProcessStatus.Refused;
                item.HasClose = item.StatusId != (int)ProcessStatus.Registered && item.StatusId != (int)ProcessStatus.Refused;
                item.HasDeletion = item.StatusId == (int)ProcessStatus.Registered;
                item.HasChange = item.StatusId == (int)ProcessStatus.Registered;
                item.HasInstruction = item.StatusId == (int)ProcessStatus.Registered;
                var serviceStep = serviceSteps.Where(x => x.ServiceId == item.ServiceId &&
                                                          x.StepId == item.StepId)
                                            .OrderBy(x => x.OrderNum)
                                            .FirstOrDefault();
                var orderNum = serviceStep?.OrderNum ?? 0;
                item.NextStep = string.Join(' ', serviceSteps.Where(x => x.ServiceId == item.ServiceId &&
                                                 x.OrderNum == orderNum)
                                     .Select(x => x.Title)
                                     .ToList());
            }
            return request.GetResponseJson(data.AsQueryable(), countAll);
        }

        public async Task<int> GetUserAssignedProcessCount()
        {
            var countInstruction = await Repo.AllReadonly<Process>()
                .Where(x =>
                    x.AssignedToUser == _userContext.UserId &&
                    x.TenantId == _userContext.AdministrationId &&
                    x.StatusId == (int)ProcessStatus.Instruction &&
                    x.Instructions.Any(i => i.InstructionResponses.Any(r => r.ReceivedBy == null))
                )
                .CountAsync();
            var count = await Repo.AllReadonly<Process>()
               .Where(x =>
                   x.AssignedToUser == _userContext.UserId &&
                   x.TenantId == _userContext.AdministrationId &&
                   (x.StatusId == (int)ProcessStatus.Send ||
                    x.StatusId == (int)ProcessStatus.InWork ||
                    x.StatusId == (int)ProcessStatus.ForCoordination ||
                    x.StatusId == (int)ProcessStatus.Coordination)
               )
               .CountAsync();
            return count + countInstruction;
        }

        public async Task<Process?> GetAssignableProcess()
        {
            Process? process = null;

            //Съгласуване
            var roles = _userContext.CoordinationRoles;
            if (roles.Any())
            {
                var rService = await serviceService.GetRegisterService();
                foreach (var role in roles)
                {
                    var rSteps = rService.Steps.Where(x => x.Roles.Any(r => r == role)).ToList();
                    foreach (var rStep in rSteps)
                    {
                        var prevStep = rService.Steps
                                               .Where(x => x.OrderNum < rStep.OrderNum)
                                               .OrderByDescending(x => x.OrderNum)
                                               .FirstOrDefault();
                        if (prevStep != null)
                        {
                            process = await Repo.AllReadonly<Process>()
                                .Where(
                                    x => x.LastServiceStepId == prevStep.Id &&
                                    (x.StatusId != (int)ProcessStatus.Instruction) &&
                                    (x.StatusId != (int)ProcessStatus.Refused) &&
                                    (x.StatusId != (int)ProcessStatus.Registered) &&
                                    x.TenantId == _userContext.AdministrationId &&
                                    (x.AssignedToUser == null || x.AssignedToUser == Guid.Empty)
                                )
                                .OrderBy(x => x.IncomingDate)
                                .TagWith(nameof(GetAssignableProcess))
                                .FirstOrDefaultAsync();
                            if (process != null)
                            {
                                return process;
                            }
                        }
                    }
                }
            }
            if (_userContext.IsInRole(UserRoles.Editor))
            {
                // Отговор на указания
                process = await Repo.AllReadonly<Process>()
                                    .Where(
                                        x =>
                                        (x.StatusId == (int)ProcessStatus.Instruction) &&
                                        x.TenantId == _userContext.AdministrationId &&
                                        x.Instructions.Any(i => i.InstructionResponses.Any(r => r.ReceivedBy == null)) &&
                                        (x.AssignedToUser == null || x.AssignedToUser == Guid.Empty)
                                    )
                                    .OrderBy(x => x.IncomingDate)
                                    .TagWith(nameof(GetAssignableProcess))
                                    .FirstOrDefaultAsync();
                if (process != null)
                {
                    return process;
                }

                // Вписване

                process = await Repo.AllReadonly<Process>()
                                .Where(
                                    x =>
                                    (x.StatusId == (int)ProcessStatus.Send ||
                                     x.StatusId == (int)ProcessStatus.InWork ||
                                     x.StatusId == (int)ProcessStatus.Coordination) &&
                                    x.TenantId == _userContext.AdministrationId &&
                                    (x.AssignedToUser == null || x.AssignedToUser == Guid.Empty)
                                )
                                .OrderBy(x => x.IncomingDate)
                                .TagWith(nameof(GetAssignableProcess))
                                .FirstOrDefaultAsync();
                if (process != null)
                {
                    return process;
                }
            }
            return process;
        }
        public async Task AssignProcess(Guid processid)
        {
            var process = await Repo.All<Process>()
                                .Where(x => x.Id == processid)
                                .FirstAsync();
            process.AssignedToUser = _userContext.UserId;

            await Repo.SaveChangesAsync();
        }
        /// <summary>
        /// Връща данни за Dashboard
        /// </summary>     
        /// <returns></returns>
        public async Task<DashboardVM> GetDashboardData()
        {
            var model = new DashboardVM();
            model.Process.ProcessAllCount = await Repo.AllReadonly<Process>().CountAsync();
            model.Process.ProcessSentCount = await Repo.AllReadonly<Process>().Where(p => p.StatusId == (int)ProcessStatus.Send).CountAsync();
            model.Process.ProcessInProgressCount = await Repo.AllReadonly<Process>().Where(p => p.StatusId == (int)ProcessStatus.InWork).CountAsync();
            model.Process.ProcessRegisteredCount = await Repo.AllReadonly<Process>().Where(p => p.StatusId == (int)ProcessStatus.Registered).CountAsync();
            model.Process.ProcessRefusedCount = await Repo.AllReadonly<Process>().Where(p => p.StatusId == (int)ProcessStatus.Refused).CountAsync();

            return model;
        }

        /// <summary>
        /// Форматира текста в удобен формат за филтрация по дата.
        /// Позлва се при търсене по дата.
        /// </summary>
        /// <param name="bgFormatInput"></param>
        /// <returns></returns>
        public static string CreateDateSearchString(string bgFormatInput)
        {
            if (bgFormatInput.All(c => char.IsDigit(c) || c == '.'))
            {
                string[] parts = bgFormatInput.Split('.');
                Array.Reverse(parts);
                string result = string.Join("-", parts);
                return $"%{result}%";

            }
            return string.Empty;
        }

        public async Task<Guid?> SaveCertificateFileDraft(Guid processId, byte[] filesAsBytes, int typeMessageId)
        {
            try
            {
                var process = await Repo.All<Process>()
                                  .Where(x => x.Id == processId)
                                  .FirstAsync();
                var fileName = string.Empty;
                int metaSourceTypeId = 0;
                switch (typeMessageId)
                {
                    case (int)EDeliveryMessageType.OutCertificate:
                        fileName = $"CertificateDraft_{process.RegisterNumber}.pdf";
                        metaSourceTypeId = (int)FileSourceType.CertificateDraft;
                        break;
                    case (int)EDeliveryMessageType.OutRefuse:
                        fileName = $"RejectionDraft_{process.RegisterNumber}.pdf";
                        metaSourceTypeId = (int)FileSourceType.RefuseDraft;
                        break;
                    case (int)EDeliveryMessageType.OutInstruction:
                        fileName = $"InstructionDraft_{process.RegisterNumber}.pdf";
                        metaSourceTypeId = (int)FileSourceType.InstructionDraft;
                        break;
                    default:
                        break;
                }
                var filemetadata = new FileMetadata
                {
                    FileSourceTypeId = metaSourceTypeId,
                    FileName = fileName,
                    ProcessId = processId
                };
                filemetadata.FileId = Guid.Parse(await _objectStoreService.SaveObject(fileName, filesAsBytes, "application/pdf", null));
                await Repo.AddAsync(filemetadata);
                await Repo.SaveChangesAsync();
                return filemetadata.Id;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Проблем при качване на файл с удостоверение");
                return null;
            }
        }

        public async Task SendMessageForProcess(Guid processId, byte[] filesAsBytes, int typeMessageId, string sourceId, string message)
        {
            var process = await Repo.AllReadonly<Process>()
                                    .Include(x => x.ProcessSteps)
                                    .IgnoreQueryFilters()
                                    .Where(x => x.Id == processId)
                                    .FirstAsync();
            var administration = await registerGrpcClient.GetAdministrationAsync(new GetAdministrationRequest { AdministrationId = _userContext.AdministrationId.ToString() });
            var register = await registerService.GetCurrentRegister();
            int fileSourceType = 0;
            var subject = $"{administration.Data.Name} ({register.Name})";
            var fileName = $"{process.RegisterNumber}.pdf";
            int metaSourceTypeId = 0;
            switch (typeMessageId)
            {
                case (int)EDeliveryMessageType.OutCertificate:
                    fileName = $"Certificate_{process.RegisterNumber}.pdf";
                    fileSourceType = (int)IntegrationSourceType.Certificate;
                    metaSourceTypeId = (int)FileSourceType.Certificate;
                    break;
                case (int)EDeliveryMessageType.OutRefuse:
                    fileName = $"Rejection_{process.RejectionNumber}.pdf";
                    fileSourceType = (int)IntegrationSourceType.Refuse;
                    metaSourceTypeId = (int)FileSourceType.Refuse;
                    break;
                case (int)EDeliveryMessageType.OutInstruction:
                    fileName = $"Instruction_{process.IncomingNumber}.pdf";
                    fileSourceType = (int)IntegrationSourceType.Instruction;
                    metaSourceTypeId = (int)FileSourceType.Instruction;
                    break;
                default:
                    break;
            }

            var service = await Repo.AllReadonly<Service>()
                                    .IgnoreQueryFilters()
                                    .Where(x => x.Id == process.ServiceId)
                                    .FirstAsync();
            var options = new IOStampOptions
            {
                DisplayText = administration.Data.Name,
                Reason = service.Title,
                Coordinates = new iText.Kernel.Geom.Rectangle(400, 800, 180, 28),
                PageNum = 1,
                PathToStamp = configuraion.GetValue<string>("Signer:CertificateFile"),
                Password = configuraion.GetValue<string>("Signer:CertificatePassword"),
                Font = "SignFonts/times.ttf"
            };

            string? pidType = null;
            string? pid = null;
            string? name = null;

            if (process.MpriId == null || process.MpriId == Guid.Empty)
            {
                var processStep = process.ProcessSteps.OrderByDescending(x => x.ModifiedOn).First();
                var formFields = JsonSerializer.Deserialize<List<FormField>>(processStep.StepData)!;
                (pidType, pid, name) = GetMPRIData(PersonRole.Partida, formFields);
            }
            else
            {
                var requestMPRI = new GetMPRIListMessage();
                requestMPRI.IdList.Add(process.MpriId.ToString());
                var responseMPRI = await registerGrpcClient.GetMasterPersonRecordIndexListAsync(requestMPRI);
                var mpri = responseMPRI.Items.First();
                pidType = mpri.PidType;
                pid = mpri.Pid;
                name = mpri.Name;
            }

            var outMessage = new OutboxMessage
            {
                ProcessId = process.Id.ToString(),
                RegisterId = await registerService.GetCurrentRegisterId(),
                TenantId = process.TenantId.ToString(),
                MessageTypeId = typeMessageId,
                Subject = subject,
                Message = message,
                Rnu = process.Id.ToString(),
                SourceType = fileSourceType,
                SourceId = sourceId,
                TemplateId = 1,
                Uic = pid,
                UicType = pidType
            };
            if (filesAsBytes.Length > 0)
            {
                var filemetadata = new FileMetadata
                {
                    FileSourceTypeId = metaSourceTypeId,
                    FileName = fileName,
                    ProcessId = process.Id,
                    SourceId = sourceId,
                };

                filesAsBytes = signToolsService.StampIt(filesAsBytes, options);
                using MemoryStream ms = new MemoryStream(filesAsBytes);
                filesAsBytes = signToolsService.AddLTV(ms);

                filemetadata.FileId = Guid.Parse(await _objectStoreService.SaveObject(fileName, filesAsBytes, "application/pdf", null));
                await Repo.AddAsync(filemetadata);
                await Repo.SaveChangesAsync();
                var fileUrl = await _objectStoreService.GetPresignedUrl(filemetadata.FileId.ToString());
                outMessage.OutboxFiles.Add(new OutboxFile
                {
                    FileName = filemetadata.FileName,
                    FileUrl = fileUrl
                });
            }
            if (process.PreferredResultDeliveryMethod != ChannelType.EDelivery)
                return;

            var response = await integrationGrpcClient.SendMessageAsync(outMessage);
            if (response.Status.Code != ResultCodes.Ok)
            {
                throw new Exception(response.Status.Message);
            }
        }

        public async Task<byte[]> GetCertificateFile(Guid id)
        {
            try
            {
                var filemetadata = await Repo.AllReadonly<FileMetadata>()
                                             .Where(x => x.Id == id)
                                             .FirstAsync();
                (var data, _) = await _objectStoreService.GetObject(filemetadata.FileId.ToString());
                return data;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Проблем при сваляне на файл с удостоверение");
                return null;
            }
        }
        public async Task<byte[]> GetCertificateFileSigned(Guid processId)
        {
            try
            {
                var filemetadata = await Repo.AllReadonly<FileMetadata>()
                                             .Where(x => x.ProcessId == processId && x.FileSourceTypeId == (int)FileSourceType.Certificate)
                                             .FirstAsync();
                (var data, _) = await _objectStoreService.GetObject(filemetadata.FileId.ToString());
                return data;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Проблем при сваляне на файл с удостоверение");
                return null;
            }
        }

        public async Task<bool> IsCertificateStep(int serviceStepId)
        {
            return await Repo.AllReadonly<ServiceStep>()
                    .TagWith(nameof(IsCertificateStep))
                    .Where(x => x.Id == serviceStepId)
                    .Select(x => x.StatusId == (int)ProcessStatus.Certificate)
                    .FirstAsync();
        }

        /// <summary>
        /// Връща заявена услуга по номер на заявена услуга от е-форма
        /// </summary>
        /// <param name="eFormRegisteredServiceNumber"></param>
        /// <returns></returns>
        public async Task<ProcessVM> GetProcess(Guid eFormRegisteredServiceNumber)
        {
            Process existingProcess = await Repo.AllReadonly<Process>()
                .TagWith(nameof(GetProcess))
                .FirstOrDefaultAsync(s =>
                    s.EFormRegisteredServiceNumber.HasValue && s.EFormRegisteredServiceNumber.Value == eFormRegisteredServiceNumber);

            if (existingProcess != null)
            {
                return new ProcessVM()
                {
                    Id = existingProcess.Id,
                    IncomingDate = existingProcess.IncomingDate,
                    IncomingNumber = existingProcess.IncomingNumber
                };
            }

            return null;
        }

        /// <summary>
        /// Връща заявена услуга по номер на стар запис
        /// </summary>
        /// <param name="oldIncomingNumber">Номер на стар запис</param>
        /// <returns></returns>
        public async Task<ProcessVM> GetProcessByOldIncomingNumber(string oldIncomingNumber)
        {
            Process existingProcess = await Repo.AllReadonly<Process>()
                .TagWith(nameof(GetProcessByOldIncomingNumber))
                .FirstOrDefaultAsync(s =>
                    !string.IsNullOrWhiteSpace(s.OldIncomingNumber) && s.OldIncomingNumber == oldIncomingNumber);

            if (existingProcess != null)
            {
                return new ProcessVM()
                {
                    Id = existingProcess.Id,
                    IncomingNumber = existingProcess.IncomingNumber,
                    IncomingDate = existingProcess.IncomingDate,
                    OldIncomingDate = existingProcess.OldIncomingDate,
                    OldIncomingNumber = existingProcess.OldIncomingNumber
                };
            }

            return null;
        }

        public async Task<string?> GetProcessLabel(Guid processId)
        {
            var process = await Repo.AllReadonly<Process>()
                                    .Where(x => x.Id == processId)
                                    .FirstOrDefaultAsync();

            return process == null ? null : $"{process.IncomingNumber} {process.IncomingDate: dd.MM.yyyy}";
        }
        public async Task<(Process, Guid)> SaveInstruction(InstructionVM model)
        {
            var process = await Repo.All<Process>()
                                    .IgnoreQueryFilters()
                                    .Include(x => x.Form)
                                    .Where(x => x.Id == model.ProcessId)
                                    .FirstAsync();
            process.StatusId = (int)ProcessStatus.Instruction;
            process.AssignedToUser = null;
            var instruction = await Repo.All<Instruction>()
                                        .Where(x => x.Id == model.Id)
                                        .FirstOrDefaultAsync();
            if (instruction == null)
            {
                instruction = new Instruction
                {
                    Id = model.Id,
                    ProcessId = model.ProcessId,
                    IsActive = false,
                };
                await Repo.AddAsync(instruction);
            }
            instruction.Content = model.Content;
            instruction.ModifiedByUserId = _userContext.UserId;
            instruction.ModifiedOn = DateTime.UtcNow;

            await SetInstructionResponseReceived(process.Id);
            await Repo.SaveChangesAsync();
            return (process, instruction.Id);
        }

        public async Task SetInstructionActive(Guid id)
        {
            var instruction = await Repo.All<Instruction>()
                                        .IgnoreQueryFilters()
                                        .Where(x => x.Id == id)
                                        .FirstOrDefaultAsync();
            if (instruction != null)
            {
                instruction.IsActive = true;
            }
            await Repo.SaveChangesAsync();
        }

        public async Task ImportEDeliveryFile(EDeliveryMessageVM model)
        {
            if (model.MessageTypeId == (int)EDeliveryMessageType.InstructionResponse)
            {
                if (await Repo.All<InstructionResponse>().AnyAsync(x => x.Id == model.Id))
                    return;
                var instructionResponse = new InstructionResponse
                {
                    Id = model.Id,
                    InstructionId = model.SourceId ?? Guid.Empty,
                    IsActive = true,
                    Content = model.Content,
                };
                await Repo.AddAsync(instructionResponse);
                var instruction = await Repo.All<Instruction>()
                                            .Where(x => x.Id == instructionResponse.InstructionId)
                                            .FirstAsync();
                foreach (var edeliveryFile in model.EDeliveryFiles)
                {
                    var integrationFile = new IntegrationFile
                    {
                        FileName = edeliveryFile.FileName,
                        IsActive = true,
                        SourceId = instructionResponse.Id,
                        IntegrationFileId = edeliveryFile.Id,
                        SourceType = (int)IntegrationSourceType.InstructionResponse,
                    };
                    await Repo.AddAsync(integrationFile);
                    var metaFile = new FileMetadata
                    {
                        ProcessId = instruction.ProcessId,
                        FileName = integrationFile.FileName ?? string.Empty,
                        FileSourceTypeId = (int)FileSourceType.InstructionResponse,
                        SourceId = instructionResponse.Id.ToString(),
                    };
                    var fileData = await httpRequester.GetFileAsync("objectStoreClient", edeliveryFile.FileUrl!);
                    metaFile.FileId = Guid.Parse(await _objectStoreService.SaveObject(metaFile.FileName, fileData));
                    await Repo.AddAsync(metaFile);
                    integrationFile.FileMetadataId = metaFile.Id;
                }
            }
            await Repo.SaveChangesAsync();
        }

        public async Task<List<FileMetadata>> ImportApplicationEDeliveryFile(List<EDeliveryFileVM> files)
        {
            var result = new List<FileMetadata>();
            foreach (var edeliveryFile in files)
            {
                var metaFile = new FileMetadata
                {
                    FileName = edeliveryFile.FileName ?? string.Empty,
                    FileSourceTypeId = edeliveryFile.FileSourceTypeId == (int)EDeliveryFileType.Application ?
                                       (int)FileSourceType.EFormApplication :
                                       (int)FileSourceType.AttachedDocument,
                };
                var fileData = await httpRequester.GetFileAsync("objectStoreClient", edeliveryFile.FileUrl!);
                metaFile.FileId = Guid.Parse(await _objectStoreService.SaveObject(metaFile.FileName, fileData));
                await Repo.AddAsync(metaFile);
                result.Add(metaFile);
            }
            await Repo.SaveChangesAsync();
            return result;
        }

        public async Task ImportApplicationEDeliveryFileSetProcess(Guid processId, List<FileMetadata> files)
        {
            foreach (var file in files)
            {
                file.ProcessId = processId;
                file.SourceId = processId.ToString();
            }
            await Repo.SaveChangesAsync();
        }

        public async Task<InstructionResponseVM> GetInstructionResponses(Guid instructionId)
        {
            var files = Repo.AllReadonly<IntegrationFile>()
                            .Where(x => x.SourceType == (int)IntegrationSourceType.InstructionResponse);
            var result = await Repo.AllReadonly<Instruction>()
                             .Where(x => x.Id == instructionId)
                             .Select(x => new InstructionResponseVM
                             {
                                 Items = x.InstructionResponses.AsQueryable()
                                           .Select(InstructionResponseToItemVM())
                                           .ToList()
                             })
                             .FirstAsync();
            return result;
        }

        public async Task<InstructionResponseVM> GetInstructionResponsesOnProcess(Guid processId)
        {
            var files = Repo.AllReadonly<IntegrationFile>()
                            .Where(x => x.SourceType == (int)IntegrationSourceType.InstructionResponse);
            var instructions = await Repo.AllReadonly<Instruction>()
                             .Where(x => x.ProcessId == processId)
                             .Select(x => new InstructionResponseVM
                             {
                                 Items = x.InstructionResponses.AsQueryable()
                                          .Select(InstructionResponseToItemVM())
                                          .ToList()
                             })
                             .ToListAsync();
            var result = new InstructionResponseVM();
            foreach (var instruction in instructions)
            {
                result.Items.AddRange(instruction.Items);
            }
            return result;
        }


        public async Task<BlanksTemplate?> GetBlankOnRegister(int formParentId)
        {
            return await Repo.AllReadonly<BlanksTemplate>()
                             .Where(x => x.SourceType == (int)BlankSourceType.CertificateOnRegister &&
                                         x.FormParentId == formParentId)
                             .FirstOrDefaultAsync();
        }

        public async Task<BlanksTemplate?> GetBlankRefuse(int formParentId)
        {
            return await Repo.AllReadonly<BlanksTemplate>()
                             .Where(x => x.SourceType == (int)BlankSourceType.Refuse &&
                                         x.FormParentId == formParentId)
                             .FirstOrDefaultAsync();
        }
        public async Task<BlanksTemplate?> GetBlankInstruction(int formParentId)
        {
            return await Repo.AllReadonly<BlanksTemplate>()
                             .Where(x => x.SourceType == (int)BlankSourceType.Instruction &&
                                         x.FormParentId == formParentId)
                             .FirstOrDefaultAsync();
        }
        private Expression<Func<InstructionResponse, InstructionResponseItemVM>> InstructionResponseToItemVM()
        {
            var files = Repo.AllReadonly<FileMetadata>()
                          .Where(x => x.FileSourceTypeId == (int)FileSourceType.InstructionResponse);

            return r => new InstructionResponseItemVM
            {
                Id = r.Id,
                ModifiedOn = r.ModifiedOn,
                ProcessId = r.Instruction.ProcessId,
                InstructionId = r.InstructionId,
                Content = r.Content,
                CanEdit = r.Instruction.ClosedOn == null,
                Files = files.Where(f => f.SourceId == r.Id.ToString())
                                                             .Select(f => new FileVM
                                                             {
                                                                 FileName = f.FileName,
                                                                 MetaFileId = f.Id,
                                                             })
                                                            .ToList()
            };
        }

        public IActionResult GetInstructionResponseList(IDataTablesRequest request, Guid instructionId)
        {
            var query = Repo.AllReadonly<InstructionResponse>()
                             .Where(x => x.InstructionId == instructionId)
                             .Select(InstructionResponseToItemVM());
            return request.GetResponse(query);
        }

        public async Task<InstructionResponseItemVM> GetInstructionResponse(Guid id)
        {
            return await Repo.AllReadonly<InstructionResponse>()
                             .Where(x => x.Id == id)
                             .Select(InstructionResponseToItemVM())
                             .FirstAsync();
        }

        /// <summary>
        /// Записва файл в хранилището
        /// </summary>
        /// <param name="file">Файлът за запис</param>
        /// <param name="key">Ключ на файла при презапис</param>
        /// <param name="eformId">Идентификатор на е-форма, ако файлът е заявление подадено чрез е-форма</param>
        /// <param name="eformDateOfFill">Дата на попълване на е-формата, ако файлът е заявление подадено чрез е-форма</param>
        /// <returns>Ключа на записания файл</returns>
        public async Task<Guid> SaveAttachedFile(IFormFile file)
        {
            using MemoryStream ms = new MemoryStream();
            await file.CopyToAsync(ms);
            ms.Position = 0; // Връщаме MemoryStream на положение 0 иначе гърми с грешка OException: PDF header not found 
            var filesAsBytes = ms.ToArray();
            var fileId = await _objectStoreService.SaveObject(file.FileName, filesAsBytes, file.ContentType, null);
            var newFileMetadata = new FileMetadata()
            {
                FileName = file.FileName,
                ModifiedByUserId = _userContext.UserId,
                FileId = fileId?.ToGuid() ?? Guid.Empty,
                ModifiedOn = DateTime.UtcNow,
                //todo:
            };
            await Repo.AddAsync(newFileMetadata);

            await Repo.SaveChangesAsync();
            return newFileMetadata.Id;
        }

        public async Task SaveInstructionResponse(InstructionResponseItemVM model)
        {
            var instructionResponse = await Repo.All<InstructionResponse>()
                                          .Where(x => x.Id == model.Id)
                                          .FirstOrDefaultAsync();
            if (instructionResponse == null)
            {
                instructionResponse = new InstructionResponse
                {
                    Id = model.Id,
                    InstructionId = model.InstructionId,
                    IsActive = true,
                    Content = model.Content,
                };
                await Repo.AddAsync(instructionResponse);
            }
            instructionResponse.IsActive = true;
            instructionResponse.Content = model.Content;

            var instruction = await Repo.All<Instruction>()
                                        .Where(x => x.Id == instructionResponse.InstructionId)
                                        .FirstAsync();
            foreach (var file in model.Files)
            {
                var metaFile = await Repo.All<FileMetadata>()
                                         .Where(x => x.Id == file.MetaFileId)
                                         .FirstAsync();
                metaFile.ProcessId = instruction.ProcessId;
                metaFile.FileSourceTypeId = (int)FileSourceType.InstructionResponse;
                metaFile.SourceId = instructionResponse.Id.ToString();
            }
            var metaFiles = await Repo.All<FileMetadata>()
                                       .Where(x => x.SourceId == instructionResponse.Id.ToString() &&
                                                   x.FileSourceTypeId == (int)FileSourceType.InstructionResponse)
                                       .ToListAsync();
            foreach (var metaFile in metaFiles)
            {
                if (!model.Files.Any(x => x.MetaFileId == metaFile.Id))
                    metaFile.IsActive = false;
            }
            await Repo.SaveChangesAsync();
        }
        public async Task<(string, string)> GetAttachedFileUrl(Guid id)
        {
            var metaFile = await Repo.AllReadonly<FileMetadata>()
                                     .Where(x => x.Id == id)
                                     .FirstAsync();
            var fileUrl = string.Empty;
            if (metaFile.FileId != Guid.Empty)
            {
                fileUrl = await _objectStoreService.GetPresignedUrl(metaFile.FileId.ToString());
            }
            else
            {
                var request = new IntegrationFileRequest();
                request.Ids.Add(metaFile.FileId.ToString());
                var response = await integrationGrpcClient.GetIntegrationFilesUrlAsync(request);
                fileUrl = response.Files.First().Url;
            }
            return (fileUrl, metaFile.FileName);
        }
        public async Task<DateTime> GetDeadlineDate(int deadlineId, Guid processId)
        {
            var process = await Repo.AllReadonly<Process>()
                                     .Where(x => x.Id == processId)
                                     .FirstOrDefaultAsync();
            var dateFrom = process?.IncomingDate ?? DateTime.Today;
            var deadline = await Repo.AllReadonly<DeadlineDay>()
                                     .Where(x => x.Id == deadlineId)
                                     .FirstOrDefaultAsync();
            if (deadline == null)
            {
                return dateFrom;
            }
            if (deadline.DayTypeId == CalendarDayKind.CalendarDay)
            {
                return dateFrom.AddDays(deadline.Days);
            }
            var response = await registerGrpcClient.CalcWorkDaysAsync(new CalendarDayCalcRequest
            {
                Days = deadline.Days,
                FromDate = dateFrom.ToUniversalTime().ToTimestamp(),
            });
            return response.ToDate.ToDateTime();
        }

        public async Task DeAssignUser(Guid processId)
        {
            var process = await Repo.All<Process>()
                                    .IgnoreQueryFilters()
                                    .Where(x => x.Id == processId &&
                                                 x.StatusId != (int)ProcessStatus.Registered &&
                                                 x.StatusId != (int)ProcessStatus.Refused &&
                                                 x.AssignedToUser != Guid.Empty &&
                                                 x.AssignedToUser != null)
                                    .FirstOrDefaultAsync();
            process!.AssignedToUser = null;
            await Repo.SaveChangesAsync();
        }

        public async Task<IActionResult> GetProcessDeliveryList(IDataTablesRequest request, ProcessDeliveryFilterVM filter)
        {
            var queryWhere = Repo.AllReadonly<ProcessDelivery>()
                                            .IgnoreQueryFilters()
                                            .Where(x => x.IsActive)
                                            .Where(x => x.ProcessId == filter.ProcessId);

            var query = queryWhere.Select(x => new ProcessDeliveryVM
            {
                Id = x.Id,
                DeliveryDate = x.DeliveryDate,
                Description = x.Description
            })
            .TagWith(nameof(GetInstructionList));
            var countAll = 0;
            (query, countAll) = request.GetResponseData(query);
            var data = await query.ToListAsync();

            return request.GetResponseJson(data.AsQueryable(), countAll);
        }
    }
}

