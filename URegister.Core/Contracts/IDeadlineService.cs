using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Core.Models.Deadline;

namespace URegister.Core.Contracts
{
    public interface IDeadlineService : IBaseService
    {
        Task<DeadlineVM> GetDeadline(int id);
        Task<List<SelectListItem>> GetDeadlineDDL(int serviceId);
        Task<IActionResult> GetDeadlineList(IDataTablesRequest request);
        Task SaveDeadline(DeadlineVM model);
    }
}
