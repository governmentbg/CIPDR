using Microsoft.AspNetCore.Mvc.Rendering;
using URegister.Core.Services;
using URegister.Infrastructure.Model.RegisterForms;

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
        /// Връща модел на формата по родителски идентификатор
        /// </summary>
        /// <param name="formParentId"></param>
        /// <returns></returns>
        public Task<FormViewModel> GetFormViewModel(int formParentId);

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
        public Task<bool> SaveDesignerJson(string json, int formParentId, string formTitle);

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
    }
}
