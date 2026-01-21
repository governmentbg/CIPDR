using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NCalc;
using System.Globalization;
using System.Text.RegularExpressions;
using URegister.Core.Contracts;
using URegister.Core.Data;
using URegister.Core.Data.Models.Common;
using URegister.Core.Models.Service;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Extensions;
using URegister.Infrastructure.Model.RegisterForms;

namespace URegister.Core.Services
{
    public class FieldFormulaCalculationService : BaseService, IFieldFormulaCalculationService
    {
        private readonly IUserContext _userContext;
        public static readonly char[] OperationSymbols = new[] { '(', ')', '-', '+', '*', '/' };
        private static readonly string OperationSymbolsPattern = "([" + string.Join("", OperationSymbols.Select(c => Regex.Escape(c.ToString()))) + "])";

        public FieldFormulaCalculationService(IApplicationRepository repo, 
            ILogger<BaseService> logger, 
            IUserContext userContext) : base(repo, logger)
        {
            _userContext = userContext;
        }

        public async Task<OperationResult> CalculateFormulas(FormViewModel model)
        {
            try
            {
                List<FieldFormula> formulas = await Repo.AllReadonly<FieldFormula>()
                    .Where(f => f.FormParentId == model.FormParentId)
                    .OrderBy(f => f.Priority).ToListAsync();

                if (!formulas.Any())
                {
                    return new OperationResult();
                }

                Dictionary<string, FormField> flatList = new Dictionary<string, FormField>();

                FormConfigurationPersistenceService.AddValuesOfInterestToDictionary(model.FormFields, flatList, false,
                    true);

                foreach (FieldFormula fieldFormula in formulas)
                {
                    FormField targetField = flatList[fieldFormula.TargetField];

                    SetValueToTargetField(fieldFormula, flatList, targetField);

                    int repetitionIndex = 1;

                    while (flatList.ContainsKey(RepeatedFormFieldHelperService.InsertBeforeFirstUnderscore(fieldFormula.TargetField, "#" + repetitionIndex)))
                    {
                        var repetitionField =
                            flatList[
                                RepeatedFormFieldHelperService.InsertBeforeFirstUnderscore(fieldFormula.TargetField, "#" + repetitionIndex)];

                        SetValueToTargetFieldRepetition(fieldFormula, flatList, repetitionField);
                        repetitionIndex++;
                    }
                }

                return new OperationResult();

            }
            catch (OverflowException e)
            {
                Logger.LogError(e, $"Невъзможно изчисление, проверете за деление на 0 или непопълнен делител в {nameof(CalculateFormulas)}");
                return new OperationResult("Невъзможно изчисление, проверете за деление на 0 или непопълнен делител");
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Неуспех при изчисления във формулите за формата в {nameof(CalculateFormulas)}");
                return new OperationResult("Неуспех при изчисление на формулите за формата");
            }
        }

        private void SetValueToTargetField(FieldFormula fieldFormula, Dictionary<string, FormField> flatList, FormField targetField)
        {
            string resolvedFormula = ResolveFieldValueInFormula(fieldFormula.Formula, flatList);

            Expression expression = new Expression(resolvedFormula);
            decimal result = Convert.ToDecimal(expression.Evaluate());

            if (targetField.Type == nameof(SimpleFormFieldType.BulgarianCurrency))
            {
                targetField.Value = BGCurrencyService.EuroValueToFormFieldValue(result);
            }
            else
            {
                targetField.Value = Math.Round(result, targetField.NumberOfDigitsAfterDelimiter ?? 0)
                    .ToString($"F{targetField.NumberOfDigitsAfterDelimiter ?? 0}", CultureInfo.InvariantCulture);
            }
        }
        
