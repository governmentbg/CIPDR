using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using URegister.Common;
using URegister.RegistersCatalog.Data.Models;

namespace URegister.RegistersCatalog.Contracts
{
    public interface IRegisterInfoService
    {
        Task<string> AddMasterPersonRecordIndex(MasterPersonRecordIndexAddMessage request);

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
        Task<List<MPRILisItemMessage>> GetMasterPersonRecordIndex(GetMasterPersonRecordIndexMessage request);
        Task<List<MPRILisItemMessage>> GetMasterPersonRecordIndexList(List<Guid> ids);

        /// <summary>
        /// Списък с всички активни администрации.
        /// </summary>
        /// <returns></returns>
        Task<List<Administration>> GetAdministrations();

        /// <summary>
        /// Връща администрация по идентификатор.
        /// </summary>
        /// <param name="administrationId"></param>
        /// <returns></returns>
        Task<Administration> GetAdministrationById(Guid administrationId);

        Task<ICollection<RegisterItem>> GetAdministrationRegistries(Guid administrationId);

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
        
        /// <summary>
        /// Премахване на администрация от регистър
        /// </summary>
        /// <param name="registerAdministrationId">Идентификатор на регистърната администрация</param>
        /// <returns></returns>
        public Task<ResultStatus> RemoveAdministrationFromRegister(Guid registerAdministrationId);

        /// <summary>
        /// Връща администрация за регистрация на глобален администратор.
        /// </summary>
        /// <returns></returns>
        Task<Administration> GetAdminAdministration();
        
        /// <summary>
        /// Връща регистер по код
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<RegisterItem?> GetRegisterByCode(GetRegisterByCodeRequest request);
        Task<IList<AppAdministration>> GetAdministrationsByIds(RepeatedField<string> ids, int registerId);
        Task AddRegisterStatus(RegisterStatusItem request);
        Task<Guid> UploadFile(byte[] filesAsBytes, string fileName, string contentType, int sourceTypeId, Guid sourceId);
        Task SetRegisterAsStarted(int registerId);
        Task<List<AdministrationUicItem>> GetAdministrationUicList();
        Task<List<ServiceItem>> GetServiceList();
        Task SaveService(ServiceItem request);
        Task<(RegisterFileMetadata, byte[], string)> DownloadFile(Guid id);
        Task<List<RegisterStatusItem>> GetRegisterStatusList(int registerId);
        Task<AppAdministration> GetAdministrationNameByUic(StringValue uic);
        Task SaveCalendarDay(CalendarDayItem request);
        Task<(List<CalendarDayItem>, int)> GetCalendarDayList(CalendarDayListRequest request);
        Task<CalendarDayItem> GetCalendarDay(int id);
        Task<DateTime> CalcWorkDays(DateTime dateFrom, int days);
        Task<RegisterStatusItem> GetRegisterStatus(Guid id);
        Task<OpenDataParam> GetOpenDataParam(OpenDataParamRequest request);
        Task SaveOpenDataRegister(OpenDataRegisterSaveRequest request);
        Task SaveOpenDataAdministration(OpenDataAdministrationSaveRequest request);
        Task SaveOpenDataRegisterAdministration(OpenDataRegisterAdministrationSaveRequest request);
        Task SaveOpenDataRegisterAdministrationMeta(OpenDataRegisterAdministrationMetaSaveRequest request);
    }
}
