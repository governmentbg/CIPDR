using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using URegister.Core.Contracts;
using URegister.Infrastructure.Model.RegisterForms;
using System.Text.RegularExpressions;

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
                if (pair.Key.StartsWith("__"))
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
                if (pair.Key is nameof(FormViewModel.SelectedType))
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

            foreach (var file in form.Files)
            {
                #region За повтарящи се елементи добавени от потребителя във формата

                Match match = Regex.Match(file.Name, repeatingFieldValuePattern);
                if (match.Success)
                {
                    try
                    {
                        HandleValueDistributionForRepeatingValues(viewModel, match, file.Name, null, file);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"Проблем в {nameof(HandleValueDistributionForRepeatingValues)}");
                    }
                    continue;
                }

                #endregion

                AssignPostedElementValueToFormField(file.Name, string.Empty, viewModel.FormFields, file);
            }
        }

        private void HandleValueDistributionForRepeatingValues(
            FormViewModel viewModel, 
            Match match,
            string postedName,
            string? postedValue,
            IFormFile file = null)
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
                clone.File = file;
                clonedParent.Fields!.Add(clone);
            }
            else
            {
                var repeatedField = repeaterParent.CreateRepeaterClone(postedName, postedValue);
                repeatedField.File = file;
                repeaterParent.Repetitions!.Add(repeatedField);
            }
        }

        private void AssignPostedElementValueToFormField(string postedName,
            string postedValue,
            IEnumerable<FormField> formFields,
            IFormFile? file = null)
        {
            if (!postedName.Contains(ComplexFieldPathSeparator))
            {
                var foundField = formFields.FirstOrDefault(f => f.Name == postedName);
                if (foundField != null)
                {
                    foundField.Value = postedValue;
                    foundField.File = file;
                }
                else
                {
                    _logger.LogError($"Field with name {postedName} not found in {nameof(AssignPostedElementValueToFormField)}");
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
                    _logger.LogError($"Поле с път {pathSoFar} не в намерено. Метод {nameof(AssignPostedElementValueToFormField)}");
                    return;
                }

                if (i == pathParts.Length - 1)
                {
                    targetField.Value = postedValue;
                    targetField.File = file;
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
    }        
}