        private void SetValueToTargetFieldRepetition(FieldFormula fieldFormula, Dictionary<string, FormField> flatList, FormField targetField)
        {
            int index = RepeatedFormFieldHelperService.GetRepetitionIndex(targetField.Name);

            string resolvedFormula = ResolveFieldValueInFormula(fieldFormula.Formula, flatList, index);

            Expression expression = new Expression(resolvedFormula);
            decimal result = Convert.ToDecimal(expression.Evaluate());

            if (targetField.Type == nameof(SimpleFormFieldType.BulgarianCurrency))
            {
                targetField.Value = BGCurrencyService.EuroValueToFormFieldValue(result);
            }
            else
            {
                targetField.Value = Math.Round(result, targetField.NumberOfDigitsAfterDelimiter ?? 0)
                    .ToString($"F{targetField.NumberOfDigitsAfterDelimiter ?? 0}", CultureInfo.InvariantCulture);
            }
        }

        public async Task<OperationResult> Delete(int id)
        {
            try
            {
                var formula = await Repo.All<FieldFormula>()
                    .TagWith(nameof(Delete))
                    .SingleOrDefaultAsync(f => f.Id == id);

                if (formula == null)
                {
                    return new OperationResult($"Активна формула с идентификатор {id} не е открита");
                }
                formula.IsActive = false;
                formula.ModifiedByUserId = _userContext.UserId;

                await Repo.SaveChangesAsync();
                return new OperationResult();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Проблем при изтриване на формула с id {id}");
                return new OperationResult("Проблем при изтриване на формула");
            }
        }

        public async Task<FieldFormulaVM> GetFormulaModel(int id)
        {
            var formula = await Repo.GetByIdAsync<FieldFormula>(id);

            return new FieldFormulaVM()
            {
                Id = id,
                TargetField = formula.TargetField,
                FormulaText = formula.Formula,
                FormParentId = formula.FormParentId ?? 0
            };
        }

        /// <summary>
        /// Връща списък с формулите за регистъра подходящ за datatable.js
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> GetList(IDataTablesRequest request, int formParentId)
        {
            var data = GetListOfModels(formParentId);

            var countAll = await data.CountAsync();
            return request.GetResponseJson(data, countAll);
        }

        /// <summary>
        /// Връща списък с формулите за регистъра
        /// </summary>
        /// <param name="formParentId"></param>
        /// <returns></returns>
        public IQueryable<FieldFormulaVM> GetListOfModels(int formParentId)
        {
            var data = Repo.AllReadonly<FieldFormula>()
                .Where(f => f.FormParentId == formParentId)
                .TagWith(nameof(GetList))
                .OrderBy(f => f.Priority)
                .Select(f => new FieldFormulaVM()
                {
                    Id = f.Id,
                    TargetField = f.TargetField,
                    FormulaText = f.Formula,
                    FormParentId = f.FormParentId ?? 0,
                    Priority = f.Priority
                });

            return data;
        }

        public async Task ChangePriority(int id, bool up)
        {
            FieldFormula formulaById = await Repo.GetByIdAsync<FieldFormula>(id);

            var data = await Repo.All<FieldFormula>()
                .Where(f => f.FormParentId == formulaById.FormParentId)
                .OrderBy(x => x.Priority)
                .ToListAsync();

            for (int i = 0; i < data.Count; i++)
            {
                var item = data[i];
                FieldFormula itemChange = null;
                if (item.Id == id)
                {
                    if (up && i > 0)
                    {
                        itemChange = data[i - 1];
                    }
                    if (!up && i < (data.Count - 1))
                    {
                        itemChange = data[i + 1];
                    }
                    if (itemChange != null)
                    {
                        (item.Priority, itemChange.Priority) = (itemChange.Priority, item.Priority);
                        await Repo.SaveChangesAsync();
                    }
                }
            }
        }

        private string ResolveFieldValueInFormula(string fieldFormulaFormula, Dictionary<string, FormField> flatList, int repetitionIndex = 0)
        {
            string sanitizedFormula = fieldFormulaFormula.Replace(" ", string.Empty);//.Replace("{{", string.Empty).Replace("}}", string.Empty);

            string sumPattern = @"Sum\(\{\{(\w+)\}\}\)";

            sanitizedFormula = Regex.Replace(sanitizedFormula, sumPattern, m =>
            {
                string fieldName = m.Groups[1].Value;
                string fieldValue = ResolveSumFieldValue(fieldName, flatList);
                return fieldValue;
            }, RegexOptions.IgnoreCase);

            const string pattern = @"\{\{(\w+)\}\}";

            return Regex.Replace(sanitizedFormula, pattern, m =>
            {
                string fieldName = m.Groups[1].Value;
                string fieldValue = ResolveFieldValue(fieldName, repetitionIndex, flatList);

                if (string.IsNullOrEmpty(fieldValue))
                    throw new KeyNotFoundException($"Field '{{{{{fieldName}}}}}' resolved to null/empty.");

                return fieldValue;
            });
        }

