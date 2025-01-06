using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Core.Data.Models.Common;
using URegister.Core.Models.Process;
using URegister.Core.Models.Service;

namespace URegister.Core.Contracts
{
    public interface IServiceService
    {
        Task AppendUpdate(ServiceVM model);
        Task<ServiceVM> GetService(int id);
        Task<List<SelectListItem>> GetServiceDDL();
        Task<IActionResult> GetServiceList(IDataTablesRequest request);
        Task<ServiceStep> GetServiceStep(int id);
        Task<List<SelectListItem>> GetServiceStepDDL(int serviceTypeId);
        Task<List<SelectListItem>> GetServiceTypeDDL();

        Task<List<ServiceStep>> GetServiceSteps(int serviceId);

        Task<List<SelectListItem>> GetStepDDL();
    }
}
