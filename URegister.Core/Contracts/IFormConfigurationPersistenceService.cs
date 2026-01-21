using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using URegister.Core.Data.Models.Common;
using URegister.Core.Data.Models.Process;
using URegister.Core.Services;
using URegister.Infrastructure.Model.RegisterForms;
using static URegister.Core.Services.FormConfigurationPersistenceService;

namespace URegister.Core.Contracts
{
    public interface IFormConfigurationPersistenceService
    {
        /// <summary>
        /// Запазва нова форма
        /// </summary>
        /// <param name="model">Модел на формата</param>
        /// <returns></returns>
        public Task<SaveOperationResult> SaveForm(AddFormViewModel model);

        /// <summary>
        /// Връша списък с формите в регистър
        /// </summary>
        /// <param name="registerIndex">Идентификатор на регистър</param>
        /// <returns></returns>
        public Task<IEnumerable<object>> GetForms(int registerIndex);

        /// <summary>
        /// Връща списък с формите в регистър
        /// </summary>
        /// <param name="registerId">Идентификатор на регистър</param>
        /// <param name="approvalStatus">Статус на одобрение</param>
        /// <returns></returns>
        public Task<IActionResult> GetFormListDashboard(IDataTablesRequest request, int registerId, int approvalStatus);

        /// <summary>
        /// Връща модел на формата по родителски идентификатор
        /// </summary>
        /// <param name="formParentId"></param>
        /// <param name="allowUnapprovedConfiguration">Дали да зарежда и още неодобрени конфигурации</param>
        /// <returns></returns>
        public Task<FormViewModel> GetFormViewModel(int formParentId, bool allowUnapprovedConfiguration = false);

        /// <summary>
        /// Зарежда конфигурацията за форма
        /// </summary>
        /// <param name="formParentId">Идентификатор на родител на форма</param>
        /// <returns></returns>
        public Task<string> LoadDesignerJson(int formParentId);

        /// <summary>
        /// Записва JSON от дизайнера в базатта данни
        /// </summary>
        /// <returns></returns>        
        public Task<bool> SaveDesignerJson(string json, int formParentId, string formTitle, bool isApproved);

        /// <summary>
        /// Извличане на форма по идентификатор
        /// </summary>
        /// <param name="id">Идентификатор на форма</param>
        /// <returns></returns>
        public Task<Form> GetFormById(int id);

        /// <summary>
        /// Изтрива форма по идентификатор
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<OperationResult> DeleteForm(int id);

        /// <summary>
        /// Редакция на съществуваща форма
        /// </summary>
        /// <returns></returns>
        public Task<SaveOperationResult> EditForm(AddFormViewModel model);

        Task<List<SelectListItem>> GetFormsDDL();

        /// <summary>
        /// Връща модел на форма от записани данни
        /// </summary>
        /// <param name="mpris">Идентификатори на партида</param>
        /// <param name="processId">Идентификатор на заявлението</param>
        /// <returns></returns>
        public Task<FormViewModel> GetFormModelForSavedData(IEnumerable<Guid> mpris, Guid processId);

        /// <summary>
        /// Връща списък с всички услуги
        /// </summary>
        /// <param name="skip">Колко записа да бъдат пропуснати</param>
        /// <param name="take">Колко записа да бъдат взети</param>
        /// <returns></returns>
        public Task<JsonResult> GetAllServiceList(int skip, int take);

        /// <summary>
        /// Връща списък с приключени заявени услуги от тип Вписване, като всяко поле е отделна колона
        /// </summary>
        /// ///
        /// <param name="administrationId">Идентификатор на администрация</param>
        /// <param name="skip">Колко записа да бъдат пропуснати</param>
        /// <param name="take">Колко записа да бъдат взети</param>
        /// <param name="searchKey"></param>
        /// <param name="searchPattern"></param>
        /// <param name="toDate">До дата на вписване, включително</param>
        /// <param name="fromDate">От дата на вписване, включително</param>
        /// <returns></returns>
        public Task<JsonResult> GetRegistrationProcessListWhereSubfieldsAreColumns(Guid administrationId, int skip, int take, string searchKey,
            string searchPattern, DateTime? toDate, DateTime? fromDate);

