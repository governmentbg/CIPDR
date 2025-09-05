using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using URegister.Core.Models.CurrentRegister;

namespace URegister.Core.Contracts
{
    public interface IRegisterService
    {
        Task<RegisterVM> GetCurrentRegister();
        Task<int> GetCurrentRegisterId();
        int GetCurrentRegisterIdForAudit();
        Task SaveRegister(RegisterVM model);
        Task<RegisterVM> StartRegister(string registerCode);
    }
}
