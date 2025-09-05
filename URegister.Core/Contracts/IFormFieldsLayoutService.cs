using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;
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

        /// <summary>
        /// Разпределя повторяемите стойности във вю модел
        /// </summary>
        /// <param name="viewModel">Вю модел</param>
        /// <param name="match">Съвпадение по регулярен израз</param>
        /// <param name="postedName">Име на поле</param>
        /// <param name="postedValue">Стойност на поле</param>
        public void HandleValueDistributionForRepeatingValues(FormViewModel viewModel,
            Match match,
            string postedName,
            string? postedValue);

        /// <summary>
        /// Записва стойност към поле на форма
        /// </summary>
        /// <param name="postedName">Име на поле</param>
        /// <param name="postedValue">Стойност на поле</param>
        /// <param name="formFields">Поле на форма</param>
        public void AssignPostedElementValueToFormField(string postedName,
            string postedValue,
            IEnumerable<FormField> formFields);
    }
}
