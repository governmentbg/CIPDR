using Core.Services;
using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using URegister.Core.Contracts;
using URegister.Core.Data;
using URegister.Core.Data.Models.Common;
using URegister.Core.Models.Service;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Extensions;
using URegister.ObjectsCatalog;
using static URegister.ObjectsCatalog.ObjectsCatalogGrpc;

namespace URegister.Core.Services
{
    public class ServiceService : BaseService, IServiceService
    {
        private readonly ObjectsCatalogGrpcClient serviceGrpcClient;
        public ServiceService(
           IApplicationRepository repo,
           ILogger<BaseService> logger,
           ObjectsCatalogGrpcClient serviceGrpcClient
        ) : base(repo, logger)
        {
            this.serviceGrpcClient = serviceGrpcClient;
        }

        public async Task<IActionResult> GetServiceList(IDataTablesRequest request)
        {
            var nomTypes = new[] {
                InternalNomenclatureTypes.PersonType,
            };
            var serviceTypeResponse = await serviceGrpcClient.GetServiceTypesAsync(new Common.DatatableRequest { Length = -1 });
            var serviceType = serviceTypeResponse.ServiceTypes;
            var data = await Repo.AllReadonly<Service>()
                          .Select(x => new ServiceListItemVM
                          {
                              Id = x.Id,
                              Title = x.Title,
                              ServiceTypeId = x.ServiceTypeId,
                          })
                          .TagWith(nameof(GetServiceList))
                          .ToListAsync();
            data.ForEach(x => x.ServiceType = serviceType.Where(s => s.Id == x.ServiceTypeId).Select(s => s.Name).FirstOrDefault());
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

        public async Task<List<SelectListItem>> GetServiceDDL()
        {
            var ddl = await Repo.AllReadonly<Service>()
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
        public async Task AppendUpdate(ServiceVM model)
        {
            var service = new Service();
            var response = await serviceGrpcClient.GetServiceTypeAsync(new GetServiceTypeRequest { ServiceId = model.ServiceTypeId });
            var serviceSteps = response.ServiceType.Steps;
            if (model.Id > 0)
            {
                service = await Repo.All<Service>()
                                    .Include(x => x.ServiceSteps)
                                    .Where(x => x.Id == model.Id)
                                    .TagWith(nameof(AppendUpdate))
                                    .FirstAsync();
            }
            else
            {
                await Repo.AddAsync(service);
            }
            service.Title = model.Name;
            service.ServiceTypeId = model.ServiceTypeId;
            foreach (var step in service.ServiceSteps)
            {
                if (model.Steps.All(x => x.Id != step.Id))
                {
                    Repo.Delete(step);
                }
                else
                {
                    var stepVm = model.Steps
                        .First(x => x.Id == step.Id);
                    step.StepId = stepVm.StepId;
                    step.OrderNum = stepVm.OrderNum;
                    step.FormParentId = stepVm.FormParentId;
                    step.Title = serviceSteps.Where(x => x.Id == step.StepId).Select(x => x.Label).FirstOrDefault();
                }
            }
            foreach (var stepVm in model.Steps)
            {
                if (stepVm.Id == 0 || service.ServiceSteps.All(x => x.Id != stepVm.Id))
                {
                    var step = new ServiceStep
                    {
                        StepId = stepVm.StepId,
                        OrderNum = stepVm.OrderNum,
                        FormParentId = stepVm.FormParentId
                    };
                    step.Title = serviceSteps.Where(x => x.Id == step.StepId).Select(x => x.Label).FirstOrDefault();
                    service.ServiceSteps.Add(step);
                }
            }
            await Repo.SaveChangesAsync();
        }

        public async Task<ServiceVM> GetService(int id)
        {
            return await Repo.AllReadonly<Service>()
                             .Where(x => x.Id == id)
                             .Select(x => new ServiceVM
                             {
                                 Id = x.Id,
                                 ServiceTypeId = x.ServiceTypeId,
                                 Name = x.Title,
                                 Steps = x.ServiceSteps.Select(s => new ServiceStepVM
                                 {
                                     Id = s.Id,
                                     StepId = s.StepId,
                                     FormParentId = s.FormParentId,
                                     OrderNum = s.OrderNum
                                 })
                                 .OrderBy(s=> s.OrderNum)
                                 .ToList()
                             })
                             .TagWith(nameof(GetService))
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
        public async Task<List<SelectListItem>>GetStepDDL() {
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
    }  
}