        private string ResolveSumFieldValue(string fieldName, Dictionary<string, FormField> flatList)
        {
            int repetitionIndex = 1;

            decimal totalSum = SafeDecimalParse(flatList[fieldName].Value);

            while (flatList.ContainsKey(RepeatedFormFieldHelperService.InsertBeforeFirstUnderscore(fieldName, "#" + repetitionIndex)))
            {
                totalSum += SafeDecimalParse(flatList[RepeatedFormFieldHelperService.InsertBeforeFirstUnderscore(fieldName, "#" + repetitionIndex)].Value);
                repetitionIndex++;
            }

            return totalSum.ToString(CultureInfo.InvariantCulture);
        }

        private decimal SafeDecimalParse(string value)
        {
            return decimal.TryParse(value, CultureInfo.InvariantCulture, out decimal result) ? result : 0m;
        }

        private string ResolveFieldValue(string fieldName, int repetitionIndex, Dictionary<string, FormField> flatList)
        {
            FormField fieldToResolve;

            if (repetitionIndex == 0)
            {
                fieldToResolve = flatList[fieldName];
            }
            else // при повтаряемо поле, първо търсим дали полетата във формулата също имат съшия индекс. Ако не, взимаме оригиналът
            {
                string expectedRepetitionName =
                    RepeatedFormFieldHelperService.InsertBeforeFirstUnderscore(fieldName, "#" + repetitionIndex);

                fieldToResolve = flatList.TryGetValue(expectedRepetitionName, out var value) ? value : flatList[fieldName];
            }

            if (fieldToResolve.Type == nameof(SimpleFormFieldType.BulgarianCurrency))
            {
                return string.IsNullOrWhiteSpace(fieldToResolve.Value)
                    ? string.Empty
                    : BGCurrencyService.RegistryItemValueToValueInEuro(fieldToResolve.Value).ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                return  string.IsNullOrWhiteSpace(fieldToResolve.Value)
                    ? string.Empty
                    : fieldToResolve.Value;
            }
        }

        public async Task<SaveOperationResult> Save(FieldFormulaVM model)
        {
            int addedObjectId;

            try
            {

                if (model.Id == 0)
                {
                    var lastPriorityFormula = await GetListOfModels(model.FormParentId).LastOrDefaultAsync();

                    FieldFormula newFormula = new FieldFormula()
                    {
                        FormParentId = model.FormParentId,
                        Formula = model.FormulaText,
                        Priority = lastPriorityFormula == null ? 1 : lastPriorityFormula.Priority + 1,
                        ModifiedByUserId = _userContext.UserId,
                        ModifiedOn = DateTime.UtcNow,
                        TargetField = model.TargetField
                    };

                    await Repo.AddAsync(newFormula);
                    addedObjectId = newFormula.Id;
                }
                else
                {
                    FieldFormula formulaForEdit = await Repo.GetByIdAsync<FieldFormula>(model.Id);

                    if (formulaForEdit == null)
                    {
                        return new SaveOperationResult("Не е намерен обекта за промяна с идентификатор " + model.Id);
                    }

                    formulaForEdit.Formula = model.FormulaText;
                    formulaForEdit.TargetField = model.TargetField;
                    formulaForEdit.ModifiedByUserId = _userContext.UserId;
                    formulaForEdit.ModifiedOn = DateTime.UtcNow;

                    addedObjectId = model.Id;
                }

                await Repo.SaveChangesAsync(); 

                return new SaveOperationResult(true, addedObjectId);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Проблем при запис на формула в {nameof(Save)}");
                return new SaveOperationResult(true, "Проблем при запис на формула");
            }
        }
    }
}
