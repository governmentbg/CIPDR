using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using URegister.Common;
using URegister.Core.Contracts;
using URegister.Core.Data;
using URegister.Core.Data.Models.Common;
using URegister.Core.Data.Models.Process;
using URegister.Core.Models.CurrentRegister;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Contracts;
using URegister.Infrastructure.Extensions;
using URegister.Infrastructure.Model.RegisterForms;
using URegister.Infrastucture.Extensions;
using URegister.NomenclaturesCatalog;
using URegister.RegistersCatalog;
using RegisterItem = URegister.Core.Data.Models.Process.RegisterItem;

namespace URegister.Core.Services
{
    /// <summary>
    /// Сервиз с методи засягащи подредбата на конфигурираните полетата на форма
    /// </summary>
    public class FormConfigurationPersistenceService : BaseService, IFormConfigurationPersistenceService
    {
        private readonly IFormFieldsLayoutService _formFieldsLayoutService;
        private readonly IUserContext _userContext;
        private readonly NomenclatureGrpc.NomenclatureGrpcClient _nomenclatureClient;
        private readonly IObjectStoreService _objectStoreService;
        private readonly RegistersCatalogGrpc.RegistersCatalogGrpcClient _registerGrpcClient;
        private readonly IRegisterService _registerService;

        public FormConfigurationPersistenceService(
            IApplicationRepository repo,
            ILogger<FormConfigurationPersistenceService> logger,
            IFormFieldsLayoutService formFieldsLayoutService,
            IUserContext userContext,
            NomenclatureGrpc.NomenclatureGrpcClient nomenclatureClient,
            IObjectStoreService objectStoreService,
            RegistersCatalogGrpc.RegistersCatalogGrpcClient registerGrpcClient,
            IRegisterService registerService) : base(repo, logger)
        {
            _formFieldsLayoutService = formFieldsLayoutService;
            _userContext = userContext;
            _nomenclatureClient = nomenclatureClient;
            _objectStoreService = objectStoreService;
            _registerGrpcClient = registerGrpcClient;
            _registerService = registerService;
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
                    .IgnoreQueryFilters()
                    .Where(f => f.IsActive || f.ApprovalStatus == (int)ApprovalStatus.Requested)
                    .TagWith(nameof(GetForms))
                    .Select(f => new
                    {
                        id = f.Id,
                        title = f.Title,
                        parentId = f.ParentId,
                        purpose = f.Purpose,
                        waitingApproval = f.ApprovalStatus == (int)ApprovalStatus.Requested
                    })
                    .ToListAsync();

                return result.GroupBy(f => f.parentId) // Group by ParentId
                    .Select(g => g
                        .OrderByDescending(f => f.id)
                        .FirstOrDefault())
                    .Where(f => f != null);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Проблем при зареждане на форми в {nameof(GetForms)} за регистър с идентификатор {registerIndex}");
            }

            return new List<object>();
        }

        /// <summary>
        /// Връща списък с формите в регистър
        /// </summary>
        /// <param name="request">Заявка от datatable</param>
        /// <param name="registerId">Идентификатор на регистър</param>
        /// <param name="approvalStatus">Статус на одобрение</param>
        /// <returns></returns>
        public async Task<IActionResult> GetFormListDashboard(IDataTablesRequest request, int registerId, int approvalStatus)
        {
            try
            {
                Expression<Func<Form, bool>> filter;

                if (approvalStatus == -1)
                {
                    filter = f => f.IsActive || f.ApprovalStatus == (int)ApprovalStatus.Requested;
                }
                else if (approvalStatus == (int)ApprovalStatus.Requested)
                {
                    filter = f => !f.IsActive && f.ApprovalStatus == (int)ApprovalStatus.Requested;
                }
                else
                {
                    filter = f => f.IsActive;
                }

                var result = Repo.AllReadonly<Form>()
                    .IgnoreQueryFilters()
                    .Where(filter)
                    .TagWith(nameof(GetFormListDashboard))
                    .Select(f => new
                    {
                        id = f.Id,
                        title = f.Title,
                        parentId = f.ParentId,
                        purpose = f.Purpose,
                        waitingApproval = f.ApprovalStatus == (int)ApprovalStatus.Requested
                    });


                result.GroupBy(f => f.parentId) // Group by ParentId
                   .Select(g => g
                       .OrderByDescending(f => f.id)
                       .FirstOrDefault())
                   .Where(f => f != null);

                var countAll = 0;
                (result, countAll) = request.GetResponseData(result);
                return request.GetResponseJson(result, countAll);

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Проблем при зареждане на форми в {nameof(GetFormListDashboard)} за регистър с идентификатор {registerId}");
                return null;
            }
        }

        /// <summary>
        /// Връща модел на формата по родителски идентификатор
        /// </summary>
        /// <param name="formParentId"></param>
        /// <param name="allowUnapprovedConfiguration">Дали да зарежда и още неодобрени конфигурации</param>
        /// <returns></returns>
        public async Task<FormViewModel> GetFormViewModel(int formParentId, bool allowUnapprovedConfiguration = false)
        {
            try
            {
                Form savedForm = await Repo.AllReadonly<Form>()
                    .TagWith(nameof(GetFormViewModel))
                    .IgnoreQueryFilters()
                    .Where(f => f.ParentId == formParentId)
                    .Where(f => f.IsActive || (allowUnapprovedConfiguration && f.ApprovalStatus == (int)ApprovalStatus.Requested))
                    .OrderByDescending(f => f.Id)
                    .FirstOrDefaultAsync();

                if (savedForm == null)
                {
                    Logger.LogError($"Не е намерена форма с родителски идентификатор {formParentId} в {nameof(GetFormViewModel)}");
                    return null;
                }

                return await GetFormViewModel(savedForm);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Проблем при генериране на view model в {nameof(GetFormViewModel)} за formParentId {formParentId}");
                return new FormViewModel
                {
                    FormFields = new List<FormField>()
                };
            }
        }

