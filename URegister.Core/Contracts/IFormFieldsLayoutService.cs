using Microsoft.AspNetCore.Http;
using URegister.Infrastructure.Model.RegisterForms;

namespace URegister.Core.Contracts
{
    public interface IFormFieldsLayoutService
    {
        /// <summary>
        /// Разпределя стойностите на полетата от POST заявката в дървовидната структура на view model-а
        /// </summary>
        /// <param name="form">Формата от POST заявката</param>
        /// <param name="viewModel">View model-а</param>
        public void DistributePostedFieldValuesToViewModel(IFormCollection form, FormViewModel viewModel);

        /// <summary>
        /// Прегенерира имена на подполетата спрямо пътя до тях
        /// </summary>
        /// <param name="formFields"></param>
        /// <param name="namePathSoFar"></param>
        public void GiveSnakeCaseNamesToComplexFieldChildren(IEnumerable<FormField> formFields,
            string namePathSoFar = "");
    }
}
