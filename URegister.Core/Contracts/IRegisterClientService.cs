using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Core.Models.Register;

namespace URegister.Core.Contracts
{
    public interface IRegisterClientService
    {
        Task<(bool, string)> AddRegister(RegisterVM register);
        Task<RegisterVM> CreateRegister();
        Task<IActionResult> GetAdministrationList(IDataTablesRequest request, AdministrationFilterVM filter);
        Task<IActionResult> GetPersonList(IDataTablesRequest request, PersonFilterVM filter);
        Task<RegisterVM> GetRegisterForAddAdministration(int registerId);
        Task<IActionResult> GetRegisterFullList(IDataTablesRequest request, RegisterFilterVM filter);
    }
}