        /// <summary>
        /// Връща списък с приключени заявени услуги от тип Вписване, като стойностите на сложните полета са конкатинирани стойности на подполетата им
        /// </summary>
        /// ///
        /// <param name="administrationId">Идентификатор на администрация</param>
        /// <param name="skip">Колко записа да бъдат пропуснати</param>
        /// <param name="take">Колко записа да бъдат взети</param>
        /// <param name="searchKey"></param>
        /// <param name="searchPattern"></param>
        /// <param name="toDate">До дата на вписване, включително</param>
        /// <param name="fromDate">От дата на вписване, включително</param>
        /// <param name="searchToDate">До дата за търсене по критерии от тип дата</param>
        /// <param name="searchFromDate">От дата за търсене по критерии от тип дата</param>
        /// <returns></returns>
        public Task<(JsonResult, List<Dictionary<string, object>>, Dictionary<string, FormField>)> GetRegistrationProcessListWhereSubfieldsAreConcatenated(Guid administrationId, 
            int skip, 
            int take, 
            string searchKey,
            string searchPattern, 
            DateTime? toDate, 
            DateTime? fromDate,
            DateTime? searchToDate,
            DateTime? searchFromDate);

        /// <summary>
        /// Връща списък със заявени услуги за лице
        /// </summary>
        /// <param name="mpris">Идентификатори на партида</param>
        /// <param name="roleInProcessType">Роля в заявената услуга</param>
        /// <param name="skip">Колко записа да бъдат пропуснати</param>
        /// <param name="take">Колко записа да бъдат взети</param>
        /// <returns></returns>
        public Task<JsonResult> GetProcessList(IEnumerable<Guid> mpris, int roleInProcessType, int skip, int take);

        /// <summary>
        /// Връща модел на форма от записани данни
        /// </summary>
        /// <param name="mpris">Идентификатори на партида</param>
        /// <param name="serviceId">Идентификато на процеса, за който се извличат данни</param>
        /// <param name="skip">Колко записа да бъдат пропуснати</param>
        /// <param name="take">Колко записа да бъдат взети</param>
        /// <returns></returns>
        public Task<JsonResult> GetDataForPublicTable(IEnumerable<Guid> mpris, int serviceId, int skip, int take);

        /// <summary>
        /// Одобрение на конфигурация
        /// </summary>
        /// <param name="formId"></param>
        /// <returns></returns>
        public Task<OperationResult> ApproveConfiguration(int formId);

        /// <summary>
        /// Взимане име на записан файл
        /// </summary>
        /// <param name="fileKey">Ключ на записания файл</param>
        /// <returns></returns>
        public Task<string> GetStoredFileName(Guid fileKey);

        /// <summary>
        /// Връща модел на формата по родителски идентификатор
        /// </summary>
        /// <param name="formId">Идентификатор на формата</param>
        /// <returns></returns>
        public Task<FormViewModel> GetFormViewModelByFormId(int formId);

        /// <summary>
        /// Връща списък със записи по услуга
        /// </summary>
        /// <param name="serviceId">Идентификатор на услуга</param>
        /// <param name="customViewId"></param>
        /// <param name="request"></param>
        /// <param name="filter">Филтър за търсене</param>
        /// <returns></returns>
        public Task<IActionResult> GetTableDataForService(int serviceId, int customViewId, IDataTablesRequest request,
            CustomTableViewViewModel filter);

        /// <summary>
        /// Връжа колоните за табличния вид на актуалната форма за услуга
        /// </summary>
        /// <param name="serviceId">Идентификатор на услугата. При 0 взима услугата за вписване</param>
        /// <param name="customViewsId">Идентификатор на справка</param>
        /// <returns></returns>
        public Task<List<SelectListItem>> GetColumnsForTableView(int serviceId, int customViewsId = 0);

        /// <summary>
        /// Връща колекция от имената на полета като ключ за дадена форма
        /// </summary>
        /// <param name="formId">Идентификатор на форма</param>
        /// <returns></returns>
        public Task<Dictionary<string, string>> GetFormFieldNamesInFlatList(int formId);

        /// <summary>
        /// Връща колекция от имената на полета като ключ за дадена форма
        /// </summary>
        /// <param name="formParentId">Идентификатор на форма родител</param>
        /// <param name="includeComplexFields">Дали да включва сложните полета в списъка</param>
        /// <returns></returns>
        public Task<Dictionary<string, string>> GetFormFieldNamesInFlatListByParentId(int formParentId, bool includeComplexFields = false);


        /// <summary>
        /// Очаква списък от имена на полета за форма и връща списък от тези които са неприемливи за нея
        /// </summary>
        /// <param name="formId">Идентификатор на форма</param>
        /// <param name="fieldNames">Списък от имена на полета</param>
        /// <returns></returns>
        public Task<IEnumerable<string>> GetUnacceptableFieldNamesForForm(int formId,
            IEnumerable<string> fieldNames);

