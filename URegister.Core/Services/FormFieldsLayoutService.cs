using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using URegister.Core.Contracts;
using URegister.Infrastructure.Model.RegisterForms;
using System.Text.RegularExpressions;
using URegister.Core.Models.Process;
using URegister.Infrastructure.Constants;

namespace URegister.Core.Services
{
    /// <summary>
    /// Сервиз с методи засягащи подредбата на конфигурираните полетата на форма
    /// </summary>
    public class FormFieldsLayoutService : IFormFieldsLayoutService
    {
        private readonly ILogger<FormFieldsLayoutService> _logger;
        private const char ComplexFieldPathSeparator = '_';

        public FormFieldsLayoutService(ILogger<FormFieldsLayoutService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Разпределя стойностите на полетата от POST заявката в дървовидната структура на view model-а
        /// </summary>
        /// <param name="form">Формата от POST заявката</param>
        /// <param name="viewModel">View model-а</param>
        public void DistributePostedFieldValuesToViewModel(IFormCollection form, FormViewModel viewModel)
        {
            string repeatingFieldValuePattern = @"^(?<repeaterParentName>[^#]+)#(?<index>\d+)(?:_(?<subfieldName>.*))?$";

            foreach (var pair in form)
            {
                if (pair.Key.StartsWith("__") || 
                    pair.Key.StartsWith(FormConstants.FormFieldIgnoreValuePrefix, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }
                if (pair.Key == nameof(FormViewModel.FormParentId))
                {
                    viewModel.FormParentId = int.Parse(pair.Value!);
                    continue;
                }
                if (pair.Key == nameof(FormViewModel.FormTitle))
                {
                    viewModel.FormTitle = pair.Value!;
                    continue;
                }
                if (pair.Key is nameof(FormViewModel.UserTimeZoneOffsetInMinutes))
                {
                    viewModel.UserTimeZoneOffsetInMinutes = int.Parse(pair.Value);
                }
                if (pair.Key is nameof(FormViewModel.SelectedType) or 
                        nameof(ProcessStepVM.ProcessId) or
                        nameof(ProcessStepVM.FromProcessId) or
                        nameof(ProcessStepVM.ServiceStepId) or
                        nameof(ProcessStepVM.ServiceId) or
                        nameof(ProcessStepVM.IncomingNumber) or
                        nameof(ProcessStepVM.IncomingDate) or
                        nameof(ProcessStepVM.OrderNum) or
                        nameof(FormViewModel.DontUploadFilesToStorage) or 
                        nameof(FormViewModel.ConditionTree) or 
                        nameof(FormViewModel.UserTimeZoneOffsetInMinutes))
                {
                    continue;
                }
                #region За повтарящи се елементи добавени от потребителя във формата

                Match match = Regex.Match(pair.Key, repeatingFieldValuePattern);
                if (match.Success)
                {
                    try
                    {
                        HandleValueDistributionForRepeatingValues(viewModel, match, pair.Key, pair.Value);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"Проблем в {nameof(HandleValueDistributionForRepeatingValues)}");
                    }
                    continue;
                }

                #endregion

                AssignPostedElementValueToFormField(pair.Key, pair.Value!, viewModel.FormFields);
            }
        }

        /// <summary>
        /// Разпределя повторяемите стойности във вю модел
        /// </summary>
        /// <param name="viewModel">Вю модел</param>
        /// <param name="match">Съвпадение по регулярен израз</param>
        /// <param name="postedName">Име на поле</param>
        /// <param name="postedValue">Стойност на поле</param>
        public void HandleValueDistributionForRepeatingValues(
            FormViewModel viewModel, 
            Match match,
            string postedName,
            string? postedValue)
        {
            // Extract values from named groups
            string repeaterParentName = match.Groups["repeaterParentName"].Value;
            int index = int.Parse(match.Groups["index"].Value);
            string restOfName = match.Groups["subfieldName"].Value;

            FormField repeaterParent =
                viewModel.FormFields.Single(parent => repeaterParentName == parent.Name);

            if (!string.IsNullOrWhiteSpace(restOfName))
            {
                FormField? clonedParent =
                    repeaterParent.Repetitions!.SingleOrDefault(cp => cp.Name == repeaterParentName + "#" + index);

                if (clonedParent == null)
                {
                    clonedParent = repeaterParent.CreateRepeaterClone(repeaterParentName + "#" + index, string.Empty);
                    repeaterParent.Repetitions!.Add(clonedParent);
                }

                var repeaterParentEquivalentSubfield = repeaterParent.Fields!.First(f => f.Name == $"{repeaterParentName}_{restOfName}");
                var clone = repeaterParentEquivalentSubfield.CreateRepeaterClone(postedName, postedValue);
                clonedParent.Fields!.Add(clone);
            }
            else
            {
                FormField? clonedParent =
                    repeaterParent.Repetitions!.SingleOrDefault(cp => cp.Name == repeaterParentName + "#" + index);

                if (clonedParent == null)
                {
                    var repeatedField = repeaterParent.CreateRepeaterClone(postedName, postedValue);
                    repeaterParent.Repetitions!.Add(repeatedField);
                }
            }
        }

        /// <summary>
        /// Записва стойност към поле на форма
        /// </summary>
        /// <param name="postedName">Име на поле</param>
        /// <param name="postedValue">Стойност на поле</param>
        /// <param name="formFields">Поле на форма</param>
        public void AssignPostedElementValueToFormField(string postedName,
            string postedValue,
            IEnumerable<FormField> formFields)
        {
            if (!postedName.Contains(ComplexFieldPathSeparator))
            {
                var foundField = formFields.FirstOrDefault(f => f.Name == postedName);
                if (foundField != null)
                {
                    foundField.Value = postedValue;
                }
                else
                {
                    _logger.LogError($"Поле с име {postedName} не е намерено в {nameof(AssignPostedElementValueToFormField)}");
                }
                return;
            }

            var pathParts = postedName.Split(ComplexFieldPathSeparator);
            var pathSoFar = new StringBuilder(pathParts.First());

            for (int i = 0; i < pathParts.Length; i++)
            {
                var targetField = formFields.FirstOrDefault(f => f.Name == pathSoFar.ToString());

                if (targetField == null)
                {
                    _logger.LogInformation($"Поле с път {pathSoFar} не e намерено. Нормално състояние ако полето не е публично а извличаме само публични данни. Метод {nameof(AssignPostedElementValueToFormField)}");
                    return;
                }

                if (i == pathParts.Length - 1)
                {
                    targetField.Value = postedValue;
                    return;
                }

                pathSoFar.Append(ComplexFieldPathSeparator).Append(pathParts[i + 1]);

                formFields = targetField.Fields!;
                if (formFields == null)
                {
                    _logger.LogError($"Полето {targetField.Name} няма под полета. Метод {nameof(AssignPostedElementValueToFormField)}");
                    return;
                }
            }
        }

        /// <summary>
        /// Прегенерира имена на подполетата спрямо пътя до тях
        /// </summary>
        /// <param name="formFields"></param>
        /// <param name="namePathSoFar"></param>
        public void GiveSnakeCaseNamesToComplexFieldChildren(IEnumerable<FormField> formFields, string namePathSoFar = "")
        {
            foreach (var formField in formFields)
            {
                if (formField.Fields != null)
                {
                    GiveSnakeCaseNamesToComplexFieldChildren(formField.Fields, namePathSoFar + formField.Name + "_");
                }

                if (!string.IsNullOrEmpty(namePathSoFar))
                {
                    formField.Name = namePathSoFar + formField.Name;
                }
            }
        }

        /// <summary>
        /// Заменя символите след ':' с '*', запазвайки пърия и последия
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string MaskAfterColonKeepingFirstAndLast(string input)
        {
            // If input is null, empty, or does not contain ':', return as is
            if (string.IsNullOrEmpty(input) || !input.Contains(":"))
                return input;

            int colonIndex = input.IndexOf(':');
            string before = input.Substring(0, colonIndex + 1); // include ':'
            string after = input.Substring(colonIndex + 1);

            // If there's nothing after ':', return as is
            if (string.IsNullOrEmpty(after))
                return input;

            // If after has only 1 or 2 characters, we just keep it as is (nothing to mask)
            if (after.Length <= 2)
                return input;

            // Build masked part: keep first and last char, mask the middle with '*'
            string masked = after[0] + new string('*', after.Length - 2) + after[^1];

            return before + masked;
        }

        public static string MaskAfterColonKeepingFirstTwo(string input)
        {
            // If input is null, empty, or does not contain ':', return as is
            if (string.IsNullOrEmpty(input) || !input.Contains(":"))
                return input;

            int colonIndex = input.IndexOf(':');
            string before = input.Substring(0, colonIndex + 1); // include ':'
            string after = input.Substring(colonIndex + 1);

            // If there's nothing after ':', return as is
            if (string.IsNullOrEmpty(after))
                return input;

            // If after has 2 or fewer characters, return as is (nothing to mask)
            if (after.Length <= 2)
                return input;

            // Keep the first 2 characters, mask the rest
            string masked = after.Substring(0, 2) + new string('*', after.Length - 2);

            return before + masked;
        }
    }        
}
