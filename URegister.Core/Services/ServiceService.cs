using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq.Expressions;
using URegister.Common;
using URegister.Core.Contracts;
using URegister.Core.Data;
using URegister.Core.Data.Models.Common;
using URegister.Core.Models.Service;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Extensions;
using URegister.Infrastructure.Model.RegisterForms;
using URegister.Infrastucture.Extensions;
using URegister.NomenclaturesCatalog;
using URegister.ObjectsCatalog;
using static FastExpressionCompiler.ExpressionCompiler;
using static URegister.ObjectsCatalog.ObjectsCatalogGrpc;

namespace URegister.Core.Services
{
    public class ServiceService : BaseService, IServiceService
    {
        private readonly ObjectsCatalogGrpcClient serviceGrpcClient;
        private readonly INomenclatureClientService nomenclatureClientService;
        public ServiceService(
           IApplicationRepository repo,
           ILogger<BaseService> logger,
           ObjectsCatalogGrpcClient serviceGrpcClient,
           INomenclatureClientService nomenclatureClientService
        ) : base(repo, logger)
        {
            this.serviceGrpcClient = serviceGrpcClient;
            this.nomenclatureClientService = nomenclatureClientService;
        }

        public async Task<IActionResult> GetServiceList(IDataTablesRequest request)
        {
            var serviceTypeResponse = await serviceGrpcClient.GetServiceTypesAsync(new Common.DatatableRequest { Length = -1 });
            var serviceType = serviceTypeResponse.ServiceTypes;
            var data = await Repo.AllReadonly<Service>()
                          .Select(x => new ServiceListItemVM
                          {
                              Id = x.Id,
                              Title = x.Title,
                              ServiceTypeId = x.ServiceTypeId,
                              FormParentId = x.FormParentId
                          })
                          .TagWith(nameof(GetServiceList))
                          .ToListAsync();
            var forms = await Repo.AllReadonly<Form>()
                                  .ToListAsync();
            foreach (var item in data)
            {
                item.ServiceType = serviceType.Where(s => s.Id == item.ServiceTypeId).Select(s => s.Name).FirstOrDefault();
                item.FormName = forms.Where(s => s.ParentId == item.FormParentId).Select(s => s.Title).FirstOrDefault();
            }

            return request.GetResponse(data.AsQueryable());
        }
        private void AddChoose(List<SelectListItem> ddl)
        {
            ddl.Insert(0, new SelectListItem
            {
                Text = "Изберете",
                Value = null
            });

        }
        public async Task<List<SelectListItem>> GetServiceTypeDDL()
        {
            var response = await serviceGrpcClient.GetServiceTypesAsync(new Common.DatatableRequest { Length = -1 });
            var ddl = response.ServiceTypes
                .Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                })
                .ToList();
            AddChoose(ddl);
            return ddl;
        }

        public async Task<List<SelectListItem>> GetServiceStepDDL(int serviceTypeId)
        {
            var ddl = new List<SelectListItem>();
            if (serviceTypeId > 0)
            {
                var response = await serviceGrpcClient.GetServiceTypeAsync(new GetServiceTypeRequest { ServiceId = serviceTypeId });
                ddl = response.ServiceType.Steps
                    .Where(x => x.Value)
                    .Select(x => new SelectListItem
                    {
                        Text = x.Label,
                        Value = x.Id.ToString()
                    })
                    .ToList();
            }
            AddChoose(ddl);
            return ddl;
        }

        public async Task<List<SelectListItem>> GetServiceDDL(List<int> serviceTypes)
        {
            Expression<Func<Service, bool>> filterService = x => true;
            if (serviceTypes?.Any() == true)
            {
                filterService = x => serviceTypes.Contains(x.ServiceTypeId);
            }

            var ddl = await Repo.AllReadonly<Service>()
                    .Where(filterService)
                    .Select(x => new SelectListItem
                    {
                        Text = x.Title,
                        Value = x.Id.ToString()
                    })
                    .TagWith(nameof(GetServiceDDL))
                    .ToListAsync();
            AddChoose(ddl);
            return ddl;
        }

        private void SetStepFromVm(ServiceStep step, ServiceStepVM stepVm, ICollection<CheckListItem> serviceSteps)
        {
            step.StepId = stepVm.StepId;
            step.OrderNum = stepVm.OrderNum;
            step.StatusId = stepVm.StatusId;
            if (step.StepId == (int)ServiceSteps.Coordination)
            {
                step.Title = stepVm.Name;
            }
            else
            {
                step.Title = serviceSteps.Where(x => x.Id == step.StepId).Select(x => x.Label).FirstOrDefault();
            }
            foreach (var stepRole in step.StepRoles)
            {
                var stepRoleVM = stepVm.Roles.Where(x => x == stepRole.RoleId).FirstOrDefault();
                if (stepRoleVM == Guid.Empty)
                {
                    Repo.Delete(stepRole);
                    continue;
                }
                stepRole.IsActive = true;
            }
            foreach (var stepRoleVM in stepVm.Roles)
            {
                var stepRole = step.StepRoles.Where(x => x.RoleId == stepRoleVM).FirstOrDefault();
                if (stepRole == null)
                {
                    step.StepRoles.Add(new StepRole
                    {
                        IsActive = true,
                        RoleId = stepRoleVM,
                        ServiceStepId = step.StepId,
                    });
                }
            }
        }

        public async Task<OperationResult> AppendUpdate(ServiceVM model)
        {
            var service = new Service();
            var response = await serviceGrpcClient.GetServiceTypeAsync(new GetServiceTypeRequest { ServiceId = model.ServiceTypeId });
            var serviceSteps = response.ServiceType.Steps;
            if (model.Id > 0)
            {
                if (await Repo.AllReadonly<Service>().AllAsync(s => s.Id != model.Id))
                {
                    return new OperationResult("Не е намерена услуга с този идентификатор");
                    //TODO : log
                }

                if (await Repo.AllReadonly<Service>().AnyAsync(s => EF.Functions.ILike(s.Title, model.Name)
                    && s.Id != model.Id))
                {
                    return new OperationResult("Услуга с такова име вече съществува");
                }

                if (model.ServiceTypeId == (int)ServiceTypes.Register && await Repo.AllReadonly<Service>()
                        .AnyAsync(s => s.ServiceTypeId == model.ServiceTypeId
                                       && s.Id != model.Id))
                {
                    return new OperationResult($"Услуга от тип {ServiceTypes.Register.GetDescription()} вече съществува");
                    //TODO : log
                }

                service = await Repo.All<Service>()
                                    .IgnoreQueryFilters()
                                    .Include(x => x.ServiceSteps)
                                    .ThenInclude(x => x.StepRoles)
                                    .Where(x => x.Id == model.Id)
                                    .TagWith(nameof(AppendUpdate))
                                    .FirstAsync();
            }
            else
            {
                if (await Repo.AllReadonly<Service>().AnyAsync(s => EF.Functions.ILike(s.Title, model.Name)))
                {
                    return new OperationResult("Услуга с такова име вече съществува");
                }

                if (model.ServiceTypeId == (int)ServiceTypes.Register && await Repo.AllReadonly<Service>()
                        .AnyAsync(s => s.ServiceTypeId == model.ServiceTypeId))
                {
                    return new OperationResult($"Услуга от тип {ServiceTypes.Register.GetDescription()} вече съществува");
                    //TODO : log
                }
                await Repo.AddAsync(service);
            }

            service.Title = model.Name;
            service.ServiceTypeId = model.ServiceTypeId;
            service.FormParentId = model.FormParentId;
            service.EFormCode = model.EFormCode;
            int orderNum = 1;
            foreach (var stepVm in model.Steps)
            {
                stepVm.OrderNum = orderNum;
                orderNum++;
            }
            foreach (var step in service.ServiceSteps.Where(x => x.IsActive))
            {
                var stepVm = model.Steps.FirstOrDefault(x => x.Id == step.Id);
                if (stepVm == null)
                {
                    Repo.Delete(step);
                    continue;
                }
                SetStepFromVm(step, stepVm, serviceSteps);
            }
            
            foreach (var stepVm in model.Steps)
            {
                if (stepVm.Id == 0)
                {
                    var step = new ServiceStep
                    {
                        StepId = stepVm.StepId,
                    };
                    SetStepFromVm(step, stepVm, serviceSteps);
                    service.ServiceSteps.Add(step);
                }
            }

            await Repo.SaveChangesAsync();
            model.Id = service.Id;
            return new OperationResult();
        }

        public async Task<ServiceVM> GetService(int id, bool ignoreSoftDeletedSteps = false)
        {
            return await Repo.AllReadonly<Service>()
                             .IgnoreQueryFilters()   
                             .Where(x => x.Id == id)
                             .Select(x => new ServiceVM
                             {
                                 Id = x.Id,
                                 ServiceTypeId = x.ServiceTypeId,
                                 Name = x.Title,
                                 FormParentId = x.FormParentId,
                                 EFormCode = x.EFormCode,
                                 Steps = x.ServiceSteps
                                     .Where(s => !ignoreSoftDeletedSteps || s.IsActive)
                                     .Select(s => new ServiceStepVM
                                 {
                                     Id = s.Id,
                                     StepId = s.StepId,
                                     OrderNum = s.OrderNum,
                                     StatusId = s.StatusId,
                                     Name = s.Title ?? string.Empty,
                                     Roles = s.StepRoles.Where(x => x.IsActive).Select(x => x.RoleId).ToList()
                                 })
                                 .OrderBy(s => s.OrderNum)
                                 .ToList()
                             })
                             .TagWith(nameof(GetService))
                             .FirstAsync();
        }
        
        public async Task<ServiceVM> GetRegisterService()
        {
            return await Repo.AllReadonly<Service>()
                             .Where(x => x.ServiceTypeId == (int)ServiceTypes.Register)
                             .Select(x => new ServiceVM
                             {
                                 Id = x.Id,
                                 ServiceTypeId = x.ServiceTypeId,
                                 Name = x.Title,
                                 FormParentId = x.FormParentId,
                                 Steps = x.ServiceSteps.Select(s => new ServiceStepVM
                                 {
                                     Id = s.Id,
                                     StepId = s.StepId,
                                     OrderNum = s.OrderNum,
                                     StatusId = s.StatusId,
                                     Name = s.Title ?? string.Empty,
                                     Roles = s.StepRoles.Select(x => x.RoleId).ToList()
                                 })
                                 .OrderBy(s => s.OrderNum)
                                 .ToList()
                             })
                             .TagWith(nameof(GetRegisterService))
                             .SingleOrDefaultAsync();
        }

        public async Task<Form> GetForm(int formParentId)
        {
            return await Repo.AllReadonly<Form>()
                             .Where(x => x.ParentId == formParentId)
                             .TagWith(nameof(GetForm))
                             .FirstAsync();
        }
        public async Task<ServiceStep> GetServiceStep(int id)
        {
            return await Repo.AllReadonly<ServiceStep>()
                             .Where(x => x.Id == id)
                             .TagWith(nameof(GetServiceStep))
                             .FirstAsync();
        }

        public async Task<List<ServiceStep>> GetServiceSteps(int serviceId)
        {
            return await Repo.AllReadonly<ServiceStep>()
                .TagWith(nameof(GetServiceSteps))
                .Where(x => x.ServiceId == serviceId).OrderBy(x => x.OrderNum).ToListAsync();
        }
        public async Task<List<SelectListItem>> GetStepDDL()
        {
            var result = await serviceGrpcClient.GetStepListAsync(new Common.DatatableRequest { Length = -1 });
            var ddl = result.Steps
                    .Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    })
                    .ToList();
            AddChoose(ddl);
            return ddl;
        }

        /// <summary>
        /// Изтриване на услуга
        /// </summary>
        /// <param name="id">Идентификатор на услугата за изтриване</param>
        /// <returns></returns>
        public async Task<OperationResult> Delete(int id)
        {
            try
            {
                Service serviceToDelete = await Repo.All<Service>()
                    .TagWith(nameof(Delete))
                    .Include(s => s.ServiceSteps)
                    .ThenInclude(s => s.Processes)
                    .SingleOrDefaultAsync(s => s.Id == id);

                if (serviceToDelete == null)
                {
                    Logger.LogError($"Услуга с идентификатор {id} не е намерена в {nameof(Delete)}");
                    return new OperationResult($"Услуга с идентификатор {id} не е намерена");
                }

                var processesUsingConnectedServiceStep = 
                    serviceToDelete.ServiceSteps.SelectMany(ss => ss.Processes);

                if (processesUsingConnectedServiceStep.Any())
                {
                    Logger.LogWarning($"Съществуват заявени услуги свързани с услугата, за която има опит за изтриване. Изтриването не е разрешено. Идентификатор на услуга{id}");
                    return new OperationResult($"Съществуват заявени услуги свързани с услугата, която се опитвате да изтриете. Изтриването не е разрешено");
                }

                Repo.DeleteRange(serviceToDelete.ServiceSteps);
                Repo.Delete(serviceToDelete);
                await Repo.SaveChangesAsync();

                return new OperationResult();
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Грешка при триене на услуга с идентификатор {id} в {nameof(Delete)}");
                return new OperationResult($"Грешка при триене на услуга с идентификатор {id}");
            }
        }

        public async Task<IActionResult> GetBlankTemplateList(IDataTablesRequest request)
        {
            var nomTypes = new[] {
                InternalNomenclatureTypes.BlankSourceType,
            };
            var nomenclatureTypes = await nomenclatureClientService.GetNomenclaturePublic(0, nomTypes);
            var forms = Repo.AllReadonly<Form>();
            var data = Repo.AllReadonly<BlanksTemplate>()
                          .IgnoreQueryFilters()
                          .Where(x => x.IsActive)
                          .Select(x => new BlanksTemplateVM
                          {
                              Id = x.Id,
                              Name = x.Name,
                              Code = x.Code,
                              ServiceId = x.ServiceId ,
                              FormParentId = x.FormParentId,
                              SourceType = x.SourceType,
                              ServiceName = x.Service.Title,
                              FormName = forms.Where(s => s.ParentId == x.FormParentId).Select(s => s.Title).FirstOrDefault(),
                              HasRegisterNumber = x.HasRegisterNumber,
                              HasStamp = x.HasStamp,
                          })
                          .TagWith(nameof(GetServiceList));
            var countAll = 0;
            (data, countAll) = request.GetResponseData(data);
            var list = data.ToList();
            foreach (var item in list) {
                item.SourceTypeName = nomenclatureClientService.GetNomenclatureValue(nomenclatureTypes, InternalNomenclatureTypes.BlankSourceType, item.SourceType.ToString());
            }

            
            return request.GetResponseJson(list.AsQueryable(), countAll);
        }

        public async Task AppendUpdate(BlanksTemplateVM model)
        {
            var data = new BlanksTemplate();
            if (model.Id > 0)
            {
                data = await Repo.All<BlanksTemplate>()
                                 .Include(x => x.BlankSignatures)
                                 .Where(x => x.Id == model.Id)
                                 .FirstAsync();
            }
            else
            {
                await Repo.AddAsync(data);
            }
            data.Name = model.Name;
            data.Code = model.Code;
            data.ServiceId = model.ServiceId;
            data.FormParentId = model.FormParentId;
            data.SourceType = model.SourceType;
            data.HasRegisterNumber = model.HasRegisterNumber;
            data.HasStamp = model.HasStamp;
            data.IsActive = true;
            foreach (var signature in data.BlankSignatures)
            {
                signature.IsActive = model.BlankSignatures.Any(x => x.Id == signature.Id);
            }
            foreach (var from in model.BlankSignatures)
            {
                var signature = data.BlankSignatures.Where(x => x.Id == from.Id).FirstOrDefault();
                if (signature == null)
                {
                    signature = new BlankSignature();
                    data.BlankSignatures.Add(signature);
                }
                signature.SignByOperator = from.SignByOperator;
                signature.RoleId = from.RoleId;
                signature.OrderNum = from.OrderNum;
            }
            var orderNum = 1;
            foreach (var signature in data.BlankSignatures.Where(x => x.IsActive).OrderBy(x => x.OrderNum))
            {
                signature.OrderNum = orderNum;
                orderNum++;
            }
            await Repo.SaveChangesAsync();
        }

        public async Task AppendUpdateContent(BlanksTemplateContentVM model)
        {
            var data = await Repo.All<BlanksTemplate>()
                                 .Where(x => x.Id == model.Id)
                                 .FirstAsync();
            data.Content = model.Content;
            await Repo.SaveChangesAsync();
        }
        public async Task<BlanksTemplateVM> GetBlankTemplate(int id)
        {
            return await Repo.AllReadonly<BlanksTemplate>()
                          .Where(x => x.Id == id)
                          .Select(x => new BlanksTemplateVM
                          {
                              Id = x.Id,
                              Name = x.Name,
                              Code = x.Code,
                              ServiceId = x.ServiceId,
                              FormParentId = x.FormParentId,
                              SourceType = x.SourceType,
                              HasRegisterNumber = x.HasRegisterNumber,
                              HasStamp = x.HasStamp,
                              BlankSignatures = x.BlankSignatures.Select(s => new BlankSignatureVM
                              {
                                  Id = s.Id,
                                  OrderNum = s.OrderNum,
                                  RoleId = s.RoleId,
                                  SignByOperator = s.SignByOperator,
                              }).ToList()
                          })
                          .TagWith(nameof(GetBlankTemplate))
                          .FirstAsync();
        }

        public async Task<BlanksTemplateContentVM> GetBlankTemplateContent(int id)
        {
            return await Repo.AllReadonly<BlanksTemplate>()
                          .Where(x => x.Id == id)
                          .Select(x => new BlanksTemplateContentVM
                          {
                              Id = x.Id,
                              Name = x.Name,
                              FormParentId = x.FormParentId,
                              ServiceId = x.ServiceId,
                              SourceType= x.SourceType,
                              HasRegisterNumber= x.HasRegisterNumber,
                              Content = x.Content
                          })
                          .TagWith(nameof(GetBlankTemplateContent))
                          .FirstAsync();
        }

        /// <summary>
        /// Изтрива бланка по идентификатор
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<OperationResult> DeleteTemplate(int id)
        {
            try
            {
                var template = await Repo.All<BlanksTemplate>()
                    .TagWith(nameof(DeleteTemplate))
                    .IgnoreQueryFilters()
                    .SingleOrDefaultAsync(f => f.Id == id);

                if (template == null)
                {
                    return new OperationResult($"Активна бланка с идентификатор {id} не е открита");
                }
                template.IsActive = false;
                await Repo.SaveChangesAsync();
                return new OperationResult();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Проблем при изтриване на формата с id {id}");
                return new OperationResult("Проблем при изтриване на формата");
            }
        }

        public List<BlanksTemplateParamVM> GetTemplateProcessParam(string prefix)
        {
            var paramList = new List<BlanksTemplateParamVM>();
            paramList.Add(new BlanksTemplateParamVM
            {
                Label = "Входящ номер",
                Name = $"{prefix}Process_IncomingNumber",
            });
            paramList.Add(new BlanksTemplateParamVM
            {
                Label = "Дата на входиране",
                Name = $"{prefix}Process_IncomingDate",
            });
            paramList.Add(new BlanksTemplateParamVM
            {
                Label = "Номер на вписване",
                Name = $"{prefix}Process_RegisterNumber",
            });
            return paramList;
        }

        public async Task<List<BlanksTemplateParamVM>> GetTemplateParam(FormViewModel formModel, string prefix)
        {
            var paramList = formModel.FormFields.Select(x => new BlanksTemplateParamVM
            {
                Label = x.Label,
                Name = $"{prefix}{x.Name}",
                Type = x.Type,
                Templates = x.Fields?.Select(f => new BlanksTemplateParamVM
                {
                    Label = f.Label,
                    Name = $"{prefix}{f.Name}",
                    Type = f.Type,
                }).ToList()
            })
           .ToList();
            var response = await serviceGrpcClient.GetFieldTemplateListAsync(new FieldTemplateListRequest
            {
                DataTableRequest = new DatatableRequest { Length = -1 }
            });
            foreach (var param in paramList)
            {
                if (param.Templates?.Any() == true)
                {
                    param.Templates.AddRange(response.FieldTemplates
                                              .Where(x => x.FieldType == param.Type)
                                              .Select(x => new BlanksTemplateParamVM
                                              {
                                                  Name = $"{prefix}{param.Name}_FieldTemplate{x.Id}",
                                                  Label = x.Name,
                                              })
                                              .ToList());
                }
                if (param.Templates?.Any() != true)
                {
                    param.Templates = null;
                }
            }
            return paramList;
        }
    }
}