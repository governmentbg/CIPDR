using Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using URegister.Core.Contracts;
using URegister.Core.Data;
using URegister.Infrastructure.Model.RegisterForms;
using System.Text.Json.Serialization;
using URegister.Infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using URegister.Core.Data.Models.Common;

namespace URegister.Core.Services
{
    /// <summary>
    /// Сервиз с методи засягащи подредбата на конфигурираните полетата на форма
    /// </summary>
    public class FormConfigurationPersistenceService : BaseService, IFormConfigurationPersistenceService
    {
        private readonly IFormFieldsLayoutService _formFieldsLayoutService;

        public FormConfigurationPersistenceService(
            IApplicationRepository repo, 
            ILogger<FormConfigurationPersistenceService> logger,
            IFormFieldsLayoutService formFieldsLayoutService) : base(repo, logger)
        {
            _formFieldsLayoutService = formFieldsLayoutService;
        }

        /// <summary>
        /// Връша списък с формите в регистър
        /// </summary>
        /// <param name="registerIndex">Идентификатор на регистър</param>
        /// <returns></returns>
        public async Task<IEnumerable<object>> GetForms(int registerIndex)
        {
            try
            {
                var result = await Repo.AllReadonly<Form>()
                    .TagWith(nameof(GetForms))
                    .Select(f => new
                    {
                        id = f.Id,
                        title = f.Title,
                        parentId = f.ParentId,
                        purpose = f.Purpose
                    }).ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Проблем при зареждане на форми в {nameof(GetForms)} за регистър с идентификатор {registerIndex}");
            }

            return new List<object>();
        }

        /// <summary>
        /// Връща модел на формата по родителски идентификатор
        /// </summary>
        /// <param name="formParentId"></param>
        /// <returns></returns>
        public async Task<FormViewModel> GetFormViewModel(int formParentId)
        {
            try
            {                
                Form savedForm = await Repo.AllReadonly<Form>()
                    .TagWith(nameof(GetFormViewModel))
                    .FirstOrDefaultAsync(f => f.ParentId == formParentId);
                
                if (savedForm == null) {
                    Logger.LogError($"Не е намерена форма с родителски идентификатор {formParentId} в {nameof(GetFormViewModel)}");
                    return null;
                }

                FormViewModel viewModel = new FormViewModel
                {
                    FormTitle = savedForm.Title,
                    FormParentId = formParentId,
                    Purpose = savedForm.Purpose
                };

                string jsonFieldsModel = savedForm.FieldConfiguration;

                using JsonDocument doc = JsonDocument.Parse(jsonFieldsModel);
                JsonElement root = doc.RootElement;

                List<FormField> formFields = new List<FormField>();

                foreach (JsonElement element in root.EnumerateArray())
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                    };
                    FormField? newFormField = JsonSerializer.Deserialize<FormField>(element, options);

                    if (newFormField == null)
                    {
                        Logger.LogError($"Cannot deserialize {element} in {nameof(FormField)} in {nameof(GetFormViewModel)}");
                        continue;
                    }

                    if (newFormField.CanBeRepeated)
                    {
                        newFormField.Repetitions = new List<FormField>();
                    }

                    formFields.Add(newFormField);
                }

                _formFieldsLayoutService.GiveSnakeCaseNamesToComplexFieldChildren(formFields);

