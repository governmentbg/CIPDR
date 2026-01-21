using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using URegister.Core.Models.Service;
using URegister.Core.Services;
using URegister.Infrastructure.Model.RegisterForms;

namespace URegister.Core.Contracts
{
    public interface IFieldFormulaCalculationService
    {
        public Task<OperationResult> CalculateFormulas(FormViewModel model);
        Task<OperationResult> Delete(int id);
        Task<FieldFormulaVM> GetFormulaModel(int id);
        Task<IActionResult> GetList(IDataTablesRequest request, int formParentId);

        /// <summary>
        /// Връща списък с формулите за регистъра
        /// </summary>
        /// <returns></returns>
        public IQueryable<FieldFormulaVM> GetListOfModels(int formParentId);

        Task ChangePriority(int id, bool up);

        public Task<SaveOperationResult> Save(FieldFormulaVM model);
    }
}
