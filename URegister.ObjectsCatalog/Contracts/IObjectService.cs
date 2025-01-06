using URegister.Common;
using URegister.Infrastructure.Model.RegisterForms;

namespace URegister.ObjectsCatalog.Contracts
{
    /// <summary>
    /// Услуга за обекти от каталога на обектите
    /// </summary>
    public interface IObjectService
    {
        /// <summary>
        /// Вземане на списък на полетата
        /// </summary>
        /// <returns></returns>
        Task<ICollection<(string type, string label, bool isComplex, string template)>> GetFieldTypesAsync();

        /// <summary>
        /// Получаване на данни за поле
        /// </summary>
        /// <param name="type">Тип на полето</param>
        /// <returns></returns>
        Task<string> GetFieldDataAsync(string type);

        /// <summary>
        /// Запис на данни за поле
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<int> SetFieldDataAsync(FormField data);

        /// <summary>
        /// Запис на нов тип поле
        /// </summary>
        /// <param name="newType"></param>
        /// <returns>Успешен ли е записът</returns>
        Task<bool> SetFieldTypeAsync(CatalogFieldType newType);

        /// <summary>
        /// Списък типове услуги
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<(List<ServiceTypeMessage>, int)> GetServiceTypes(DatatableRequest request);

        /// <summary>
        /// Списък стъпки
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<(List<StepMessage>, int)> GetStepList(DatatableRequest request);

        /// <summary>
        /// Запис на стъпка към услуга
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task AppendUpdateStep(StepMessage request);

        /// <summary>
        /// Четене на стъпка към услуга по ид
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<StepMessage> GetStep(int id);

        /// <summary>
        /// Четене на тип услуга по ид
        /// </summary>
        /// <returns></returns>
        Task<GetServiceTypeMessage> GetServiceType(int id);
        
        /// <summary>
        /// Запис на тип услуга
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task AppendUpdate(ServiceTypeMessage request);
    }
}