                viewModel.FormFields = formFields;
                return viewModel;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Проблем при генериране на view model в {nameof(GetFormViewModel)}");
                return new FormViewModel
                {
                    FormFields = new List<FormField>()
                };
            }
        }

        /// <summary>
        /// Записва JSON от дизайнера в базатта данни
        /// </summary>
        /// <returns></returns>        
        public async Task<bool> SaveDesignerJson(string json, int formParentId, string formTitle)
        {
            try
            {
                Form parentForm = await Repo.All<Form>()
                    .IgnoreQueryFilters()
                    .TagWith(nameof(SaveDesignerJson))
                    .SingleOrDefaultAsync(f => f.Id == formParentId);
                
                Form newForm = new Form();

                newForm.Purpose = parentForm.Purpose;
                newForm.Parent = parentForm;
                newForm.ModifiedOn = DateTime.UtcNow;
                newForm.FieldConfiguration = json;
                newForm.Title = formTitle;
                newForm.ModifiedByUserId = Guid.Empty.ToString();//TODO
                    
                List<Form> oldVersions = await Repo.All<Form>()
                    .Where(f => f.ParentId == formParentId)
                    .TagWith(nameof(SaveDesignerJson))
                    .ToListAsync();

                //TODO : при минаване на .net9 + да се ползва JsonElement.DeepEquals
                if (oldVersions.Count == 1 && json.MinifyJson() == oldVersions.First().FieldConfiguration.MinifyJson())
                {
                    return true;
                }
                
                Repo.DeleteRange(oldVersions);
                await Repo.AddAsync(newForm);
                await Repo.SaveChangesAsync();

                if (parentForm == null)
                {
                    newForm.ParentId = newForm.Id;
                    await Repo.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Проблем при запис на JSON конфигурация на форма в {nameof(SaveDesignerJson)}");
                return false;
            }
        }

        /// <summary>
        /// Изтрива форма по идентификатор
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<OperationResult> DeleteForm(int id)
        {
            try
            {
                Form form = await Repo.GetByIdAsync<Form>(id);
                if (form == null)
                {
                    return new OperationResult($"Активна форма с идентификатор {id} не е открита");
                }

                await Repo.DeleteAsync<Form>(id);
                await Repo.SaveChangesAsync();
                return new OperationResult();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Проблем при изтриване на формата с id {id}");
                return new OperationResult("Проблем при изтриване на формата");
            }
        }

        /// <summary>
        /// Редакция на съществуваща форма
        /// </summary>
        /// <returns></returns>
        public async Task<SaveOperationResult> EditForm(AddFormViewModel model)
        {
            try
            {

                List<Form> parentFormList = await Repo.All<Form>()
                    .TagWith(nameof(EditForm))
                    .Where(f => f.ParentId == model.ParentId).ToListAsync();

                if (!parentFormList.Any())
                {
                    return new SaveOperationResult("Формата не е намерена в базата данни");
                }

                if (parentFormList.Count > 1)
                {
                    Logger.LogWarning($"Повече от една активни форми с родителски идентификатор {model.ParentId}");
                }

                Repo.DeleteRange(parentFormList);

                Form newForm = new Form()
                {
                    ParentId = model.ParentId,
                    Title = model.FormTitle,
                    Purpose = model.Purpose,
                    FieldConfiguration = parentFormList.OrderBy(f => f.Id).Last().FieldConfiguration,
                    ModifiedOn = DateTime.UtcNow,
                    ModifiedByUserId = Guid.Empty.ToString() //TODO
                };

                await Repo.AddAsync(newForm);
                await Repo.SaveChangesAsync();

                return new SaveOperationResult(true, newForm.Id);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Проблем при редакция на форма с parentId {model.ParentId} в {nameof(EditForm)}");
            }

            return new SaveOperationResult("Проблем при редакция на форма");
        }

        /// <summary>
        /// Зарежда конфигурацията за форма
        /// </summary>
        /// <param name="formParentId">Идентификатор на родител на форма</param>
        /// <returns></returns>
        public async Task<string> LoadDesignerJson(int formParentId)
        {
            try
            {
                Form form = await Repo.AllReadonly<Form>()
                    .TagWith(nameof(LoadDesignerJson))
                    .FirstOrDefaultAsync(f => f.ParentId == formParentId);
                return form != null ? form.FieldConfiguration : string.Empty;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Грешка при зареждане на данни в {nameof(LoadDesignerJson)} за форма родител {formParentId}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Запазва нова форма
        /// </summary>
        /// <param name="model">Модел на формата</param>
        /// <returns></returns>
        public async Task<SaveOperationResult> SaveForm(AddFormViewModel model)
        {
            try
            {
                bool nameDuplicates = await Repo.AllReadonly<Form>()
                    .Where(f => EF.Functions.ILike(f.Title, model.FormTitle))
                    .TagWith(nameof(SaveForm))
                    .AnyAsync();

                if (nameDuplicates)
                {
                    return new SaveOperationResult($"Името {model.FormTitle} вече съществува в регистъра");
                }

                Form newForm = new Form
                {
                    IsActive = true,
                    Purpose = model.Purpose!,
                    Title = model.FormTitle!,
                    ModifiedByUserId = Guid.Empty.ToString(), //TODO
                    ModifiedOn = DateTime.UtcNow,
                    FieldConfiguration = JsonSerializer.Serialize(new List<FormField>())
                };

                await Repo.AddAsync(newForm);
                await Repo.SaveChangesAsync();
                newForm.ParentId = newForm.Id;
                await Repo.SaveChangesAsync();

                return new SaveOperationResult(true, newForm.Id);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Проблем при запис на форма {model.FormTitle} в {nameof(SaveForm)}");
                return new SaveOperationResult("Проблем при запис на форма");
            }
        }
        /// <summary>
        /// Връша списък с формите в регистър
        /// </summary>
        /// <param name="registerIndex">Идентификатор на регистър</param>
        /// <returns></returns>
        public async Task<List<SelectListItem>> GetFormsDDL()
        {
            var forms = Repo.AllReadonly<Form>();
           var result = await Repo.AllReadonly<Form>()
                             .TagWith(nameof(GetFormsDDL))
                             .Where(x => !forms.Any(f => f.ParentId == x.ParentId && f.Id > x.Id))
                             .Select(x => new SelectListItem
                             {
                                 Text = x.Title,
                                 Value = x.ParentId.ToString()
                             }).ToListAsync();
            result.Insert(0, new SelectListItem
            {
                Text = "Изберете",
                Value = null
            });
            return result;
        }
    }        
}
