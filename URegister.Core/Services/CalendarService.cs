using DataTables.AspNet.Core;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NodaTime.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Core.Contracts;
using URegister.Core.Data;
using URegister.Core.Models;
using URegister.Infrastructure.Extensions;
using URegister.RegistersCatalog;
namespace URegister.Core.Services
{
    public class CalendarService(RegistersCatalogGrpc.RegistersCatalogGrpcClient registerGrpcClient) : ICalendarService
    {
        public async Task<IActionResult> GetCalendarList(IDataTablesRequest request, CalendarFilterVM filter)
        {
            var result = await registerGrpcClient.GetCalendarDayListAsync(
                new CalendarDayListRequest
                {
                    DataTableRequest = request!.GetDataTablesRequestProto(),
                    FromDate = filter.DateFrom.Date.ToUniversalTime().ToTimestamp(),
                    ToDate = filter.DateTo.Date.ToUniversalTime().ToTimestamp(),
                });
            return request.GetResponseServerPaging(result.Data, result.CountAll);
        }

        public async Task<CalendarVM> GetCalendarVM(int id)
        {
            var result = await registerGrpcClient.GetCalendarDayAsync(new CalendarDayRequest { Id = id });
            return new CalendarVM
            {
                Id = result.Data.Id,
                CurrentDate = result.Data.CurrentDate.ToDateTime().ConvertUtcToBGTime(),
                KindId = result.Data.KindId,
                Description = result.Data.Description,
            };
        }
        public async Task SaveCalendar(CalendarVM model)
        {
            var result = await registerGrpcClient.SaveCalendarDayAsync(new CalendarDayItem
            {
                Id = model.Id,
                CurrentDate = model.CurrentDate.ToUniversalTime().ToTimestamp(),
                KindId = model.KindId,
                Description = model.Description,
            });
        }
    }
}

