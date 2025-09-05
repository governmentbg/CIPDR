using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using URegister.Core.Models.Register;
using URegister.RegistersCatalog;
using URegister.Users;

namespace URegister.Core.Contracts
{
    public interface IRegisterClientService
    {
        Task<AppAdministrations> GetAllAdministrations();
        Task<GetAdministrationResponse> GetAdministrationById(string administrationId);
        Task<(bool, string)> AddRegister(RegisterVM register);
        Task<RegisterVM> CreateRegister();
        Task<IActionResult> GetAdministrationList(IDataTablesRequest request, AdministrationFilterVM filter);
        Task<IActionResult> GetPersonList(IDataTablesRequest request, PersonFilterVM filter);
        Task<RegisterVM> GetRegisterForAddAdministration(int registerId);
        Task<IActionResult> GetRegisterFullList(IDataTablesRequest request, RegisterFilterVM filter);
        Task<List<RegisterVM>> GetAllRegisterInAdministration(string administrationId);
        Task<RegisterVM> GetRegister(int registerId, Guid registerAdministrationId);
        Task<(bool, string)> EditRegister(RegisterVM register);

        /// <summary>
        /// Връща списък от всички регистри
        /// </summary>
        /// <returns></returns>
        public Task<RegisterFullListResponse> GetRegisterFullList();

        /// <summary>
        /// Връща брой на всички регистри
        /// </summary>
        /// <returns></returns>
        public Task<int> GetRegisterCount();
        Task<GetAdministrationResponse> GetAdminAdministration();
        Task<List<string>> FormatUserRoles(UserListData user);
        Task AddRegisterStatus(RegisterStatusVM model);
        Task<Guid?> UploadFile(IFormFile file, Guid sourceId, int sourceType);
        Task<(byte[], string, string)> DownloadFile(Guid id);
        Task<IActionResult> GetRegisterStatusList(IDataTablesRequest request, int registerId);
        Task<RegisterStatusVM> GetRegisterStatus(Guid registerStatusId);
    }
}