        /// <summary>
        /// Връща вю модел на формата по EF модел
        /// </summary>
        /// <param name="form"></param>
        /// <param name="publicDataOnly">Дали да показва само публични данни</param>
        /// <returns></returns>
        private async Task<FormViewModel> GetFormViewModel(Form form, bool publicDataOnly = false)
        {
            try
            {
                FormViewModel viewModel = new FormViewModel
                {
                    FormTitle = form.Title,
                    FormParentId = form.ParentId!.Value,
                    Purpose = form.Purpose,
                    FormId = form.Id,
                    IsWaitingApproval = (ApprovalStatus)form.ApprovalStatus == ApprovalStatus.Requested
                };

                string jsonFieldsModel = form.FieldConfiguration;

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

                    if (publicDataOnly)
                    {
                        if (!newFormField.IsPublic)
                        {
                            continue;
                        }

                        foreach (FormField subField in newFormField.Fields.Where(s => !s.IsPublic).ToArray())
                        {
                            newFormField.Fields.Remove(subField);
                        }
                    }

                    if (newFormField.CanBeRepeated)
                    {
                        newFormField.Repetitions = new List<FormField>();
                    }

                    formFields.Add(newFormField);
                }

                _formFieldsLayoutService.GiveSnakeCaseNamesToComplexFieldChildren(formFields);

                viewModel.FormFields = formFields;

                RegisterVM register = await _registerService.GetCurrentRegister();

                viewModel.IsSubmitterRequired = register.TypeEntry != RegisterTypeEntry.Officially;

                viewModel.ConditionTree = JsonSerializer.Serialize(await this.GetConditionTreeForFormParentId(form.ParentId.Value));

                return viewModel;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Проблем при генериране на view model в {nameof(GetFormViewModel)}");
                return null;
            }
        }

        /// <summary>
        /// Записва JSON от дизайнера в базатта данни
        /// </summary>
        /// <returns></returns>        
        public async Task<bool> SaveDesignerJson(string json, int formParentId, string formTitle, bool isApproved)
        {
            try
            {
                Form existingForm = await Repo.All<Form>()
                    .IgnoreQueryFilters()
                    .TagWith(nameof(SaveDesignerJson))
                    .Where(f => f.IsActive || f.ApprovalStatus == (int)ApprovalStatus.Requested)
                    .OrderByDescending(f => f.Id)
                    .FirstOrDefaultAsync(f => f.ParentId == formParentId);

                List<Form> existingVersions = await Repo.All<Form>()
                    .IgnoreQueryFilters()
                    .Where(f => f.ParentId == formParentId)
                    .Where(f => f.IsActive || f.ApprovalStatus == (int)ApprovalStatus.Requested)
                    .TagWith(nameof(SaveDesignerJson))
                    .ToListAsync();

                var awaitingApprovalVersions = existingVersions.Where(c => !c.IsActive);

                foreach (Form form in awaitingApprovalVersions)
                {
                    form.ApprovalStatus = (int)ApprovalStatus.Rejected;
                }

                if (isApproved)
                {
                    var lastApprovedVersion = existingVersions.SingleOrDefault(f => f.IsActive);

                    //TODO : при минаване на .net9 + да се ползва JsonElement.DeepEquals
                    if (lastApprovedVersion != null &&
                        json.MinifyJson() == lastApprovedVersion.FieldConfiguration.MinifyJson())
                    {
                        if (awaitingApprovalVersions.Any())
                        {
                            await Repo.SaveChangesAsync();
                        }

                        return true;
                    }

                    Repo.DeleteRange(existingVersions);
                }
                else
                {
                    Form lastVersion = existingVersions
                        .OrderByDescending(f => f.Id)
                        .FirstOrDefault();

                    //TODO : при минаване на .net9 + да се ползва JsonElement.DeepEquals
                    if (lastVersion != null &&
                        json.MinifyJson() == lastVersion.FieldConfiguration.MinifyJson())
                    {
                        return true;
                    }
                }

                Form newForm = new Form();

                newForm.Purpose = existingForm.Purpose;
                newForm.ParentId = existingForm.ParentId;
                newForm.ModifiedOn = DateTime.UtcNow;
                newForm.FieldConfiguration = json;
                newForm.Title = formTitle;
                newForm.ModifiedByUserId = _userContext.UserId;
                newForm.ApprovalStatus =
                    isApproved ? (int)ApprovalStatus.Approved : (int)ApprovalStatus.Requested;
                newForm.IsActive = isApproved;

                await Repo.AddAsync(newForm);
                await Repo.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Проблем при запис на JSON конфигурация на форма в {nameof(SaveDesignerJson)}");
                return false;
            }
        }

        /// <summary>
        /// Извличане на форма по идентификатор
        /// </summary>
        /// <param name="id">Идентификатор на форма</param>
        /// <returns></returns>
        public async Task<Form> GetFormById(int id)
        {
            try
            {
                Form form = await Repo.All<Form>()
                .TagWith(nameof(GetFormById))
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(f => f.Id == id);

                return form;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Проблем при извличане на форма с идентификатор: {id} в {nameof(GetFormById)}");
                return null;
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
                Form form = await Repo.All<Form>()
                    .TagWith(nameof(DeleteForm))
                    .IgnoreQueryFilters()
                    .SingleOrDefaultAsync(f => f.Id == id);

                if (form == null)
                {
                    return new OperationResult($"Активна форма с идентификатор {id} не е открита");
                }

                if (form.ApprovalStatus == (int)ApprovalStatus.Requested)
                {
                    form.ApprovalStatus = (int)ApprovalStatus.Rejected;
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
                Form savedForm = await Repo.All<Form>()
                    .TagWith(nameof(EditForm))
                    .IgnoreQueryFilters()
                    .Where(f => f.IsActive || f.ApprovalStatus == (int)ApprovalStatus.Requested)
                    .OrderByDescending(f => f.Id)
                    .FirstOrDefaultAsync(f => f.ParentId == model.ParentId);

                if (savedForm == null)
                {
                    return new SaveOperationResult("Формата не е намерена в базата данни");
                }

                Form newForm = new Form()
                {
                    ParentId = model.ParentId,
                    Title = model.FormTitle,
                    Purpose = model.Purpose,
                    FieldConfiguration = savedForm.FieldConfiguration,
                    ModifiedOn = DateTime.UtcNow,
                    ModifiedByUserId = _userContext.UserId,
                    ApprovalStatus = savedForm.ApprovalStatus,
                    IsActive = savedForm.IsActive
                };


                if (savedForm.IsActive)
                {
                    await Repo.DeleteAsync<Form>(savedForm.Id);
                }
                else
                {
                    savedForm.ApprovalStatus = (int)ApprovalStatus.Rejected;
                }

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
                    .IgnoreQueryFilters()
                    .Where(f => f.ApprovalStatus != (int)ApprovalStatus.Rejected)
                    .OrderByDescending(f => f.Id)
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
                    IsActive = false,
                    Purpose = model.Purpose!,
                    Title = model.FormTitle!,
                    ModifiedByUserId = _userContext.UserId,
                    ModifiedOn = DateTime.UtcNow,
                    FieldConfiguration = JsonSerializer.Serialize(new List<FormField>()),
                    ApprovalStatus = (int)ApprovalStatus.Requested
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

        /// <summary>
        /// Връща модел на форма от записани данни
        /// </summary>
        /// <param name="mpris">Идентификатори на партида</param>
        /// <param name="processId">Идентификатор на заявлението</param>      
        /// <returns></returns>
        public async Task<FormViewModel> GetFormModelForSavedData(IEnumerable<Guid> mpris, Guid processId)
        {
            try
            {
                var publicDataOnly = !mpris.Any() || !mpris.Contains((await Repo.GetByIdAsync<Process>(processId)).MpriId);

                var process = await Repo.AllReadonly<Process>()
                    .IgnoreQueryFilters()
                    .Include(ps => ps.Form)
                    .TagWith(nameof(GetFormModelForSavedData))
                    .Where(s => s.Id == processId)
                    .FirstOrDefaultAsync();

                if (process == null)
                {
                    Logger.LogError($"Не е намерена стъпка с данни за заявена услуга с идентификатор {processId} в {nameof(GetFormModelForSavedData)}");
                    return null;
                }

                if (!process.IsActive && (await _registerService.GetCurrentRegister()).HistoryNotPublic)
                {
                    Logger.LogError($"Опит за достъп до история на заявление в регистър със забранен публичен достъп до история в {nameof(GetFormModelForSavedData)}");
                    return null;
                }

                List<RegisterItem> fieldValues = await Repo.AllReadonly<RegisterItem>()
                    .IgnoreQueryFilters()
                    .Include(ri => ri.Process)
                    .TagWith(nameof(GetFormModelForSavedData))
                    .Where(ri => ri.ProcessId == processId)
                    .Where(ri =>
                        ri.ProcessStepId == ri.Process.ProcessSteps.OrderBy(s => s.OrderNum).LastOrDefault().Id)
                    .ToListAsync();

                FormViewModel viewModel = await GetFormViewModel(process.Form, publicDataOnly);
                if (viewModel == null)
                {
                    return null;
                }

                Dictionary<string, FormField> allPublicFields = new Dictionary<string, FormField>();

                AddValuesOfInterestToDictionary(viewModel.FormFields, allPublicFields, true, true);

                var cachedNomenclatures = await CacheNomenclaturesForValuesOfInterest(allPublicFields);

                DistributeRegisterItemValuesToFormViewModel(fieldValues, viewModel);
                await ResolveFormFieldsValues(viewModel.FormFields, cachedNomenclatures, true);

                if (!string.IsNullOrWhiteSpace(process.OldIncomingNumber))
                {
                    viewModel.FormFields.Add(new FormField()
                    {
                        Name = FormConstants.OldIncomingNumber,
                        Value = process.OldIncomingNumber,
                        Label = "Номер на старо вписване",
                        Type = nameof(SimpleFormFieldType.Text)
                    });
                }

                if (process.OldIncomingDate.HasValue)
                {
                    viewModel.FormFields.Add(new FormField()
                    {
                        Name = FormConstants.OldIncomingDate,
                        Value = process.OldIncomingDate.Value.ConvertUtcToBGTime().ToString(FormattingConstant.NormalDateFormat),
                        Label = "Дата на старо вписване",
                        Type = nameof(SimpleFormFieldType.Text)
                    });
                }

                return viewModel;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Възникна грешка в {nameof(GetFormModelForSavedData)} за заявена услуга с идентификатор {processId}");
                return null;
            }
        }

        /// <summary>
        /// Заменя номенклатурните стойности във полета от FormViewModel с актуалните им текстови стойности
        /// </summary>
        /// <param name="formModelFields">Полета от модел със заредени стойности</param>
        /// <returns></returns>
        public async Task ResolveFormFieldsViewModelValues(IEnumerable<FormField> formModelFields)
        {
            Dictionary<string, FormField> allPublicFields = new Dictionary<string, FormField>();

            AddValuesOfInterestToDictionary(formModelFields, allPublicFields, false, true);

            var cachedNomenclatures = await CacheNomenclaturesForValuesOfInterest(allPublicFields);

            await ResolveFormFieldsValues(formModelFields, cachedNomenclatures, false);
        }

        /// <summary>
        /// Зареждане на конфигурацията на формата на услугата за вписване от базата данни
        /// </summary>
        /// <returns>JSON</returns>
        public async Task<string> ImportRegisterFormConfiguration()
        {
            try
            {
                var formParentId = (await Repo.AllReadonly<Service>()
                        .TagWith(nameof(ImportRegisterFormConfiguration))
                        .SingleOrDefaultAsync(s => s.ServiceTypeId == (int)ServiceTypes.Register))?
                    .FormParentId;

                if (!formParentId.HasValue)
                {
                    return String.Empty;
                }

                return await this.LoadDesignerJson(formParentId.Value);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Проблем в {nameof(ImportRegisterFormConfiguration)}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Връща идентификатора на формата родител на услугата за вписване
        /// </summary>
        /// <returns>Идентификатор или null ако не е намерен резултат</returns>
        public async Task<int?> GetFormParentIdOfTheRegisterService()
        {
            try
            {
                int? result = (await Repo.AllReadonly<Service>()
                    .TagWith(nameof(GetFormParentIdOfTheRegisterService))
                    .SingleOrDefaultAsync(p => p.ServiceTypeId == (int)ServiceTypes.Register))?
                    .FormParentId;

                return result;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Проблем в {nameof(GetFormParentIdOfTheRegisterService)}");
                return 0;
            }
        }

        /// <summary>
        /// Разпределя стойностите на заредени от базата registryItem-и в съответстващ viewModel на форма
        /// </summary>
        /// <param name="registerItems">Списък с registryItem</param>
        /// <param name="viewModel">viewModel на форма</param>
        public void DistributeRegisterItemValuesToFormViewModel(List<RegisterItem> registerItems, FormViewModel viewModel)
        {
            string repeatingFieldValuePattern = @"^(?<repeaterParentName>[^#]+)#(?<index>\d+)(?:_(?<subfieldName>.*))?$";

            foreach (var item in registerItems)
            {
                #region За повтарящи се елементи добавени от потребителя във формата

                Match match = Regex.Match(item.Name, repeatingFieldValuePattern);
                if (match.Success)
                {
                    try
                    {
                        _formFieldsLayoutService.HandleValueDistributionForRepeatingValues(viewModel, match, item.Name, item.Value);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e, $"Проблем в {nameof(_formFieldsLayoutService.HandleValueDistributionForRepeatingValues)}");
                    }
                    continue;
                }

                #endregion

                _formFieldsLayoutService.AssignPostedElementValueToFormField(item.Name, item.Value!, viewModel.FormFields);
            }

            ArrangeRepeatingFieldsSubfieldsInTheCorrectOrder(viewModel);
        }

        private static void ArrangeRepeatingFieldsSubfieldsInTheCorrectOrder(FormViewModel viewModel)
        {
            foreach (FormField originalField in viewModel.FormFields)
            {
                foreach (FormField clone in originalField.Repetitions!)
                {
                    int clonedFieldCorrectIndex = 0;
                    for (int originalSubfieldIndex = 0; originalSubfieldIndex < originalField.Fields.Count; originalSubfieldIndex++)
                    {
                        var clonedSubfield = clone.Fields.FirstOrDefault((f =>
                            f.Label == originalField.Fields[originalSubfieldIndex].Label));

                        if (clonedSubfield != null)
                        {
                            if (clone.Fields.IndexOf(clonedSubfield) != clonedFieldCorrectIndex)
                            {

                                int clonedFieldCurrentIndex = clone.Fields.IndexOf(clonedSubfield);
                                var fieldInWrongPlace = clone.Fields[clonedFieldCorrectIndex];

                                clone.Fields[clonedFieldCorrectIndex] = clonedSubfield;
                                clone.Fields[clonedFieldCurrentIndex] = fieldInWrongPlace;
                            }

                            clonedFieldCorrectIndex++;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Връща списък с всички услуги
        /// </summary>
        /// <param name="skip">Колко записа да бъдат пропуснати</param>
        /// <param name="take">Колко записа да бъдат взети</param>
        /// <returns></returns>
        public async Task<JsonResult> GetAllServiceList(int skip, int take)
        {
            try
            {
                var result = await Repo.AllReadonly<Service>()
                .TagWith(nameof(GetAllServiceList))
                .Select(s => new { s.Id, s.Title, s.ServiceTypeId, s.FormParentId })
                .Skip(skip)
                .Take(take)
                .ToListAsync();

                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Проблем в {nameof(GetAllServiceList)}");
                return null;
            }
        }

        /// <summary>
        /// Връща списък с приключени заявени услуги от тип Вписване
        /// </summary>
        ///
        /// <param name="administrationId">Идентификатор на администрация</param>
        /// <param name="skip">Колко записа да бъдат пропуснати</param>
        /// <param name="take">Колко записа да бъдат взети</param>
        /// <param name="searchKey">Критерии за търсене</param>
        /// <param name="searchPattern">Низ зая търсене</param>
        /// <param name="toDate">До дата на вписване, включително</param>
        /// <param name="fromDate">От дата на вписване, включително</param>
        /// <returns></returns>
        public async Task<JsonResult> GetRegistrationProcessListWhereSubfieldsAreColumns(Guid administrationId,
            int skip,
            int take,
            string searchKey,
            string searchPattern, DateTime? toDate, DateTime? fromDate)
        {
            try
            {
                bool historyNotPublic = (await _registerService.GetCurrentRegister()).HistoryNotPublic;

                int? processFormParentId = (await Repo.AllReadonly<Service>()
                .TagWith(nameof(GetRegistrationProcessListWhereSubfieldsAreColumns))
                    .Where(p => p.ServiceTypeId == (int)ServiceTypes.Register)
                    .SingleOrDefaultAsync())?.FormParentId;

                if (processFormParentId == null)
                {
                    Logger.LogError($"Не намерена форма в {nameof(GetRegistrationProcessListWhereSubfieldsAreColumns)} за администрация с идентификатор {administrationId}");
                    return new JsonResult(new List<object>());
                }

                FormViewModel formModel = await GetFormViewModel(processFormParentId.Value);

                Dictionary<string, FormField> valuesOfInterest = new Dictionary<string, FormField>();

                AddValuesOfInterestToDictionary(formModel.FormFields, valuesOfInterest, true, false, false, true);

                var columnData = valuesOfInterest
                    .Select(v => new { label = v.Value.Label, fieldName = v.Key }).ToList();

                var searchOptions = columnData.ToArray();

                columnData.Add(new { label = "Дата на вписване", fieldName = "IncomingDate" });
                //columnData.Add(new { label = "Идентификатор на процес", fieldName = "ProcessId" });

                var cachedNomenclatures = await CacheNomenclaturesForValuesOfInterest(valuesOfInterest);

                bool isSearchPatternValid = TryDetermineSearchPattern(searchKey,
                    searchPattern,
                    valuesOfInterest,
                    cachedNomenclatures,
                    out List<string> searchPatterns);

                if (!isSearchPatternValid)
                {
                    var emptyData = new
                    {
                        searchOptions = new object[] { },
                        columnData = new object[] { },
                        data = new object[] { },
                        metadata = new { totalRecordsFiltered = 0 }
                    };

                    return new JsonResult(emptyData);
                }

                DateTime? toDateUTC = toDate.HasValue ? toDate.Value.ToUniversalTime().AddDays(1) : null;
                DateTime? fromDateUTC = fromDate.HasValue ? fromDate.Value.ToUniversalTime() : null;

                var processesOfInterest =
                    GetRegistrationProcessListQuery(administrationId, searchKey, toDateUTC, fromDateUTC, valuesOfInterest, searchPatterns);

                int pageNumber;
                int perPage;
                if (take == 0)
                {
                    perPage = take = 20;
                    skip = 0;
                    pageNumber = 1;
                }
                else
                {
                    perPage = take;
                    pageNumber = skip / take + 1;
                }

                var processesForPage = processesOfInterest
                    .Skip(skip)
                    .Take(take);

                var fieldValues = await processesForPage
                    .SelectMany(p => p.RegisterItems.Select(ri => new ProcessFieldValue
                    {
                        Value = ri.Value,
                        Name = ri.Name,
                        ProcessId = ri.ProcessId,
                        IncomingDate = ri.Process.IncomingDate,
                        RegisterNumber = ri.Process.RegisterNumber,
                        RegisterDate = ri.Process.ModifiedOn
                    }))
                    .Where(ri => valuesOfInterest.Keys.Contains(ri.Name))
                    .GroupBy(ri => ri.ProcessId)
                    .ToListAsync();

                var result = await ResolveRegisterItemValues(fieldValues, valuesOfInterest, cachedNomenclatures, true);

                result = result.OrderByDescending(d => (DateTime)d[nameof(Process.IncomingDate)]).ToList();

                var combinedData = new
                {
                    searchOptions = searchOptions,
                    columnData = columnData,
                    data = result,
                    metadata = new
                    {
                        totalRecordsFiltered = await processesOfInterest.CountAsync(),
                        pageNumber = pageNumber,
                        perPage = perPage,
                        historyNotPublic
                    }
                };

                return new JsonResult(combinedData);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Възникна грешка в {nameof(GetRegistrationProcessListWhereSubfieldsAreColumns)} за администрация с идентификатор {administrationId}");
                var combinedData = new
                {
                    searchOptions = new object[] { },
                    columnData = new object[] { },
                    data = new object[] { },
                    metadata = new { totalRecordsFiltered = 0 }
                };

                return new JsonResult(combinedData);
            }
        }

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
        public async Task<(JsonResult, List<Dictionary<string, object>>, Dictionary<string, FormField>)> GetRegistrationProcessListWhereSubfieldsAreConcatenated(Guid administrationId,
            int skip,
            int take,
            string searchKey,
            string searchPattern,
            DateTime? toDate,
            DateTime? fromDate,
            DateTime? searchToDate,
            DateTime? searchFromDate)
        {
            try
            {
                bool historyNotPublic = (await _registerService.GetCurrentRegister()).HistoryNotPublic;

                int? processFormParentId = (await Repo.AllReadonly<Service>()
                .TagWith(nameof(GetRegistrationProcessListWhereSubfieldsAreConcatenated))
                    .Where(p => p.ServiceTypeId == (int)ServiceTypes.Register)
                    .SingleOrDefaultAsync())?.FormParentId;

                if (processFormParentId == null)
                {
                    Logger.LogError($"Не намерена форма в {nameof(GetRegistrationProcessListWhereSubfieldsAreConcatenated)} за администрация с идентификатор {administrationId}");
                    return (new JsonResult(new List<object>()), new List<Dictionary<string, object>>(), new Dictionary<string, FormField>());
                }

                FormViewModel formModel = await GetFormViewModel(processFormParentId.Value);

                Dictionary<string, FormField> valuesOfInterestSearchCriteria = new Dictionary<string, FormField>();
                Dictionary<string, FormField> valuesOfInterestFinal = new Dictionary<string, FormField>();

                AddValuesOfInterestToDictionary(formModel.FormFields,
                    valuesOfInterestSearchCriteria,
                    true,
                    false,
                    false,
                    true);

                AddValuesOfInterestToDictionary(formModel.FormFields,
                    valuesOfInterestFinal,
                    true,
                    false,
                    true);

                var columnData = valuesOfInterestFinal
                    .Select(v => new { label = v.Value.Label, fieldName = v.Key }).ToList();
                var searchOptions = valuesOfInterestSearchCriteria
                    .Select(v => new { label = v.Value.Label, fieldName = v.Key, type = v.Value.Type });

                bool includeOldRegisteredFilters = await Repo.AllReadonly<Process>()
                    .AnyAsync(p => p.OldIncomingDate.HasValue || !string.IsNullOrWhiteSpace(p.OldIncomingNumber));

                searchOptions =
                    searchOptions.Append(new
                    {
                        label = "Номер на вписване",
                        fieldName = FormConstants.RegisterNumber,
                        type = nameof(SimpleFormFieldType.Text)
                    });


                if (includeOldRegisteredFilters)
                {
                   
                    searchOptions = searchOptions.Append(new
                    {
                        label = "Стара дата на вписване",
                        fieldName = FormConstants.OldIncomingDate,
                        type = nameof(SimpleFormFieldType.Date)
                    });

                    searchOptions =
                        searchOptions.Append(new
                        {
                            label = "Стар номер на вписване",
                            fieldName = FormConstants.OldIncomingNumber,
                            type = nameof(SimpleFormFieldType.Text)
                        });
                }

                columnData.Insert(0, new { label = "Дата на последна промяна", fieldName = "ModifiedOn" });
                columnData.Insert(0, new { label = "Номер на вписване", fieldName = "RegisterNumber" });

                var cachedNomenclatures = await CacheNomenclaturesForValuesOfInterest(valuesOfInterestSearchCriteria);

                List<string> searchPatterns = null;

                bool isSearchPatternValid = (string.IsNullOrWhiteSpace(searchKey) && (searchFromDate != null || searchToDate != null)) ||
                                            TryDetermineSearchPattern(searchKey,
                                            searchPattern,
                                            valuesOfInterestSearchCriteria,
                                            cachedNomenclatures,
                                            out searchPatterns);

                if (!isSearchPatternValid)
                {
                    var emptyData = new
                    {
                        searchOptions = new object[] { },
                        columnData = new object[] { },
                        data = new object[] { },
                        metadata = new { totalRecordsFiltered = 0 }
                    };

                    return (new JsonResult(emptyData), new List<Dictionary<string, object>>(), valuesOfInterestSearchCriteria);
                }

                DateTime? toDateUTC = toDate?.ToUniversalTime().AddDays(1);
                DateTime? fromDateUTC = fromDate?.ToUniversalTime();

                DateTime? searchToDateUTC = searchToDate?.ToUniversalTime().AddDays(1);
                DateTime? searchFromDateUTC = searchFromDate?.ToUniversalTime();

                var processesOfInterest =
                    GetRegistrationProcessListQuery(administrationId,
                        searchKey,
                        toDateUTC,
                        fromDateUTC,
                        valuesOfInterestSearchCriteria,
                        searchPatterns,
                        searchToDateUTC,
                        searchFromDateUTC);

                int pageNumber;
                int perPage;
                if (take == 0)
                {
                    perPage = take = 20;
                    skip = 0;
                    pageNumber = 1;
                }
                else
                {
                    perPage = take;
                    pageNumber = skip / take + 1;
                }

                var processesForPage = processesOfInterest
                    .Skip(skip)
                    .Take(take);

                var fieldValues = await processesForPage
                    .SelectMany(p => p.RegisterItems
                        .Select(ri => new ProcessFieldValue
                        {
                            Value = ri.Value,
                            Name = ri.Name,
                            ProcessId = ri.ProcessId,
                            RegisterNumber = ri.Process.RegisterNumber,
                            RegisterDate = ri.Process.RegisterDate.HasValue ? ri.Process.RegisterDate.ConvertUtcToBGTime() : null,
                            ServiceTypeId = ri.Process.Service.ServiceTypeId
                        }))
                    .Where(ri => valuesOfInterestSearchCriteria.Keys.Contains(ri.Name))
                    .GroupBy(ri => ri.ProcessId)
                    .ToListAsync();

                var result = await ResolveRegisterItemValues(fieldValues, valuesOfInterestSearchCriteria, cachedNomenclatures, true);

                result = result
                    .OrderByDescending(d =>
                        d.TryGetValue(nameof(Process.RegisterDate), out var val) && val is DateTime dt
                            ? (DateTime?)dt
                            : null)
                    .ToList();

                List<Dictionary<string, object>> concatenatedResult = new List<Dictionary<string, object>>();
                foreach (Dictionary<string, object> dictionary in result)
                {
                    Dictionary<string, object> entry = new Dictionary<string, object>();
                    concatenatedResult.Add(entry);

                    foreach (string key in valuesOfInterestFinal.Keys)
                    {
                        if (dictionary.ContainsKey(key))
                        {
                            entry.Add(key, dictionary[key]);
                        }
                        else
                        {
                            var subfieldValues = dictionary.Where(d => d.Key.StartsWith(key + '_')
                                                                       && d.Value != null && !string.IsNullOrWhiteSpace(
                                                                           d.Value.ToString()))
                                .ToDictionary(d => d.Key, d => d.Value);



                            entry.Add(key, JoinSubFieldValues(", ", subfieldValues));
                        }
                    }

                    entry.Add(nameof(RegisterItem.ProcessId), dictionary[nameof(RegisterItem.ProcessId)]);
                    entry.Add(nameof(RegisterItem.Process.RegisterNumber), dictionary[nameof(RegisterItem.Process.RegisterNumber)]);
                    entry.Add(nameof(RegisterItem.Process.RegisterDate), dictionary[nameof(RegisterItem.Process.RegisterDate)]);
                    entry.Add(nameof(RegisterItem.Process.Service.ServiceTypeId), dictionary[nameof(RegisterItem.Process.Service.ServiceTypeId)]);
                }

                var combinedData = new
                {
                    searchOptions = searchOptions.ToArray(),
                    columnData = columnData,
                    data = concatenatedResult,
                    metadata = new
                    {
                        totalRecordsFiltered = await processesOfInterest.CountAsync(),
                        pageNumber = pageNumber,
                        perPage = perPage,
                        historyNotPublic
                    }
                };

                return (new JsonResult(combinedData), concatenatedResult, valuesOfInterestFinal);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Възникна грешка в {nameof(GetRegistrationProcessListWhereSubfieldsAreConcatenated)} за администрация с идентификатор {administrationId}");
                var combinedData = new
                {
                    searchOptions = new object[] { },
                    columnData = new object[] { },
                    data = new object[] { },
                    metadata = new { totalRecordsFiltered = 0 }
                };

                return (new JsonResult(combinedData), new List<Dictionary<string, object>>(), new Dictionary<string, FormField>());
            }
        }

        private static string JoinSubFieldValues(string separator, Dictionary<string, object> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            if (separator == null)
                separator = string.Empty;

            using (var enumerator = values.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    return string.Empty;

                string firstValue = enumerator.Current.Value.ToString() ?? string.Empty;
                StringBuilder result = new StringBuilder(firstValue);
                string previousFieldName = enumerator.Current.Key ?? string.Empty;

                while (enumerator.MoveNext())
                {
                    string currentValue = enumerator.Current.Value.ToString() ?? string.Empty;
                    string currentName = enumerator.Current.Key ?? string.Empty;

                    // Use space as separator if previous label ends with "firstNameImmutable" and current label ends with "lastNameImmutable"
                    string effectiveSeparator = (previousFieldName.EndsWith(ComplexFieldsNameConstants.FirstNameImmutable, StringComparison.OrdinalIgnoreCase)
                                                 && currentName.EndsWith(ComplexFieldsNameConstants.LastNameImmutable, StringComparison.OrdinalIgnoreCase))
                        ? " "
                        : separator;

                    result.Append(effectiveSeparator);
                    result.Append(currentValue);
                    previousFieldName = currentName;
                }

                return result.ToString();
            }
        }

        private IOrderedQueryable<Process> GetRegistrationProcessListQuery(Guid administrationId,
            string searchKey,
            DateTime? toDateUTC,
            DateTime? fromDateUTC,
            Dictionary<string, FormField> valuesOfInterest,
            List<string> searchPatterns,
            DateTime? searchToDateUTC = null,
            DateTime? searchFromDateUTC = null)
        {

            var query = Repo.AllReadonly<Process>()
                .TagWith(nameof(GetRegistrationProcessListWhereSubfieldsAreColumns))
                .Include(p => p.Service)
                .Where(p => p.Service.ServiceTypeId == (int)ServiceTypes.Register ||
                            p.Service.ServiceTypeId == (int)ServiceTypes.Change ||
                            p.Service.ServiceTypeId == (int)ServiceTypes.AskForCorrectionError ||
                            p.Service.ServiceTypeId == (int)ServiceTypes.Deletion)
                .Where(p => p.StatusId == (int)ProcessStatus.Registered)
                .Where(p => p.TenantId == administrationId)
                .Where(p => !toDateUTC.HasValue || !p.RegisterDate.HasValue || p.RegisterDate < toDateUTC)
                .Where(p => !fromDateUTC.HasValue || !p.RegisterDate.HasValue || p.RegisterDate >= fromDateUTC);

            if (searchToDateUTC != null || searchFromDateUTC != null)
            {
                if (searchKey == FormConstants.OldIncomingDate)
                {
                    query = query.Where(p => p.RegisterItems.Any(ri => valuesOfInterest.Keys.Contains(ri.Name) &&
                                                                       ri.ProcessStepId == ri.Process.ProcessSteps
                                                                           .OrderBy(s => s.OrderNum).LastOrDefault().Id))
                                .Where(p => p.OldIncomingDate.HasValue &&
                                             ((!searchToDateUTC.HasValue || p.OldIncomingDate.Value < searchToDateUTC) &&
                                              (!searchFromDateUTC.HasValue || p.OldIncomingDate.Value >= searchFromDateUTC)));
                }
                else
                {

                    query = query.Where(p => p.RegisterItems.Any(ri => valuesOfInterest.Keys.Contains(ri.Name) &&
                                                                       ri.ProcessStepId == ri.Process.ProcessSteps
                                                                           .OrderBy(s => s.OrderNum).LastOrDefault().Id
                                                                       && (searchToDateUTC == null ||
                                                                           ri.DateTimeValue < searchToDateUTC)
                                                                       && (searchFromDateUTC == null ||
                                                                           ri.DateTimeValue >= searchFromDateUTC)
                    ));
                }
            }
            else // searchPatterns != null
            {
                if (searchKey == FormConstants.RegisterNumber)
                {
                    query = query.Where(p => searchPatterns.Any(sp => p.RegisterNumber == sp));
                }
                else if (searchKey == FormConstants.OldIncomingNumber)
                {
                    query = query.Where(p => searchPatterns.Any(sp => p.OldIncomingNumber == sp));
                }
                else
                {
                    query = query.Where(p => p.RegisterItems.Any(ri => valuesOfInterest.Keys.Contains(ri.Name) &&
                                                                       ri.ProcessStepId == ri.Process.ProcessSteps
                                                                           .OrderBy(s => s.OrderNum).LastOrDefault().Id
                                                                       && (!searchPatterns.Any() ||
                                                                           searchPatterns.Any(sp =>
                                                                               ri.Name == searchKey &&
                                                                               EF.Functions.ILike(ri.Value, sp)
                                                                           ))
                    ));
                }
            }

            var processesOfInterest = query
                .OrderByDescending(p => p.RegisterDate);
            return processesOfInterest;
        }

        /// <summary>
        /// Връща списък със заявени услуги за лице
        /// </summary>
        /// <param name="mpris">Идентификатори на партида</param>
        /// <param name="roleInProcessType">Роля в заявената услуга</param>
        /// <param name="skip">Колко записа да бъдат пропуснати</param>
        /// <param name="take">Колко записа да бъдат взети</param>
        /// <returns></returns>
        public async Task<JsonResult> GetProcessList(IEnumerable<Guid> mpris, int roleInProcessType, int skip, int take)
        {
            try
            {
                var result = await Repo.AllReadonly<Process>()
                    .Include(p => p.Service)
                .TagWith(nameof(GetProcessList))
                .Where(ri => roleInProcessType != (int)ProcessRole.MasterRecordOwner || mpris.Contains(ri.MpriApplicantId))
                .Where(ri => roleInProcessType != (int)ProcessRole.Submitter || mpris.Contains(ri.MpriId))
                .Select(p => new { p.IncomingNumber, status = ((ProcessStatus)p.StatusId).GetDescription(), p.Service.Title })
                .Skip(skip)
                .Take(take)
                .ToListAsync();

                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Проблем в {nameof(GetProcessList)}");
                return null;
            }
        }

        /// <summary>
        /// Връща модел на форма от записани данни
        /// </summary>
        /// <param name="mpris">Идентификатори на партида</param>
        /// <param name="serviceId">Идентификатор на услугата, за която се извличат данни</param>
        /// <param name="skip">Колко записа да бъдат пропуснати</param>
        /// <param name="take">Колко записа да бъдат взети</param>
        /// <returns></returns>
        public async Task<JsonResult> GetDataForPublicTable(IEnumerable<Guid> mpris, int serviceId, int skip, int take)
        {
            try
            {
                var processFormParentId = (await Repo.AllReadonly<Process>()
                        .Include(p => p.Form)
                        .TagWith(nameof(GetDataForPublicTable))
                        .IgnoreQueryFilters()
                        .Where(p => p.ServiceId == serviceId)
                        .OrderBy(p => p.IncomingDate)
                        .LastOrDefaultAsync())
                        .Form.ParentId;

                if (processFormParentId == null)
                {
                    Logger.LogError($"Не намерена форма в {nameof(GetDataForPublicTable)} за заявена услуга с идентификатор {serviceId}");
                    return null;
                }

                FormViewModel formModel = await GetFormViewModel(processFormParentId.Value);

                Dictionary<string, FormField> valuesOfInterest = new Dictionary<string, FormField>();

                AddValuesOfInterestToDictionary(formModel.FormFields, valuesOfInterest);

                var fieldValues = await Repo.AllReadonly<RegisterItem>()
                    .Include(ri => ri.Process)
                    .TagWith(nameof(GetDataForPublicTable))
                    .Where(ri => ri.Process.StatusId == (int)ProcessStatus.Registered)
                    .Where(ri => ri.Process.ServiceId == serviceId)
                    .Where(ri => !mpris.Any() || mpris.Contains(ri.Process.MpriId))
                    .Where(ri => valuesOfInterest.Keys.Contains(ri.Name))
                    .Where(ri =>
                        ri.ProcessStepId == ri.Process.ProcessSteps.OrderBy(s => s.OrderNum).LastOrDefault().Id)
                    .Select(ri => new
                    {
                        //ri.Name,
                        Label = valuesOfInterest[ri.Name].Label,
                        ri.Value,
                        ri.ProcessId
                    })
                    .GroupBy(ri => ri.ProcessId)

                    .ToListAsync();

                return new JsonResult(fieldValues);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Възникна грешка в {nameof(GetDataForPublicTable)} за заявена услуга с идентификатор {serviceId}");
                return null;
            }
        }

        /// <summary>
        /// Одобрение на конфигурация
        /// </summary>
        /// <param name="formId"></param>
        /// <returns></returns>
        public async Task<OperationResult> ApproveConfiguration(int formId)
        {
            try
            {
                Form form = await Repo.All<Form>()
                    .TagWith(nameof(ApproveConfiguration))
                    .IgnoreQueryFilters()
                    .Where(f => f.Id == formId)
                    .SingleOrDefaultAsync();

                if (form == null)
                {
                    return new OperationResult("Не е намерена форма за идентификатора");
                }

                if (form.ApprovalStatus != (int)ApprovalStatus.Requested)
                {
                    return new OperationResult("Конфигурацията е вече одобрена или отхвърлена");
                }

                form.IsActive = true;
                form.ApprovalStatus = (int)ApprovalStatus.Approved;
                form.ModifiedByUserId = _userContext.UserId;

                List<Form> activeFormsWithSameParentId = await Repo.All<Form>()
                    .TagWith(nameof(ApproveConfiguration))
                    .Where(f => f.ParentId == form.ParentId)
                    .ToListAsync();

                Repo.DeleteRange(activeFormsWithSameParentId);
                await Repo.SaveChangesAsync();

                return new OperationResult();
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Проблем при одобрение на конфигурация в {nameof(ApproveConfiguration)} с идентификатор на форма {formId}");
                return new OperationResult("Грешка при промяна в базата данни");
            }
        }

        internal static void AddValuesOfInterestToDictionary(
            IEnumerable<FormField> formModelFields,
            Dictionary<string, FormField> valuesOfInterest,
            bool onlyPublic = true,
            bool interestedInAll = false,
            bool atComplexFieldLevel = false,
            bool addParentName = false,           
            string parentName = "")
        {
            foreach (FormField formModelFormField in formModelFields
                         .Where(f =>
                             (interestedInAll || f.IsColumnInDataTable) &&
                             (!onlyPublic || f.IsPublic)))
            {
                if (atComplexFieldLevel || !formModelFormField.Fields.Any())
                {
                    if (addParentName)
                    {
                        AddParentLabelToFieldLabel(parentName, formModelFormField);
                    }

                    valuesOfInterest.Add(formModelFormField.Name, formModelFormField);
                }

                if (!atComplexFieldLevel)
                {
                    AddValuesOfInterestToDictionary(formModelFormField.Fields, valuesOfInterest, onlyPublic,
                        interestedInAll, atComplexFieldLevel,  addParentName, formModelFormField.Label);
                }

                AddValuesOfInterestToDictionary(formModelFormField.Repetitions, 
                    valuesOfInterest, 
                    onlyPublic, 
                    interestedInAll, 
                    atComplexFieldLevel, 
                    addParentName,
                    parentName);
            }
        }

        private static void AddParentLabelToFieldLabel(string parentLabel, FormField field)
        {
            if (!string.IsNullOrWhiteSpace(parentLabel))
            {
                field.Label = parentLabel + '➤' + field.Label;
            }
        }

        /// <summary>
        /// Взимане име на записан файл
        /// </summary>
        /// <param name="fileKey">Ключ на записания файл</param>
        /// <returns></returns>
        public async Task<string> GetStoredFileName(Guid fileKey)
        {
            try
            {
                string result = await Repo.AllReadonly<FileMetadata>()
                    .TagWith(nameof(GetStoredFileName))
                    .Where(f => f.FileId == fileKey)
                    .Select(f => f.FileName)
                    .SingleOrDefaultAsync();

                if (string.IsNullOrWhiteSpace(result))
                {
                    Logger.LogError($"Файл с ключ {fileKey} не е намерен");
                    return MessageConstant.Values.FileNotFound;
                }

                return result;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Проблем при намирането на име на файл с ключ {fileKey}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Връща модел на формата по родителски идентификатор
        /// </summary>
        /// <param name="formId">Идентификатор на формата</param>
        /// <returns></returns>
        public async Task<FormViewModel> GetFormViewModelByFormId(int formId)
        {
            try
            {
                Form savedForm = await Repo.AllReadonly<Form>()
                    .TagWith(nameof(GetFormViewModel))
                    .IgnoreQueryFilters()
                    .Where(f => f.Id == formId)
                    .SingleOrDefaultAsync();

                if (savedForm == null)
                {
                    Logger.LogError($"Не е намерена форма с родителски идентификатор {formId} в {nameof(GetFormViewModelByFormId)}");
                    return null;
                }

                return await GetFormViewModel(savedForm);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Проблем при генериране на view model в {nameof(GetFormViewModel)} за {nameof(formId)} {formId}");
                return new FormViewModel
                {
                    FormFields = new List<FormField>()
                };
            }
        }

        /// <summary>
        /// Връща списък със записи по услуга
        /// </summary>
        /// <param name="serviceId">Идентификатор на услуга</param>
        /// <param name="customViewId">Идентификатор на справка</param>
        /// <param name="request"></param>
        /// <param name="filter">Филтър за търсене</param>
        /// <returns></returns>
        public async Task<IActionResult> GetTableDataForService(int serviceId, int customViewId, IDataTablesRequest request,
            CustomTableViewViewModel filter)
        {
            try
            {
                int? processFormParentId;

                if (serviceId == 0)
                {
                    processFormParentId = (await Repo.AllReadonly<Service>()
                            .TagWith(nameof(GetTableDataForService))
                            .SingleOrDefaultAsync(s => s.ServiceTypeId == (int)ServiceTypes.Register))?
                        .FormParentId;
                }
                else
                {
                    processFormParentId = (await Repo.AllReadonly<Service>()
                        .TagWith(nameof(GetTableDataForService))
                        .IgnoreQueryFilters()
                        .Where(p => p.Id == serviceId)
                        .SingleOrDefaultAsync())?.FormParentId;
                }

                if (processFormParentId == null)
                {
                    Logger.LogError($"Не намерена форма в {nameof(GetTableDataForService)} за заявена услуга с идентификатор {serviceId}");
                    return request.GetResponseServerPaging(new List<object>(), 0);
                }

                FormViewModel formModel = await GetFormViewModel(processFormParentId.Value);

                Dictionary<string, FormField> valuesOfInterest = new Dictionary<string, FormField>();

                if (customViewId == 0)
                {
                    AddValuesOfInterestToDictionary(formModel.FormFields, valuesOfInterest, false);
                }
                else
                {
                    var customViewColumns = (await GetCustomViewViewModel(customViewId)).SelectedColumns;
                    AddValuesOfInterestToDictionary(formModel.FormFields, valuesOfInterest, customViewColumns);
                }

                var cachedNomenclatures = await CacheNomenclaturesForValuesOfInterest(valuesOfInterest);

                bool isSearchPatternValid = TryDetermineSearchPattern(filter.FieldName,
                    filter.SearchPattern,
                    valuesOfInterest,
                    cachedNomenclatures,
                    out List<string> searchPatterns);

                if (!isSearchPatternValid)
                {
                    return request.GetResponseServerPaging(new List<object>(), 0);
                }

                bool isCurrentUserGlobalAdmin = _userContext.IsGlobalAdmin;

                List<Guid> mprids = new List<Guid>();

                if (!string.IsNullOrWhiteSpace(filter.MprId))
                {
                    var requestMPRI = new GetMasterPersonRecordIndexMessage();
                    requestMPRI.Pid = filter.MprId;
                    var responseMPRI = await _registerGrpcClient.GetMasterPersonRecordIndexAsync(requestMPRI);

                    if (responseMPRI.Status.Code != Common.ResultCodes.Ok)
                    {
                        Logger.LogError(
                            $"Грешка {responseMPRI.Status.Code} при извикване на {nameof(_registerGrpcClient.GetMasterPersonRecordIndexAsync)} в {nameof(GetTableDataForService)}.");
                    }

                    mprids.AddRange(responseMPRI.Items.Select(i => new Guid(i.Id)));
                }

                List<Guid> submitterIds = new List<Guid>();

                if (!string.IsNullOrWhiteSpace(filter.SubmitterId))
                {
                    var requestMPRI = new GetMasterPersonRecordIndexMessage();
                    requestMPRI.Pid = filter.SubmitterId;
                    var responseMPRI = await _registerGrpcClient.GetMasterPersonRecordIndexAsync(requestMPRI);

                    if (responseMPRI.Status.Code != Common.ResultCodes.Ok)
                    {
                        Logger.LogError(
                            $"Грешка {responseMPRI.Status.Code} при извикване на {nameof(_registerGrpcClient.GetMasterPersonRecordIndexAsync)} в {nameof(GetTableDataForService)}.");
                    }

                    submitterIds.AddRange(responseMPRI.Items.Select(i => new Guid(i.Id)));
                }

                //Ако търсим по сложлно или повтаряемо поле в поле в справка, материализираме заявката
                if (customViewId > 0 && filter.FieldName != null && valuesOfInterest.ContainsKey(filter.FieldName) &&
                    !string.IsNullOrWhiteSpace(filter.SearchPattern) &&
                    (valuesOfInterest[filter.FieldName].CanBeRepeated ||
                    valuesOfInterest[filter.FieldName].Fields.Any()))
                {
                    var complexFieldSearchResult = await GetCustomViewResultsWhenSearchingForComplexField(serviceId, request, filter, isCurrentUserGlobalAdmin, mprids, submitterIds, valuesOfInterest, cachedNomenclatures);
                    return complexFieldSearchResult;
                }

                var processesOfInterest = Repo.AllReadonly<Process>()
                    .TagWith(nameof(GetTableDataForService))
                    .Where(p => p.StatusId == (int)ProcessStatus.Registered)
                    .Where(p => isCurrentUserGlobalAdmin || p.TenantId == _userContext.AdministrationId)
                    .Where(p => serviceId == 0 || p.ServiceId == serviceId)
                    .Where(p => string.IsNullOrWhiteSpace(filter.IncomingNumber) || EF.Functions.ILike(p.IncomingNumber, "%" + filter.IncomingNumber + "%"))
                    .Where(p => string.IsNullOrWhiteSpace(filter.RegisterNumber) || EF.Functions.ILike(p.RegisterNumber, "%" + filter.RegisterNumber + "%"))
                    .Where(p => !filter.IncomingDateFrom.HasValue || p.IncomingDate >= filter.IncomingDateFrom.Value.ToUniversalTime())
                    .Where(p => !filter.IncomingDateTo.HasValue || p.IncomingDate <= filter.IncomingDateTo.Value.ToUniversalTime().AddDays(1))
                    .Where(p => string.IsNullOrWhiteSpace(filter.MprId) || mprids.Contains(p.MpriId))
                    .Where(p => string.IsNullOrWhiteSpace(filter.SubmitterId) || submitterIds.Contains(p.MpriApplicantId))
                    .Where(p => p.RegisterItems.Any(
                              ri => valuesOfInterest.Keys.Contains(ri.Name) &&
                              ri.ProcessStepId == ri.Process.ProcessSteps.OrderBy(s => s.OrderNum).LastOrDefault().Id
                                 && (!searchPatterns.Any() || searchPatterns.Any(sp =>
                                      ri.Name == filter.FieldName && EF.Functions.ILike(ri.Value, sp)
                                  ))
                    ))
                    .OrderByDescending(p => p.IncomingDate);

                var processesForPage = processesOfInterest
                    .Skip(request.Start)
                    .Take(request.Length);

                var fieldValues = await processesForPage
                    .SelectMany(p => p.RegisterItems.Select(ri => new ProcessFieldValue()
                    {
                        Value = ri.Value,
                        Name = ri.Name,
                        ProcessId = ri.ProcessId,
                        RegisterDate = ri.Process.ModifiedOn
                    }))
                    .Where(ri => valuesOfInterest.Keys.Contains(ri.Name)
                                 || valuesOfInterest.Keys.Any(k => ri.Name.StartsWith(k + "_")
                                                                   || valuesOfInterest.Keys.Any(k => ri.Name.StartsWith(k + "#"))))
                    .GroupBy(ri => ri.ProcessId)
                    .ToListAsync();

                var result = await ResolveRegisterItemValues(fieldValues, valuesOfInterest, cachedNomenclatures, false);

                result = result.OrderByDescending(d =>
                    d.TryGetValue(nameof(Process.RegisterDate), out var val) && val is DateTime dt
                        ? (DateTime?)dt
                        : null).ToList();

                return request.GetResponseServerPaging(result, await processesOfInterest.CountAsync());
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Възникна грешка в {nameof(GetTableDataForService)} за услуга с идентификатор {serviceId}");
                return request.GetResponseServerPaging(new List<object>(), 0);
            }
        }

        private async Task<IActionResult> GetCustomViewResultsWhenSearchingForComplexField(int serviceId, IDataTablesRequest request,
            CustomTableViewViewModel filter, bool isCurrentUserGlobalAdmin, List<Guid> mprids, List<Guid> submitterIds,
            Dictionary<string, FormField> valuesOfInterest, Dictionary<string, Dictionary<string, string>> cachedNomenclatures)
        {
            var processesOfInterest = Repo.AllReadonly<Process>()
                .TagWith(nameof(GetTableDataForService))
                .Where(p => p.StatusId == (int)ProcessStatus.Registered)
                .Where(p => isCurrentUserGlobalAdmin || p.TenantId == _userContext.AdministrationId)
                .Where(p => serviceId == 0 || p.ServiceId == serviceId)
                .Where(p => string.IsNullOrWhiteSpace(filter.IncomingNumber) || EF.Functions.ILike(p.IncomingNumber, "%" + filter.IncomingNumber + "%"))
                .Where(p => string.IsNullOrWhiteSpace(filter.RegisterNumber) || EF.Functions.ILike(p.RegisterNumber, "%" + filter.RegisterNumber + "%"))
                .Where(p => !filter.IncomingDateFrom.HasValue || p.IncomingDate >= filter.IncomingDateFrom.Value.ToUniversalTime())
                .Where(p => !filter.IncomingDateTo.HasValue || p.IncomingDate <= filter.IncomingDateTo.Value.ToUniversalTime().AddDays(1))
                .Where(p => string.IsNullOrWhiteSpace(filter.MprId) || mprids.Contains(p.MpriId))
                .Where(p => string.IsNullOrWhiteSpace(filter.SubmitterId) || submitterIds.Contains(p.MpriApplicantId))
                .Where(p => p.RegisterItems.Any(
                    ri => valuesOfInterest.Keys.Contains(ri.Name) &&
                          ri.ProcessStepId == ri.Process.ProcessSteps.OrderBy(s => s.OrderNum).LastOrDefault().Id
                ))
                .OrderByDescending(p => p.IncomingDate);

            var fieldValues = await processesOfInterest
                .SelectMany(p => p.RegisterItems.Select(ri => new ProcessFieldValue()
                {
                    Value = ri.Value,
                    Name = ri.Name,
                    ProcessId = ri.ProcessId,
                    RegisterDate = ri.Process.ModifiedOn
                }))
                .Where(ri => valuesOfInterest.Keys.Contains(ri.Name)
                             || valuesOfInterest.Keys.Any(k => ri.Name.StartsWith(k + "_")
                                                               || valuesOfInterest.Keys.Any(k => ri.Name.StartsWith(k + "#"))))
                .GroupBy(ri => ri.ProcessId)
                .ToListAsync();

            var result = await ResolveRegisterItemValues(fieldValues, valuesOfInterest, cachedNomenclatures, false);

            result = result.Where(r => (r[filter.FieldName] as string)
                .Contains(filter.SearchPattern, StringComparison.InvariantCultureIgnoreCase))
                .OrderByDescending(d =>
                    d.TryGetValue(nameof(Process.RegisterDate), out var val) && val is DateTime dt
                        ? (DateTime?)dt
                        : null).ToList();

            var resultPage = result
                .Skip(request.Start)
                .Take(request.Length)
                .ToList();

            return request.GetResponseServerPaging(resultPage, result.Count);
        }

        private void AddValuesOfInterestToDictionary(List<FormField> formModelFormFields, Dictionary<string, FormField> valuesOfInterest, List<string> selectedColumns)
        {
            foreach (FormField formModelFormField in formModelFormFields)
            {
                if (selectedColumns.Contains(formModelFormField.Name))
                {
                    valuesOfInterest.Add(formModelFormField.Name, formModelFormField);
                }

                AddValuesOfInterestToDictionary(formModelFormField.Fields, valuesOfInterest, selectedColumns);
                AddValuesOfInterestToDictionary(formModelFormField.Repetitions, valuesOfInterest, selectedColumns);
            }
        }

        private async Task<List<Dictionary<string, object>>> ResolveRegisterItemValues(
            List<IGrouping<Guid, ProcessFieldValue>> fieldValues,
            Dictionary<string, FormField> valuesOfInterest,
            Dictionary<string, Dictionary<string, string>> cachedNomenclatures,
            bool censorSensitiveData)
        {
            bool lookForRegistryNumber = true;
            bool lookForModifiedOn = true;

            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            foreach (var registerItems in fieldValues)
            {
                Dictionary<string, object> jsonFields =
                    valuesOfInterest.Keys.ToDictionary(k => k, k => (object)string.Empty);

                foreach (var valueOfInterestPair in valuesOfInterest)
                {
                    FormField currentField = valueOfInterestPair.Value;
                    var registerItem = registerItems.FirstOrDefault(ri => ri.Name == currentField.Name);
                    if (registerItem == null)
                    {
                        continue;
                    }

                    if (currentField.Fields != null && currentField.Fields.Any())
                    {
                        StringBuilder displayValue = new StringBuilder(
                            await ResolveComplexField(cachedNomenclatures, censorSensitiveData, currentField,
                                registerItems));

                        if (currentField.CanBeRepeated)
                        {
                            int i = 1;
                            var registerItemClone = registerItems.FirstOrDefault(ri => ri.Name == RepeatedFormFieldHelperService.InsertBeforeFirstUnderscore(currentField.Name, "#" + i));

                            while (registerItemClone != null)
                            {
                                var complexFieldValue = await ResolveComplexField(cachedNomenclatures,
                                    censorSensitiveData, currentField, registerItems, i);
                                displayValue.Append(Environment.NewLine);
                                displayValue.Append(complexFieldValue);
                                i++;

                                registerItemClone =
                                    registerItems.FirstOrDefault(ri =>
                                        ri.Name == RepeatedFormFieldHelperService.InsertBeforeFirstUnderscore(
                                            currentField.Name, "#" + i));
                            }
                        }

                        jsonFields[currentField.Name] = displayValue.ToString();
                    }
                    else
                    {
                        string resolvedValue = await ResolveRegisterItemValue(
                            currentField, cachedNomenclatures, censorSensitiveData, registerItem);
                        StringBuilder displayValue =
                            new StringBuilder(string.IsNullOrWhiteSpace(resolvedValue) ? string.Empty : resolvedValue);

                        if (currentField.CanBeRepeated)
                        {
                            int i = 1;
                            var registerItemClone = registerItems.FirstOrDefault(ri => ri.Name == RepeatedFormFieldHelperService.InsertBeforeFirstUnderscore(currentField.Name, "#" + i));

                            while (registerItemClone != null)
                            {
                                resolvedValue = await ResolveRegisterItemValue(currentField, cachedNomenclatures, censorSensitiveData, registerItemClone);
                                displayValue.Append(Environment.NewLine);
                                displayValue.Append(resolvedValue);
                                i++;
                                registerItemClone =
                                    registerItems.FirstOrDefault(ri => ri.Name == RepeatedFormFieldHelperService.InsertBeforeFirstUnderscore(currentField.Name, "#" + i));
                            }
                        }

                        jsonFields[currentField.Name] = displayValue.ToString();
                    }

                    if (!jsonFields.ContainsKey(nameof(registerItem.ProcessId)))
                    {
                        jsonFields.Add(nameof(registerItem.ProcessId), registerItem.ProcessId.ToString());
                    }

                    if (!jsonFields.ContainsKey(nameof(registerItem.ServiceTypeId)))
                    {
                        jsonFields.Add(nameof(registerItem.ServiceTypeId), registerItem.ServiceTypeId.ToString());
                    }

                    if (lookForRegistryNumber)
                    {
                        try
                        {
                            if (!jsonFields.ContainsKey(nameof(registerItem.RegisterNumber)))
                            {
                                jsonFields.Add(nameof(registerItem.RegisterNumber),
                                    registerItem.RegisterNumber);
                            }
                        }
                        catch (RuntimeBinderException)
                        {
                            lookForRegistryNumber = false;
                        }
                    }

                    if (lookForModifiedOn)
                    {
                        try
                        {
                            if (!jsonFields.ContainsKey(nameof(registerItem.RegisterDate)))
                            {
                                jsonFields.Add(nameof(registerItem.RegisterDate),
                                    registerItem.RegisterDate);
                            }
                        }
                        catch (RuntimeBinderException)
                        {
                            lookForModifiedOn = false;
                        }
                    }
                }

                result.Add(jsonFields);
            }

            return result;
        }

        private async Task<string> ResolveComplexField(Dictionary<string, Dictionary<string, string>> cachedNomenclatures, bool censorSensitiveData, FormField currentField,
            IGrouping<Guid, ProcessFieldValue> registerItems, int repetitionIndex = 0)
        {
            StringBuilder complexFieldValue = new StringBuilder();
            foreach (FormField subField in currentField.Fields)
            {
                var registerSubItem = repetitionIndex == 0 ?
                    registerItems.FirstOrDefault(ri => ri.Name == subField.Name) :
                    registerItems.FirstOrDefault(ri => ri.Name == RepeatedFormFieldHelperService.InsertBeforeFirstUnderscore(subField.Name, "#" + repetitionIndex));
                if (registerSubItem != null)
                {
                    var resolvedValue = await ResolveRegisterItemValue(subField, cachedNomenclatures,
                        censorSensitiveData, registerSubItem);

                    if (!string.IsNullOrWhiteSpace(resolvedValue))
                    {
                        complexFieldValue.Append(subField.Label + ": ");
                        complexFieldValue.Append(resolvedValue);
                        complexFieldValue.Append("; ");
                    }
                }
            }

            return complexFieldValue.ToString();
        }

        private async Task<string> ResolveRegisterItemValue(FormField formField, Dictionary<string, Dictionary<string, string>> cachedNomenclatures,
            bool censorSensitiveData, ProcessFieldValue registerItem)
        {
            //булева
            if (formField.Type == nameof(SimpleFormFieldType.Boolean))
            {
                return bool.Parse(registerItem.Value) ? "да" : "не";
            }
            //файл
            if (formField.Type == nameof(SimpleFormFieldType.File))
            {
                try
                {
                    string link = await _objectStoreService.GetPresignedUrl(registerItem.Value);
                    return link;
                }
                catch (Exception e)
                {
                    Logger.LogError(e, $"Грешка при създаване на линк в {nameof(GetTableDataForService)}");
                    return registerItem.Value;
                }
            }
            //номенклатура
            if (!string.IsNullOrWhiteSpace(formField.NomenclatureType) ||
                formField.Type == nameof(SimpleFormFieldType.City))
            {
                if (!string.IsNullOrWhiteSpace(registerItem.Value))
                {
                    Dictionary<string, string> nomenclature;
                    if (formField.Type == nameof(SimpleFormFieldType.City))
                    {
                        nomenclature = cachedNomenclatures[NomenclatureTypes.Ekatte];
                    }
                    else
                    {
                        nomenclature =
                            cachedNomenclatures[formField.NomenclatureType];
                    }

                    if (formField.Type == nameof(SimpleFormFieldType.MultiSelect))
                    {
                        List<string> resolvedMultiselectValues = new List<string>();
                        foreach (string multiselectValue in registerItem.Value.Split(','))
                        {
                            if (!nomenclature.TryGetValue(multiselectValue, out string resolvedValue))
                            {
                                Logger.LogError(
                                    $"Не е намерена стойност {multiselectValue} на номенклатура {formField.Type} в {nameof(ResolveRegisterItemValues)}");

                                resolvedMultiselectValues.Add(multiselectValue);
                            }
                            else
                            {
                                resolvedMultiselectValues.Add(resolvedValue);
                            }
                        }

                        return string.Join(", ", resolvedMultiselectValues);
                    }

                    if (!nomenclature.TryGetValue(registerItem.Value, out string value))
                    {
                        Logger.LogError(
                            $"Не е намерена стойност {registerItem.Value} на номенклатура {formField.Type} в {nameof(ResolveRegisterItemValues)}");
                        return registerItem.Value;
                    }

                    return value;
                }

                return string.Empty;
            }

            //Pid
            if (formField.Type == nameof(SimpleFormFieldType.PersonIdentifier))
            {
                if (string.IsNullOrWhiteSpace(registerItem.Value))
                {
                    return registerItem.Value;
                }

                var splitValue = registerItem.Value
                    .Split(':', StringSplitOptions.RemoveEmptyEntries);

                string pidResult = ((PidTypes)int.Parse((string)splitValue[0])).GetDescription() + ":" + Enumerable.Last<string>(splitValue);

                if (censorSensitiveData)
                {
                    pidResult = FormFieldsLayoutService.MaskAfterColonKeepingFirstTwo(pidResult);//#401315
                }

                return pidResult;
            }

            //Cid
            if (formField.Type == nameof(SimpleFormFieldType.CompanyIdentifier))
            {
                if (string.IsNullOrWhiteSpace(registerItem.Value))
                {
                    return registerItem.Value;
                }

                var splitValue = registerItem.Value
                    .Split(':', StringSplitOptions.RemoveEmptyEntries);

                return ((CidTypes)int.Parse((string)splitValue[0])).GetDescription() + ":" + Enumerable.Last<string>(splitValue);
            }

            //Bulgarian currency
            if (formField.Type == nameof(SimpleFormFieldType.BulgarianCurrency))
            {
                if (string.IsNullOrWhiteSpace(registerItem.Value))
                {
                    return registerItem.Value;
                }

                return BGCurrencyService.RegistryItemValueToPublicText(registerItem.Value);
            }

            //Number
            if (formField.Type == nameof(SimpleFormFieldType.Number))
            {
                if (string.IsNullOrWhiteSpace(registerItem.Value))
                {
                    return registerItem.Value;
                }

                return FormatInvariantToBg(registerItem.Value);
            }

            return registerItem.Value;
        }

        public static string FormatInvariantToBg(string invariantNumber)
        {
            if (string.IsNullOrWhiteSpace(invariantNumber))
            {
                return string.Empty;
            }

            // Step 1: Parse using InvariantCulture
            if (!decimal.TryParse(invariantNumber, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal value))
                throw new FormatException($"Cannot parse '{invariantNumber}' as a number.");

            // Step 2: Split into integer and fractional parts based on original string separators
            string integerPartStr;
            string fractionalPartStr = "";

            int dotPos = invariantNumber.IndexOf('.');
            int commaPos = invariantNumber.IndexOf(',');

            int decimalSeparatorPos = dotPos >= 0 ? dotPos : commaPos;

            if (decimalSeparatorPos >= 0)
            {
                integerPartStr = invariantNumber.Substring(0, decimalSeparatorPos);
                fractionalPartStr = invariantNumber.Substring(decimalSeparatorPos + 1);
            }
            else
            {
                integerPartStr = invariantNumber;
            }

            // Step 3: Format integer part with Bulgarian culture (with grouping)
            var bgCulture = new CultureInfo("bg-BG");
            string formattedIntegerPart = long.Parse(integerPartStr).ToString("N0", bgCulture);

            if (string.IsNullOrEmpty(fractionalPartStr))
            {
                return formattedIntegerPart;
            }

            // Step 4: Add thousand separators to fractional part in groups of 3 from left to right
            string formattedFractionalPart = AddThousandSeparatorsToFractionalPart(fractionalPartStr, bgCulture);

            // Step 5: Join with Bulgarian decimal separator (comma)
            return formattedIntegerPart + bgCulture.NumberFormat.NumberDecimalSeparator + formattedFractionalPart;
        }

        private static string AddThousandSeparatorsToFractionalPart(string fractionalPart, CultureInfo culture)
        {
            // Remove any leading/trailing spaces
            fractionalPart = fractionalPart.Trim();

            // Group digits in sets of 3 from the left
            List<string> groups = new List<string>();
            for (int i = 0; i < fractionalPart.Length; i += 3)
            {
                int length = Math.Min(3, fractionalPart.Length - i);
                groups.Add(fractionalPart.Substring(i, length));
            }

            // Join with the non-breaking space used in Bulgarian number formatting
            return string.Join(culture.NumberFormat.NumberGroupSeparator, groups);
        }

        private async Task ResolveFormFieldsValues(
            IEnumerable<FormField> fields,
            Dictionary<string, Dictionary<string, string>> cachedNomenclatures,
            bool censorSensitiveData)
        {
            foreach (var field in fields)
            {
                if (field.Fields.Any())
                {
                    await ResolveFormFieldsValues(field.Fields, cachedNomenclatures, censorSensitiveData);
                }

                if (field.Repetitions.Any())
                {
                    await ResolveFormFieldsValues(field.Repetitions, cachedNomenclatures, censorSensitiveData);
                }

                if (string.IsNullOrWhiteSpace(field.Value))
                {
                    continue;
                }

                //булева
                if (field.Type == SimpleFormFieldType.Boolean.ToString())
                {
                    field.Value = bool.Parse(field.Value) ? "да" : "не";
                    continue;
                }
                //файл
                if (field.Type == SimpleFormFieldType.File.ToString())
                {
                    try
                    {
                        string link = await _objectStoreService.GetPresignedUrl(field.Value);
                        field.Value = link;
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e, $"Грешка при създаване на линк в {nameof(ResolveFormFieldsValues)}");
                    }
                    continue;
                }
                //населено място
                if (field.Type == SimpleFormFieldType.City.ToString())
                {
                    if (!string.IsNullOrWhiteSpace(field.Value))
                    {
                        //SettlementFullInfoResponse settlementInfoResponse =
                        //    await _nomenclatureClient.GetSettlementFullInfoAsync(new SettlementFullInfoRequest(){EkatteCode =  field.Value});

                        //if (settlementInfoResponse.ResultStatus.Code != ResultCodes.Ok)
                        //{
                        //    Logger.LogError(
                        //        $"Не е намерена стойност {field.Value} на номенклатура {field.Type} в {nameof(ResolveFormFieldsValues)}. {settlementInfoResponse.ResultStatus.Message}");
                        //}
                        //else
                        //{
                        //    field.Value = settlementInfoResponse.SettlementFullInfo;
                        //}
                        Dictionary<string, string> nomenclature = cachedNomenclatures[NomenclatureTypes.Ekatte];

                        if (!nomenclature.TryGetValue(field.Value, out string value))
                        {
                            Logger.LogError(
                                $"Не е намерена стойност {field.Value} на номенклатура {field.Type} в {nameof(ResolveFormFieldsValues)}");
                        }
                        else
                        {
                            field.Value = value;
                        }
                    }

                    continue;
                }
                //номенклатура
                if (!string.IsNullOrWhiteSpace(field.NomenclatureType))
                {
                    if (!string.IsNullOrWhiteSpace(field.Value))
                    {
                        Dictionary<string, string> nomenclature;
                        if (field.Type == SimpleFormFieldType.City.ToString())
                        {
                            nomenclature = cachedNomenclatures[NomenclatureTypes.Ekatte];
                        }
                        else
                        {
                            nomenclature =
                                cachedNomenclatures[field.NomenclatureType];
                        }

                        if (field.Type == SimpleFormFieldType.MultiSelect.ToString())
                        {
                            List<string> resolvedMultiselectValues = new List<string>();
                            foreach (string multiselectValue in field.Value.Split(','))
                            {
                                if (!nomenclature.TryGetValue(multiselectValue, out string resolvedValue))
                                {
                                    Logger.LogError(
                                        $"Не е намерена стойност {multiselectValue} на номенклатура {field.Type} в {nameof(ResolveFormFieldsValues)}");

                                    resolvedMultiselectValues.Add(multiselectValue);
                                }
                                else
                                {
                                    resolvedMultiselectValues.Add(resolvedValue);
                                }

                                field.Value = string.Join(", ", resolvedMultiselectValues);
                            }
                        }
                        else if (!nomenclature.TryGetValue(field.Value, out string value))
                        {
                            Logger.LogError(
                                $"Не е намерена стойност {field.Value} на номенклатура {field.Type} в {nameof(ResolveFormFieldsValues)}");
                        }
                        else
                        {
                            field.Value = value;
                        }
                    }

                    continue;
                }
                //Pid
                if (field.Type == SimpleFormFieldType.PersonIdentifier.ToString())
                {
                    if (!string.IsNullOrWhiteSpace(field.Value))
                    {
                        var splitValue = field.Value
                            .Split(':', StringSplitOptions.RemoveEmptyEntries);

                        string pidValue = ((PidTypes)int.Parse((string)splitValue[0])).GetDescription() + ":" + Enumerable.Last<string>(splitValue);

                        pidValue = FormFieldsLayoutService.MaskAfterColonKeepingFirstTwo(pidValue);

                        field.Value = pidValue;
                    }

                    continue;
                }
                //Cid
                if (field.Type == SimpleFormFieldType.CompanyIdentifier.ToString())
                {
                    if (!string.IsNullOrWhiteSpace(field.Value))
                    {
                        var splitValue = field.Value
                            .Split(':', StringSplitOptions.RemoveEmptyEntries);

                        field.Value =
                            ((CidTypes)int.Parse((string)splitValue[0])).GetDescription() + ":" + Enumerable.Last<string>(splitValue);
                    }

                    continue;
                }
                //Bulgarian currency
                if (field.Type == SimpleFormFieldType.BulgarianCurrency.ToString())
                {
                    if (!string.IsNullOrWhiteSpace(field.Value))
                    {
                        field.Value = BGCurrencyService.RegistryItemValueToPublicText(field.Value);
                    }

                    continue;
                }
            }
        }

        internal static bool TryDetermineSearchPattern(string fieldName,
            string searchPattern,
            Dictionary<string, FormField> valuesOfInterest,
            Dictionary<string, Dictionary<string, string>> cachedNomenclatures,
            out List<string> result)
        {
            if (string.IsNullOrWhiteSpace(fieldName) || string.IsNullOrWhiteSpace(searchPattern))
            {
                result = new List<string>();
                return true;
            }

            if (fieldName.StartsWith("_"))
            {
                result = new List<string>() { searchPattern };
                return true;
            }

            if (!valuesOfInterest.TryGetValue(fieldName, out FormField field))
            {
                result = new List<string>();
                return false;
            }

            if (field.Type == SimpleFormFieldType.Boolean.ToString())
            {
                if (searchPattern.ToLower() == "да")
                {
                    result = new List<string> { true.ToString() };
                    return true;
                }
                if (searchPattern.ToLower() == "не")
                {
                    result = new List<string> { false.ToString() };
                    return true;
                }

                result = new List<string>();
                return false;
            }

            if (field.Type == nameof(SimpleFormFieldType.City))
            {
                result = cachedNomenclatures[NomenclatureTypes.Ekatte]
                    .Where(n =>
                        n.Value.Contains(searchPattern, StringComparison.InvariantCultureIgnoreCase))
                    .Select(v => v.Key).ToList();

                return result.Any();
            }

            if (field.Type is nameof(SimpleFormFieldType.Number) or nameof(SimpleFormFieldType.BulgarianCurrency))
            {
                result = new List<string> { searchPattern.Replace(" ", String.Empty).Replace(",", ".") };
                return true;
            }

            if (!string.IsNullOrWhiteSpace(field.NomenclatureType))
            {
                result = cachedNomenclatures[field.NomenclatureType]
                    .Where(n =>
                        n.Value.Contains(searchPattern, StringComparison.InvariantCultureIgnoreCase))
                    .Select(v => v.Key).ToList();

                return result.Any();
            }

            result = new List<string>() { $"%{searchPattern}%" };
            return true;
        }

        /// <summary>
        /// Връжа колоните за табличния вид на актуалната форма за услуга
        /// </summary>
        /// <param name="serviceId">Идентификатор на услугата. При 0 взима услугата за вписване</param>
        /// <param name="customViewsId">Идентификатор на справка</param>
        /// <returns></returns>
        public async Task<List<SelectListItem>> GetColumnsForTableView(int serviceId, int customViewsId = 0)
        {
            try
            {
                int? processFormParentId;

                if (serviceId == 0)
                {
                    processFormParentId = (await Repo.AllReadonly<Service>()
                        .TagWith(nameof(GetTableDataForService))
                        .SingleOrDefaultAsync(s => s.ServiceTypeId == (int)ServiceTypes.Register))?
                        .FormParentId;
                }
                else
                {
                    processFormParentId = (await Repo.AllReadonly<Service>()
                        .TagWith(nameof(GetTableDataForService))
                        .IgnoreQueryFilters()
                        .Where(p => p.Id == serviceId)
                        .SingleOrDefaultAsync())?.FormParentId;
                }

                if (processFormParentId == null)
                {
                    Logger.LogError(
                        $"Не е намерена форма в {nameof(GetTableDataForService)} за заявена услуга с идентификатор {serviceId}");
                    return null;
                }

                FormViewModel formModel = await GetFormViewModel(processFormParentId.Value);

                Dictionary<string, FormField> valuesOfInterest = new Dictionary<string, FormField>();

                if (customViewsId == 0)
                {
                    AddValuesOfInterestToDictionary(formModel.FormFields, valuesOfInterest, false);
                }
                else
                {
                    CustomViewViewModel customViewViewModel = await GetCustomViewViewModel(customViewsId);

                    AddValuesOfInterestToDictionary(formModel.FormFields, valuesOfInterest, customViewViewModel.SelectedColumns);

                    return valuesOfInterest
                        .Where(v => customViewViewModel.SelectedColumns.Contains(v.Key))
                        .Select(v =>
                            new SelectListItem(v.Value.Label, v.Key))
                        .ToList();
                }

                var result = valuesOfInterest
                    .Select(v =>
                        new SelectListItem(v.Value.Label, v.Key));

                return result.ToList();
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Грешка при {nameof(GetColumnsForTableView)} за услуга с идентификатор {serviceId}");
            }

            return new List<SelectListItem>();
        }

        public async Task<Dictionary<string, Dictionary<string, string>>> CacheNomenclaturesForValuesOfInterest(
            Dictionary<string, FormField> valuesOfInterest,
            bool detailedEkatte = true)
        {
            NomenclaturePublicRequest getNomenclaturesRequest = new NomenclaturePublicRequest
            {
                RegisterId = 0,
                SkipDateCheck = true
            };

            Dictionary<string, string> settlementDictionary = new Dictionary<string, string>();

            foreach (var field in valuesOfInterest)
            {
                if (field.Value.Fields.Any())
                {
                    foreach (FormField subField in field.Value.Fields)
                    {
                        await AddNomenclatureForCache(detailedEkatte, subField, getNomenclaturesRequest,
                            settlementDictionary);
                    }
                }
                else
                {
                    await AddNomenclatureForCache(detailedEkatte, field.Value, getNomenclaturesRequest,
                        settlementDictionary);
                }
            }

            var result = new Dictionary<string, Dictionary<string, string>>();

            if (getNomenclaturesRequest.NomenclatureTypes.Any())
            {
                NomenclaturePublicResponse response =
                    await _nomenclatureClient.GetNomenclaturePublicAsync(getNomenclaturesRequest);

                result = response.NomenclatureTypes.ToDictionary(
                    n => n.Type,
                    c => c.CodeableConcepts.ToDictionary(k => k.Code, v => v.Value));
            }

            if (settlementDictionary.Any())
            {
                result.Add(NomenclatureTypes.Ekatte, settlementDictionary);
            }

            return result;
        }

        private async Task AddNomenclatureForCache(bool detailedEkatte, FormField field,
            NomenclaturePublicRequest getNomenclaturesRequest, Dictionary<string, string> settlementDictionary)
        {
            if (!string.IsNullOrWhiteSpace(field.NomenclatureType) &&
                !getNomenclaturesRequest.NomenclatureTypes.Contains(field.NomenclatureType))
            {
                getNomenclaturesRequest.NomenclatureTypes.Add(field.NomenclatureType);
            }

            if (field.Type == SimpleFormFieldType.City.ToString() &&
                !getNomenclaturesRequest.NomenclatureTypes.Contains(NomenclatureTypes.Ekatte))
            {
                if (!detailedEkatte)
                {
                    getNomenclaturesRequest.NomenclatureTypes.Add(NomenclatureTypes.Ekatte);
                }
                else
                {
                    if (settlementDictionary.Any())
                    {
                        return;
                    }

                    EkattePublicResponse ekatteResponse = await _nomenclatureClient.GetEkattePublicAsync(new EkattePublicRequest() { RegisterId = 0 });
                    if (ekatteResponse.ResultStatus.Code != ResultCodes.Ok)
                    {
                        Logger.LogError($"Не може да зареди ЕКАТТЕ номенклатура в {nameof(CacheNomenclaturesForValuesOfInterest)} {ekatteResponse.ResultStatus.Message}");
                    }
                    else
                    {
                        foreach (EkatteItemPublic ekatteResponseCategory in ekatteResponse.Categories)
                        {
                            foreach (var city in ekatteResponseCategory.Cities)
                            {
                                settlementDictionary.Add(city.Code,
                                    ekatteResponseCategory.Category + " " + city.Name);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Връща колекция от имената на полета като ключ за дадена форма
        /// </summary>
        /// <param name="formId">Идентификатор на форма</param>
        /// <returns></returns>
        public async Task<Dictionary<string, string>> GetFormFieldNamesInFlatList(int formId)
        {
            try
            {
                FormViewModel formModel = await GetFormViewModelByFormId(formId);

                var result = new Dictionary<string, string>();

                foreach (FormField field in formModel.FormFields)
                {
                    if (field.Fields?.Any() != true && field.Type != SimpleFormFieldType.StaticText.ToString())
                    {
                        result.Add(field.Name, field.Label);
                    }
                    foreach (FormField subField in field.Fields
                                 .Where(f => f.Type != SimpleFormFieldType.StaticText.ToString()))
                    {
                        result.Add(subField.Name, $"{field.Label}/{subField.Label}");
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Грешка при {nameof(GetFormFieldNamesInFlatList)} за форма с идентификатор {formId}");
                return new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Връща колекция от имената на полета като ключ за дадена форма
        /// </summary>
        /// <param name="formParentId">Идентификатор на форма родител</param>
        /// <param name="includeComplexFields">Дали да включва сложните полета в списъка</param>
        /// <returns></returns>
        public async Task<Dictionary<string, string>> GetFormFieldNamesInFlatListByParentId(int formParentId, bool includeComplexFields = false)
        {
            try
            {
                FormViewModel formModel = await GetFormViewModel(formParentId);

                var result = new Dictionary<string, string>();

                foreach (FormField field in formModel.FormFields)
                {
                    if (field.Fields?.Any() != true || includeComplexFields)
                    {
                        result.Add(field.Name, field.Label);
                    }
                    if (!includeComplexFields || !field.CanBeRepeated)//Първото условие гарантира, че е за "Справки"
                    {
                        foreach (FormField subField in field.Fields)
                        {
                            result.Add(subField.Name, $"{field.Label}/{subField.Label}");
                        }
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Грешка при {nameof(GetFormFieldNamesInFlatListByParentId)} за форма с идентификатор на родител {formParentId}");
                return new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Очаква списък от имена на полета за форма и връща списък от тези които са неприемливи за нея
        /// </summary>
        /// <param name="formId">Идентификатор на форма</param>
        /// <param name="fieldNames">Списък от имена на полета</param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetUnacceptableFieldNamesForForm(int formId, IEnumerable<string> fieldNames)
        {
            try
            {
                List<string> unacceptableNames =
                    new List<string>();

                Dictionary<string, string> flatListWithFieldNames =
                    await GetFormFieldNamesInFlatList(formId);

                string repetitionPattern = @"#\d+";

                foreach (string name in fieldNames)
                {
                    string nameWithoutRepetitionIndex = Regex.Replace(name, repetitionPattern, string.Empty);

                    if (!flatListWithFieldNames.ContainsKey(nameWithoutRepetitionIndex))
                    {
                        unacceptableNames.Add(name);
                    }
                }

                return unacceptableNames;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Грешка при {nameof(GetUnacceptableFieldNamesForForm)} за форма с идентификатор {formId}");
                return new List<string>();
            }
        }

        /// <summary>
        /// Очаква списък от имена на полета за форма и връща списък от тези които са неприемливи за нея
        /// </summary>
        /// <param name="formParentId">Идентификатор на форма родител</param>
        /// <param name="fieldNames">Списък от имена на полета</param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetUnacceptableFieldNamesForFormByParentId(int formParentId,
            IEnumerable<string> fieldNames)
        {
            try
            {
                List<string> unacceptableNames =
                    new List<string>();

                Dictionary<string, string> flatListWithFieldNames =
                    await GetFormFieldNamesInFlatListByParentId(formParentId);

                string repetitionPattern = @"#\d+";

                foreach (string name in fieldNames)
                {
                    string nameWithoutRepetitionIndex = Regex.Replace(name, repetitionPattern, string.Empty);

                    if (!flatListWithFieldNames.ContainsKey(nameWithoutRepetitionIndex))
                    {
                        unacceptableNames.Add(name);
                    }
                }

                return unacceptableNames;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Грешка при {nameof(GetUnacceptableFieldNamesForFormByParentId)} за форма с идентификатор на родител {formParentId}");
                return new List<string>();
            }
        }

        /// <summary>
        /// Зареджда потребителските справки
        /// </summary>
        /// <returns></returns>
        public async Task<List<CustomViewViewModel>> GetCustomViews()
        {
            try
            {
                return await Repo.AllReadonly<CustomView>()
                    .TagWith(nameof(GetCustomViews))
                    .Select(v => new CustomViewViewModel { Id = v.Id, CustomViewTitle = v.Name })
                    .ToListAsync();
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Грешка при {nameof(GetCustomViews)}");
                throw;
            }
        }

        /// <summary>
        /// Добавяне или редакция на потребителска справка
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<SaveOperationResult> UpsertCustomView(CustomViewViewModel model)
        {
            if (model.Id == 0)
            {
                try
                {
                    CustomView newCustomView = new CustomView()
                    {
                        ModifiedByUserId = _userContext.UserId,
                        //SelectedColumns = JsonSerializer.Serialize(model.SelectedColumns),
                        SelectedColumns = model.SelectedColumns,
                        ModifiedOn = DateTime.UtcNow,
                        Name = model.CustomViewTitle!
                    };

                    await Repo.AddAsync(newCustomView);
                    await Repo.SaveChangesAsync();

                    return new SaveOperationResult(true, newCustomView.Id);
                }
                catch (Exception e)
                {
                    Logger.LogError(e,
                        $"Проблем при запис на потребителска справка с идентификатор {model.Id} в {nameof(UpsertCustomView)}");
                    return new SaveOperationResult("Проблем при запис на данни");
                }
            }

            try
            {
                CustomView savedView = await Repo.GetByIdAsync<CustomView>(model.Id);

                if (savedView == null)
                {
                    Logger.LogError($"Не е намерен {nameof(CustomView)} с id {model.Id} в {nameof(UpsertCustomView)}");
                    return new SaveOperationResult($"Не е намерена справка с id {model.Id}");
                }

                savedView.Name = model.CustomViewTitle;
                savedView.SelectedColumns = model.SelectedColumns;
                await Repo.SaveChangesAsync();
                return new SaveOperationResult(true, model.Id);
            }
            catch (Exception e)
            {
                Logger.LogError(e,
                    $"Проблем при промяна на потребителска справка с идентификатор {model.Id} в {nameof(UpsertCustomView)}");
                return new SaveOperationResult("Проблем при запис на данни");
            }
        }

        /// <summary>
        /// Извлича списък с потенциалните колони за потребителска справка
        /// </summary>
        /// <returns></returns>
        public async Task<Dictionary<string, string>> CustomViewColumns()
        {
            try
            {
                int? processFormParentId = await GetFormParentIdOfTheRegisterService();

                if (processFormParentId.HasValue)
                {
                    return await GetFormFieldNamesInFlatListByParentId(processFormParentId.Value, true);
                }
                else
                {
                    Logger.LogError($"Не е намерена форма за услуга вписване в {nameof(CustomViewColumns)}");
                    return new Dictionary<string, string>();
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Проблем в {nameof(CustomViewColumns)}");
                return new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Извлича модел на потребителска справка
        /// </summary>
        /// <param name="id">Идентификатор на потребитлеска справка</param>
        /// <returns></returns>
        public async Task<CustomViewViewModel> GetCustomViewViewModel(int id)
        {
            try
            {
                CustomView view = await Repo.GetByIdAsync<CustomView>(id);

                if (view == null)
                {
                    Logger.LogError($"Не е намерен {nameof(CustomView)} с id {id} в {nameof(GetCustomViewViewModel)}");
                    return null;
                }

                return new CustomViewViewModel
                {
                    CustomViewTitle = view.Name,
                    SelectedColumns = view.SelectedColumns
                };
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Проблем при извличане на {nameof(CustomView)} с id {id} в {nameof(GetCustomViewViewModel)}");
                return null;
            }
        }

        /// <summary>
        /// Изтриване на потребителска справка по идентификатор
        /// </summary>
        /// <param name="id">Идентификатор на потребителска справка</param>
        /// <returns></returns>
        public async Task<OperationResult> DeleteCustomView(int id)
        {
            try
            {
                CustomView view = await Repo.GetByIdAsync<CustomView>(id);

                if (view == null)
                {
                    Logger.LogError($"Не е намерен {nameof(CustomView)} с id {id} в {nameof(DeleteCustomView)}");
                    return null;
                }

                view.ModifiedOn = DateTime.UtcNow;
                view.ModifiedByUserId = _userContext.UserId;

                Repo.Delete(view);
                await Repo.SaveChangesAsync();
                return new OperationResult();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Проблем при изтриване на потребителска справка с id {id}");
                return new OperationResult("Проблем при изтриване на потребителската справка");
            }
        }

        /// <summary>
        /// Извличане на условия към форма
        /// </summary>
        /// <param name="formParentId">Идентификатор на родителска форма</param>
        /// <returns></returns>
        public async Task<List<FormCondition>> GetFormConditions(int formParentId)
        {
            try
            {
                return await Repo.AllReadonly<FormCondition>()
                    .TagWith(nameof(GetFormConditions))
                    .Where(f => f.FormParentId == formParentId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Проблем при извличане на условия към форма с formParentId {formParentId}");
                return null;
            }
        }

        /// <summary>
        /// Връща модел на условие към форма по родителски идентификатор на форма
        /// </summary>
        /// <param name="formConditionId"></param>
        /// <returns></returns>
        public async Task<AddConditionViewModel> GetFormConditionViewModel(int formConditionId)
        {
            try
            {
                FormCondition condition = await Repo.GetByIdAsync<FormCondition>(formConditionId);

                if (condition == null)
                {
                    Logger.LogError($"Не е намерен {nameof(FormCondition)} с id {formConditionId} в {nameof(GetFormViewModel)}");
                    return null;
                }

                AddConditionViewModel result = new AddConditionViewModel()
                {
                    Id = condition.Id,
                    FormParentId = condition.FormParentId!.Value,
                    FieldsToHide = condition.FieldsToHide.Split(';').ToList(),
                    TriggeringFieldName = condition.TriggeringFieldName,
                    TriggeringNomenclatureValue = condition.TriggeringNomenclatureValue,
                };

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Проблем при изтриване на потребителска справка с id {formConditionId}");
                return null;
            }
        }

        /// <summary>
        /// Записва условие за форма
        /// </summary>
        /// <param name="model">Модел на условие за форма</param>
        /// <returns></returns>
        public async Task<SaveOperationResult> SaveFormCondition(AddConditionViewModel model)
        {
            try
            {

                if (model.Id == 0)
                {
                    FormCondition conditionToSave = new FormCondition()
                    {
                        FormParentId = model.FormParentId,
                        TriggeringFieldName = model.TriggeringFieldName,
                        TriggeringNomenclatureValue = model.TriggeringNomenclatureValue,
                        FieldsToHide = string.Join(';', model.FieldsToHide),
                        ModifiedByUserId = _userContext.UserId,
                        ModifiedOn = DateTime.UtcNow
                    };

                    await Repo.AddAsync(conditionToSave);
                    await Repo.SaveChangesAsync();
                    return new SaveOperationResult(true, conditionToSave.Id);
                }
                else
                {
                    FormCondition savedCondition = await Repo.GetByIdAsync<FormCondition>(model.Id);

                    if (savedCondition == null)
                    {
                        return new SaveOperationResult("Не е намерено условие с идентификатор " + model.Id);
                    }

                    savedCondition.TriggeringFieldName = model.TriggeringFieldName;
                    savedCondition.TriggeringNomenclatureValue = model.TriggeringNomenclatureValue;
                    savedCondition.FieldsToHide = string.Join(';', model.FieldsToHide);
                    savedCondition.ModifiedByUserId = _userContext.UserId;
                    savedCondition.ModifiedOn = DateTime.UtcNow;

                    await Repo.SaveChangesAsync();
                    return new SaveOperationResult(true, savedCondition.Id);
                }
            }
            catch (Exception e)
            {
                return new SaveOperationResult(e.Message);
            }
        }

        /// <summary>
        /// Записва условие за форма
        /// </summary>
        /// <param name="formParentId">Модел на условие за форма</param>
        /// <returns></returns>
        public async Task<Dictionary<string, FieldConditions>> GetConditionTreeForFormParentId(int formParentId)
        {
            List<FormCondition> formConditions = await Repo.AllReadonly<FormCondition>()
                .TagWith(nameof(GetConditionTreeForFormParentId))
                .Where(c => c.FormParentId == formParentId)
                .ToListAsync();

            Dictionary<string, FieldConditions> tree = formConditions
                .GroupBy(fc => fc.TriggeringFieldName)
                .ToDictionary(
                    group => group.Key,
                    group => new FieldConditions
                    {
                        FieldsToShow = new List<string>(),
                        Conditions = group
                            .GroupBy(fc => fc.TriggeringNomenclatureValue)
                            .ToDictionary(
                                innerGroup => innerGroup.Key,
                                innerGroup => new ConditionDetails
                                {
                                    FieldsToHide = innerGroup
                                        .Select(fc => fc.FieldsToHide.Split(';', StringSplitOptions.RemoveEmptyEntries)
                                            .Select(field => field.Trim())
                                            .ToList())
                                        .FirstOrDefault() ?? new List<string>()
                                }
                            )
                    }
                );

            //Полетата за показване са комбинацция от всички полета, които някое услови крие
            foreach (var triggeringFieldPair in tree)
            {
                var fieldsToShow = triggeringFieldPair.Value.Conditions
                    .SelectMany(v => v.Value.FieldsToHide).Distinct();

                triggeringFieldPair.Value.FieldsToShow = fieldsToShow.ToList();
            }

            return tree;
        }

        /// <summary>
        /// Прилава условията върху модел на форма
        /// </summary>
        /// <param name="model">Модела на формата</param>
        /// <returns></returns>
        public async Task ApplyConditionTreeOnFormModel(FormViewModel model)
        {
            var conditionTree = await GetConditionTreeForFormParentId(model.FormParentId);

            foreach (var pair in conditionTree)
            {
                var triggeringField = model.FormFields.SingleOrDefault(f => f.Name == pair.Key);
                if (triggeringField != null)
                {
                    foreach (var condition in pair.Value.Conditions)
                    {
                        if (condition.Key == triggeringField.Value)
                        {
                            foreach (var fieldToHideName in condition.Value.FieldsToHide)
                            {
                                var fieldToHide = model.FormFields.SingleOrDefault(f => f.Name == fieldToHideName);
                                if (fieldToHide != null)
                                {
                                    fieldToHide.Value = null;
                                    fieldToHide.IsRequired = false;
                                    foreach (var subField in fieldToHide.Fields)
                                    {
                                        subField.Value = null;
                                    }
                                    foreach (var clonedField in fieldToHide.Repetitions)
                                    {
                                        clonedField.Value = null;
                                        foreach (var subField in clonedField.Fields)
                                        {
                                            subField.Value = null;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Извличане на условие по идентификатор
        /// </summary>
        /// <param name="id">Идентификатор на условие</param>
        /// <returns></returns>
        public async Task<FormCondition> GetFormConditionById(int id)
        {
            try
            {
                FormCondition formCondition = await Repo.All<FormCondition>()
                .TagWith(nameof(GetFormConditionById))
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(fc => fc.Id == id);

                return formCondition;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Проблем при извличане на условие с идентификатор: {id} в {nameof(GetFormConditionById)}");
                return null;
            }
        }

        /// <summary>
        /// Изтриване на условие по идентификатор
        /// </summary>
        /// <param name="id">Идентификатор на условие</param>
        /// <returns></returns>
        public async Task<OperationResult> DeleteFormCondition(int id)
        {
            if (id <= 0)
            {
                return new OperationResult($"Невалиден идентификатор на условие: {id}");
            }

            try
            {
                FormCondition formCondition = await Repo.All<FormCondition>()
                    .TagWith(nameof(DeleteFormCondition))
                    .IgnoreQueryFilters()
                    .SingleOrDefaultAsync(fc => fc.Id == id);

                if (formCondition == null)
                {
                    return new OperationResult($"Условие с идентификатор {id} не е открито");
                }

                await Repo.DeleteAsync<FormCondition>(id);
                await Repo.SaveChangesAsync();
                return new OperationResult();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Грешка при изтриване на условие с id {id} в {nameof(DeleteFormCondition)}");
                return new OperationResult("Възникна грешка при изтриване на условието");
            }
        }

        public class ConditionDetails
        {
            public List<string> FieldsToHide { get; set; } = new List<string>();
        }

        public class FieldConditions
        {
            public List<string> FieldsToShow { get; set; } = new List<string>();
            public Dictionary<string, ConditionDetails> Conditions { get; set; } = new Dictionary<string, ConditionDetails>();
        }

        public class ProcessFieldValue
        {
            public string Value { get; set; }
            public string Name { get; set; }
            public Guid ProcessId { get; set; }
            public DateTime IncomingDate { get; set; }
            public string RegisterNumber { get; set; }
            public DateTime? RegisterDate { get; set; }
            public int ServiceTypeId { get; set; }
        }
    }
}
