using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Core.Contracts;
using URegister.Core.Data;
using URegister.Core.Data.Models.Common;
using URegister.Core.Models.Deadline;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Extensions;

namespace URegister.Core.Services
{
    public class DeadlineService : BaseService, IDeadlineService
    {
        private readonly INomenclatureClientService nomenclatureClientService;
        private readonly ILogger<DeadlineService> logger;
        private readonly IUserContext userContext;
        public DeadlineService(
            IApplicationRepository repo,
            ILogger<DeadlineService> logger,
            INomenclatureClientService nomenclatureClientService,
            IUserContext userContext
        ) : base(repo, logger)
        {
            this.nomenclatureClientService = nomenclatureClientService;
            this.logger = logger;
            this.userContext = userContext;
        }
        public async Task<IActionResult> GetDeadlineList(IDataTablesRequest request)
        {
            var nomTypes = new[] {
                InternalNomenclatureTypes.DeadlineType,
                InternalNomenclatureTypes.DeadlineDayType
            };
            var nomenclatureTypes = await nomenclatureClientService.GetNomenclaturePublic(0, nomTypes);
            var data = Repo.AllReadonly<DeadlineDay>()
                          .Select(x => new DeadlineVM
                          {
                              Id = x.Id,
                              DayTypeId = x.DayTypeId,
                              DeadlineTypeId = x.DeadlineTypeId,
                              Days = x.Days,
                              ServiceId = x.ServiceId ?? 0,
                              ServiceName = x.Service.Title
                          })
                          .TagWith(nameof(GetDeadlineList));
            var countAll = 0;
            (data, countAll) = request.GetResponseData(data);
            var list = data.ToList();
            foreach (var item in list)
            {
                item.DeadlineType = nomenclatureClientService.GetNomenclatureValue(nomenclatureTypes, InternalNomenclatureTypes.DeadlineType, item.DeadlineTypeId.ToString());
                item.DayType = nomenclatureClientService.GetNomenclatureValue(nomenclatureTypes, InternalNomenclatureTypes.DeadlineDayType, item.DayTypeId.ToString());
            }
            return request.GetResponseJson(list.AsQueryable(), countAll);
        }

        public async Task<List<SelectListItem>> GetDeadlineDDL(int serviceId)
        {
            var nomTypes = new[] {
                InternalNomenclatureTypes.DeadlineType,
                InternalNomenclatureTypes.DeadlineDayType
            };
            var nomenclatureTypes = await nomenclatureClientService.GetNomenclaturePublic(0, nomTypes);
            var data = await Repo.AllReadonly<DeadlineDay>()
                          .Where(x => x.ServiceId == serviceId)
                          .Select(x => new DeadlineVM
                          {
                              Id = x.Id,
                              DayTypeId = x.DayTypeId,
                              DeadlineTypeId = x.DeadlineTypeId,
                              Days = x.Days,
                          })
                          .TagWith(nameof(GetDeadlineList))
                          .ToListAsync();
            foreach (var item in data)
            {
                item.DeadlineType = nomenclatureClientService.GetNomenclatureValue(nomenclatureTypes, InternalNomenclatureTypes.DeadlineType, item.DeadlineTypeId.ToString());
                item.DayType = nomenclatureClientService.GetNomenclatureValue(nomenclatureTypes, InternalNomenclatureTypes.DeadlineDayType, item.DayTypeId.ToString());
            }
            var ddl = data.Select(x =>  new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = $"{x.DeadlineType} {x.Days} {x.DayType}"
            })
            .ToList();
            nomenclatureClientService.AddChoice(ddl);
            return ddl;
        }


        public async Task<DeadlineVM> GetDeadline(int id)
        {
            return await Repo.AllReadonly<DeadlineDay>()
                             .Where(x => x.Id == id)
                             .Select(x => new DeadlineVM
                             {
                                 Id = x.Id,
                                 DayTypeId = x.DayTypeId,
                                 DeadlineTypeId = x.DeadlineTypeId,
                                 Days = x.Days,
                                 ServiceId = x.ServiceId ?? 0,
                             })
                             .FirstAsync();
        }

        public async Task SaveDeadline(DeadlineVM model)
        {

            DeadlineDay data = null!;
            if (model.Id > 0)
            {
                data = await Repo.All<DeadlineDay>()
                                 .Where(x => x.Id == model.Id)
                                 .FirstAsync();
            }
            else
            {
                data = new DeadlineDay();
                await Repo.AddAsync(data);
            }
            data.ServiceId = model.ServiceId;
            data.DeadlineTypeId = model.DeadlineTypeId;
            data.DayTypeId = model.DayTypeId;
            data.Days = model.Days;
            data.ModifiedOn = DateTime.UtcNow;
            data.ModifiedByUserId = userContext.UserId;
            await Repo.SaveChangesAsync();
        }
    }
}
