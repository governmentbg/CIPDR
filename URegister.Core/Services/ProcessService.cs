using Core.Services;
using Microsoft.Extensions.Logging;
using URegister.Core.Contracts;
using URegister.Core.Data;
using Microsoft.EntityFrameworkCore;
using URegister.Core.Models.Process;
using URegister.Core.Data.Models.Register;
using URegister.Core.Data.Models.Process;
using URegister.Infrastructure.Extensions;
using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using URegister.Core.Data.Models.Common;
using System.Text.Json;
using URegister.NumberGenerator;
using URegister.Infrastructure.Model.RegisterForms;
using URegister.RegistersCatalog;
using URegister.Common;


namespace URegister.Core.Services
{
    public class ProcessService : BaseService, IBaseService, IProcessService
    {
        private readonly NumberGenerator.NumberGenerator.NumberGeneratorClient numberGeneratorClient;
        private readonly IRegisterService registerService;
        private readonly RegistersCatalogGrpc.RegistersCatalogGrpcClient registerGrpcClient;
        private readonly IFormConfigurationPersistenceService formConfigurationPersistenceService;
        public ProcessService(
         IApplicationRepository repo,
         NumberGenerator.NumberGenerator.NumberGeneratorClient numberGeneratorClient,
         RegistersCatalogGrpc.RegistersCatalogGrpcClient registerGrpcClient,
         IFormConfigurationPersistenceService formConfigurationPersistenceService,
         IRegisterService registerService,
         ILogger<BaseService> logger
        ) : base(repo, logger)
        {
            this.registerService = registerService;
            this.numberGeneratorClient = numberGeneratorClient;
            this.registerGrpcClient = registerGrpcClient;
            this.formConfigurationPersistenceService = formConfigurationPersistenceService;
        }
        public async Task AddProcess(ProcessVM model)
        {
            // TODO: Да се махне да се взима от UserContext
            var administration = await Repo.AllReadonly<Administration>()
                                           .FirstAsync();
            var process = new Process
            {
                Id = model.Id,
                ServiceId = model.ServiceId,
                IncomingNumber = model.IncomingNumber,
                IncomingDate = model.IncomingDate.ConvertToUtcIfUnspecified(),
                TennantId = administration.Id,
            };
            await Repo.AddAsync(process);
            await Repo.SaveChangesAsync();
        }
        public async Task AddStep(ProcessStepVM model)
        {
            var process = await Repo.All<Process>()
                                    .Where(x => x.Id == model.ProcessId)
                                    .TagWith(nameof(AddStep))
                                    .FirstAsync();

            var stepData = JsonSerializer.Serialize(model.FormFields);
            var processStep = new ProcessStep
            {
                ProcessId = model.ProcessId,
                FormId = model.FormParentId,
                ServiceStepId = model.ServiceStepId,
                OrderNum = model.OrderNum,
                StepData = stepData,
            };
            await Repo.AddAsync(processStep);
            //TODO:
            var serviceStep = await Repo.AllReadonly<ServiceStep>()
                                        .Where(x => x.Id == model.ServiceStepId)
                                        .TagWith(nameof(AddStep))
                                        .FirstAsync();
            if (serviceStep.StepId == 3)
            {
                await RegisterStep(processStep);
            }
            await Repo.SaveChangesAsync();
        }

        public IActionResult GetProcessList(IDataTablesRequest request)
        {
            var query = Repo.AllReadonly<Process>()
                          .Select(x => new ProcessListItemVM
                          {
                              Id = x.Id,
                              IncomingNumber = x.IncomingNumber,
                              IncomingDate = x.IncomingDate.ConvertUtcToBGTime(),
                              RegisterNumber = x.RegisterNumber,
                              ServiceName = x.Service.Title,
                              StepName = x.ProcessSteps
                                          .OrderByDescending(s => s.ServiceStep.OrderNum)
                                          .Select(s => s.ServiceStep.Title)
                                          .FirstOrDefault(),
                              StepId = x.ProcessSteps
                                          .OrderByDescending(s => s.ServiceStep.OrderNum)
                                          .Select(s => s.ServiceStep.StepId)
                                          .FirstOrDefault()
                          })
                          .TagWith(nameof(GetProcessList));

            return request.GetResponse(query);
        }

        private async Task<List<ServiceStep>> GetProcessSteps(Guid processId)
        {
            var process = await Repo.AllReadonly<Process>()
                                    .Include(x => x.ProcessSteps)
                                    .Where(x => x.Id == processId)
                                    .TagWith(nameof(GetProcessSteps))
                                    .FirstAsync();
            var step = process.ProcessSteps
                              .OrderByDescending(x => x.OrderNum)
                              .FirstOrDefault();
            var orderNum = step?.OrderNum ?? 0;
            var serviceStep = await Repo.AllReadonly<ServiceStep>()
                                        .Where(x => x.ServiceId == process.ServiceId &&
                                                    x.OrderNum > orderNum)
                                        .OrderBy(x => x.OrderNum)
                                        .FirstOrDefaultAsync();
            orderNum = serviceStep?.OrderNum ?? 0;
            return await Repo.AllReadonly<ServiceStep>()
                                        .Where(x => x.ServiceId == process.ServiceId &&
                                                    x.OrderNum == orderNum)
                                        .ToListAsync();
        }

