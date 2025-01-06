using URegister.Common;

namespace URegister.RegistersCatalog.Contracts
{
    public interface IRegisterInfoService
    {
        Task<string> AddMasterPersonRecordsIndex(MasterPersonRecordsIndexAddMessage request);

        /// <summary>
        /// Добавяне на регистър
        /// </summary>
        /// <param name="request">данни за регистър</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        Task AddRegister(RegisterItem request);
        Task<RegisterItem> CreateRegister();

        /// <summary>
        /// Списък администрации
        /// </summary>
        /// <param name="request">идентификатор на регистър</param>
        /// <returns>администрации</returns>
        Task<(List<AdministrationListItem>, int)> GetAdministrationList(AdministrationListRequest request);

        /// <summary>
        /// Списък лица към администрация
        /// </summary>
        /// <param name="request">идентификатор на администрация</param>
        /// <returns></returns>
        Task<(List<PersonListItem>, int)> GetPersonList(PersonListRequest request);
        Task<RegisterItem> GetRegister(int registerId);

        /// <summary>
        /// Регистри за администрация
        /// </summary>
        /// <param name="registerId">идентификатор</param>
        /// <returns>регистър</returns>

        Task<RegisterItem> GetRegisterForAddAdministration(int registerId);

        /// <summary>
        /// Страницирани данни за datatables с регистри
        /// </summary>
        /// <param name="request">datatables филтър</param>
        /// <returns>Данни за datatables с регистри</returns>
        Task<(List<RegisterListItem>, int)> GetRegisterFullList(RegisterListRequest request);
        
        /// <summary>
        /// Списък регистри за checklist
        /// </summary>
        /// <returns></returns>
        Task<List<ListItem>> GetRegisterList();
        Task<List<ListItem>> GetRegisterNotStartedList();
        Task SetRegisterAsStarted(int registerId);
    }
}
