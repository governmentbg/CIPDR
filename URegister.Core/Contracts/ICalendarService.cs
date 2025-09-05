using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Core.Models;

namespace URegister.Core.Contracts
{
    public interface ICalendarService
    {
        Task<IActionResult> GetCalendarList(IDataTablesRequest request, CalendarFilterVM filter);
        Task<CalendarVM> GetCalendarVM(int id);
        Task SaveCalendar(CalendarVM model);
    }
}
