using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Core.Models.CurrentRegister;
using URegister.RegistersCatalog;

namespace URegister.Core.Contracts
{
    public interface IRegisterService
    {
        Task<AdministrationVM> GetAdministration(Guid administrationId);
        Task<IActionResult> GetAdministrationList(IDataTablesRequest request);
        Task<RegisterVM> GetCurrentRegister();
        Task<int> GetCurrentRegisterId();
        Task<List<SelectListItem>> GetRegisterNotStartedDdl();
        Task SaveAdministration(AdministrationVM model);
        Task SaveRegister(RegisterVM model);
        Task StartRegister(int registerId);
    }
}