        public async Task<ProcessStepVM> GetFormViewModel(Guid processId)
        {
            var serviceSteps = await GetProcessSteps(processId);
            var serviceStep = serviceSteps.First()!;
            var formModel = await formConfigurationPersistenceService.GetFormViewModel(serviceStep.FormParentId);
            var processStep = await Repo.AllReadonly<ProcessStep>()
                                                .Where(x => x.ProcessId == processId)
                                                .OrderByDescending(x => x.ServiceStep.OrderNum)
                                                .TagWith(nameof(GetFormViewModel))
                                                .FirstOrDefaultAsync();
            if (processStep != null)
            {
                var formFields = JsonSerializer.Deserialize<List<FormField>>(processStep.StepData)!;
                formModel.FormFields = formFields;
            }
            return ToProcessStepVM(processId, serviceStep.Id, serviceStep.OrderNum, formModel);
        }

        private FormField? FindPersonIdentifierPartida(List<FormField> formFields)
        {
            foreach (var formField in formFields)
            {
                if (formField.Type == "PersonIdentifier" && 
                    !string.IsNullOrWhiteSpace(formField.Value) && 
                    formField.Value.EndsWith(":true"))
                {
                    return formField;
                }
                if (formField.Fields?.Any() == true)
                {
                    var result = FindPersonIdentifierPartida(formField.Fields);
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
            return value.Split(":");
        }

        private string GetPidFieldValue(string[] values, int index)
        {
            if (index < values.Length)
                return values[index];
            return string.Empty;
        }
        public async Task RegisterStep(ProcessStep processStep)
        {
            var formFields = JsonSerializer.Deserialize<List<FormField>>(processStep.StepData)!;
            var process = await Repo.All<Process>()
                                    .Where(x => x.Id == processStep.ProcessId)
                                    .FirstAsync();
            var personIdentifier = FindPersonIdentifierPartida(formFields);
            if (personIdentifier == null)
                throw new Exception("Липсват данни за партида");
            var pidValues = ParsePidFieldValue(personIdentifier.Value);

            var responseMpriAdd = await registerGrpcClient.AddMasterPersonRecordsIndexAsync(new MasterPersonRecordsIndexAddMessage
            {
                PidType = GetPidFieldValue(pidValues, 0),
                Pid = GetPidFieldValue(pidValues, 1),
                Name = string.Empty,
                RegisterId = await registerService.GetCurrentRegisterId()
            });
            process.MpriId = Guid.Parse(responseMpriAdd.Id);
            process.RegisteredStepId = processStep.Id;
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
            foreach (var formField in formFields)
            {
                await AddRegisterItem(process, formField, formField.Identifier);//, formField.FieldId);
                if (formField.Fields?.Any() == true)
                {
                    foreach (var innerformField in formField.Fields)
                    {
                        await AddRegisterItem(process, innerformField, formField.Identifier);//formField.FieldId);
                    }
                }
                if (formField.Repetitions?.Any() == true)
                {
                    foreach (var innerformField in formField.Repetitions)
                    {
                        await AddRegisterItem(process, innerformField, formField.Identifier);//, formField.FieldId);
                    }
                }
            }

            await Repo.SaveChangesAsync();
        }

        private async Task AddRegisterItem(Process process, FormField formField, Guid parenId)
        {
            var registerItem = new Data.Models.Process.RegisterItem
            {
                TennantId = process.TennantId,
                Name = formField.Name,
                FieldId = formField.Identifier, //formField.FieldId,
                ParentFieldId = parenId,
                IsComplex = formField.Fields?.Any() == true || formField.Repetitions?.Any() == true,
                ProcessId = process.Id,
                Value = formField.Value,
                IsPublic = formField.IsPublic,
                MpriId = process.MpriId,
                RegisterNumber = process.RegisterNumber!
            };
            await Repo.AddAsync(registerItem);
        }

        public ProcessStepVM ToProcessStepVM(Guid processId, int serviceStepId, int orderNum, FormViewModel formModel)
        {
            return new ProcessStepVM
            {
                ProcessId = processId,
                ServiceStepId = serviceStepId,
                FormFields = formModel.FormFields,
                FormParentId = formModel.FormParentId,
                FormTitle = formModel.FormTitle,
                Purpose = formModel.Purpose,
                SelectedType = formModel.SelectedType,
                OrderNum = orderNum
            };
        }
    }
}