        /// <summary>
        /// Очаква списък от имена на полета за форма и връща списък от тези които са неприемливи за нея
        /// </summary>
        /// <param name="formParentId">Идентификатор на форма родител</param>
        /// <param name="fieldNames">Списък от имена на полета</param>
        /// <returns></returns>
        public Task<IEnumerable<string>> GetUnacceptableFieldNamesForFormByParentId(int formParentId,
            IEnumerable<string> fieldNames);

        /// <summary>
        /// Зареджда потребителските справки
        /// </summary>
        /// <returns></returns>
        public Task<List<CustomViewViewModel>> GetCustomViews();

        /// <summary>
        /// Добавяне или редакция на потребителска справка
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public Task<SaveOperationResult> UpsertCustomView(CustomViewViewModel model);

        /// <summary>
        /// Извлича списък с потенциалните колони за потребителска справка
        /// </summary>
        /// <returns></returns>
        public Task<Dictionary<string, string>> CustomViewColumns();

        /// <summary>
        /// Извлича модел на потребителска справка
        /// </summary>
        /// <param name="id">Идентификатор на потребитлеска справка</param>
        /// <returns></returns>
        public Task<CustomViewViewModel> GetCustomViewViewModel(int id);

        /// <summary>
        /// Изтриване на потребителска справка по идентификатор
        /// </summary>
        /// <param name="id">Идентификатор на потребителска справка</param>
        /// <returns></returns>
        public Task<OperationResult> DeleteCustomView(int id);

        /// <summary>
        /// Заменя номенклатурните стойности във FormViewModel с актуалните им текстови стойности
        /// </summary>
        /// <param name="formModelFields">Модел със заредени стойности</param>
        /// <returns></returns>
        public Task ResolveFormFieldsViewModelValues(IEnumerable<FormField> formModelFields);

        /// <summary>
        /// Зареждане на конфигурацията на формата на услугата за вписване от базата данни
        /// </summary>
        /// <returns></returns>
        public Task<string> ImportRegisterFormConfiguration();

        /// <summary>
        /// Връща идентификатора на формата родител на услугата за вписване
        /// </summary>
        /// <returns>Идентификатор или null ако не е намерен резултат</returns>
        public Task<int?> GetFormParentIdOfTheRegisterService();

        /// <summary>
        /// Разпределя стойностите на заредени от базата registryItem-и в съответстващ viewModel на форма
        /// </summary>
        /// <param name="registerItems">Списък с registryItem</param>
        /// <param name="viewModel">viewModel на форма</param>
        public void DistributeRegisterItemValuesToFormViewModel(List<RegisterItem> registerItems,
            FormViewModel viewModel);

        /// <summary>
        /// Извличане на условия към форма
        /// </summary>
        /// <param name="formParentId">Идентификатор на родителска форма</param>
        /// <returns></returns>
        public Task<List<FormCondition>> GetFormConditions(int formParentId);

        protected internal Task<Dictionary<string, Dictionary<string, string>>> CacheNomenclaturesForValuesOfInterest(
            Dictionary<string, FormField> valuesOfInterest,
            bool detailedEkatte = true);

        /// <summary>
        /// Връща модел на условие към форма по родителски идентификатор на форма
        /// </summary>
        /// <param name="formConditionId"></param>
        /// <returns></returns>
        public Task<AddConditionViewModel> GetFormConditionViewModel(int formConditionId);

        /// <summary>
        /// Записва условие за форма
        /// </summary>
        /// <param name="model">Модел на условие за форма</param>
        /// <returns></returns>
        Task<SaveOperationResult> SaveFormCondition(AddConditionViewModel model);

        /// <summary>
        /// Връща дървовиден модел на условия към форма по родителски идентификатор на форма
        /// </summary>
        /// <param name="formParentId"></param>
        /// <returns></returns>
        Task<Dictionary<string, FieldConditions>> GetConditionTreeForFormParentId(int formParentId);

        /// <summary>
        /// Прилава условията върху модел на форма
        /// </summary>
        /// <param name="model">Модела на формата</param>
        /// <returns></returns>
        Task ApplyConditionTreeOnFormModel(FormViewModel model);

        /// <summary>
        /// Извличане на условие по идентификатор
        /// </summary>
        /// <param name="id">Идентификатор на условие</param>
        /// <returns></returns>
        public Task<FormCondition> GetFormConditionById(int id);

        /// <summary>
        /// Изтриване на условие по идентификатор
        /// </summary>
        /// <param name="id">Идентификатор на условие</param>
        /// <returns></returns>
        public Task<OperationResult> DeleteFormCondition(int id);
    }
}