using Microsoft.EntityFrameworkCore;
using URegister.Common;
using URegister.Infrastructure.Extensions;
using URegister.Infrastructure.Model.RegisterForms;
using URegister.ObjectsCatalog.Contracts;
using URegister.ObjectsCatalog.Data;
using URegister.ObjectsCatalog.Data.Models;

namespace URegister.ObjectsCatalog.Services
{
    /// <summary>
    /// Услуга за обекти от каталога на обектите
    /// </summary>
    /// <param name="objectCatalogRepository">Репозитори за достъп до данните</param>
    public class ObjectService(IObjectCatalogRepository objectCatalogRepository,
        ILogger<ObjectService> logger) : IObjectService
    {
        /// <summary>
        /// Получаване на данни за поле
        /// </summary>
        /// <param name="type">Тип на полето</param>
        /// <returns></returns>
        public async Task<string> GetFieldDataAsync(string type)
        {
            var data = await objectCatalogRepository.AllReadonly<Field>()
                .TagWith(nameof(GetFieldDataAsync))
                .Where(f => f.FieldType.Name == type)
                .Where(f => f.IsCurrent)
                .Select(f => new { f.FieldData, f.Id })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                logger.LogInformation($"Не е намерено поле от тип {type} в {nameof(GetFieldDataAsync)}");
                return string.Empty.ToJson();
                //throw new ArgumentException("Не е намерено поле от този тип");
            }

            FormField? field = data.FieldData.FromJson<FormField>();

            if (field == null)
            {
                throw new ArgumentException("Грешка при зареждане на данните за полето");
            }

            field.FieldId = data.Id;

            return field.ToJson();
        }

        /// <summary>
        /// Вземане на списък на полетата
        /// </summary>
        /// <returns></returns>
        public async Task<ICollection<(string type, string label, bool isComplex, string template)>> GetFieldTypesAsync()
        {
            var types = await objectCatalogRepository.AllReadonly<FieldType>()
                .TagWith(nameof(GetFieldTypesAsync))
                .Select(t => new { t.Name, t.Label, t.IsComplexField, t.Template })
                .ToListAsync();

            return types.Select(t => (t.Name, t.Label, t.IsComplexField, t.Template)).ToList();
        }

        /// <summary>
        /// Запис на данни за поле
        /// </summary>
        /// <param name="data">Поле за запис</param>
        /// <returns></returns>
        public async Task<int> SetFieldDataAsync(FormField data)
        {
            FieldType? fieldType = await GetFieldTypeByName(data.Type);

            Field? currentVersion = null;

            if (fieldType == null)
            {
                logger.LogError($"Тип поле {data.Type} не е познат. Грешка в {nameof(SetFieldDataAsync)}");
                return 0;
            }

            currentVersion = await objectCatalogRepository.All<Field>()
                .TagWith(nameof(SetFieldDataAsync))
                .Where(f => f.FieldTypeId == fieldType.Id)
                .Where(f => f.IsCurrent)
                .FirstOrDefaultAsync();

            int version = 1;

            if (currentVersion != null)
            {
                version = currentVersion.Version + 1;
                currentVersion.IsCurrent = false;
            }

            Field field = new Field
            {
                Name = data.Name,
                FieldType = fieldType,
                FieldData = data.ToJson(),
                Version = version,
                IsCurrent = true
            };

            await objectCatalogRepository.AddAsync(field);
            await objectCatalogRepository.SaveChangesAsync();

            return version;
        }

        /// <summary>
        /// Запис на нов тип поле
        /// </summary>
        /// <param name="newType"></param>
        /// <returns>Успешен ли е записът</returns>
        public async Task<bool> SetFieldTypeAsync(CatalogFieldType newType)
        {
            try
            {
                await objectCatalogRepository.AddAsync(new FieldType
                {
                    IsComplexField = newType.IsComplex,
                    Label = newType.Label,
                    Name = newType.Type,
                    Template = newType.TemplateName
                });

                int savedEntries = await objectCatalogRepository.SaveChangesAsync();
                return savedEntries > 0;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка на запис на тип поле {newType.Type} в {nameof(SetFieldTypeAsync)}");
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private async Task<FieldType?> GetFieldTypeByName(string typeName)
        {
            FieldType? result = null;

            if (!string.IsNullOrWhiteSpace(typeName))
            {
                result = await objectCatalogRepository.All<FieldType>()
                    .TagWith(nameof(GetFieldTypeByName))
                    .FirstOrDefaultAsync(t => t.Name == typeName);
            }

            if (result == null)
            {
                logger.LogError($"Грешка при извличане на FieldType в {nameof(GetFieldTypeByName)} за име на тип {typeName}");
            }

            return result;
        }
        /// <summary>
        /// Списък типове услуги
        /// </summary>
        /// <param name="request">Заявка с инфромация</param>
        /// <returns></returns>
        public async Task<(List<ServiceTypeMessage>, int)> GetServiceTypes(DatatableRequest request)
        {
            var query = objectCatalogRepository.AllReadonly<ServiceType>().TagWith(nameof(GetServiceTypes));
            var countAll = 0;
            (query, countAll) = await request.GetFilteredData(query);
            var data = (await query.ToListAsync())
                                  .Select(x => new ServiceTypeMessage
                                  {
                                      Name = x.Name,
                                      Id = x.Id,
                                  })
                                  .ToList();

            return (data, countAll);
        }

        /// <summary>
        /// Списък стъпки
        /// </summary>
        /// <param name="request">Заявка с инфромация</param>
        /// <returns></returns>
        public async Task<(List<StepMessage>, int)> GetStepList(DatatableRequest request)
        {
            var query = objectCatalogRepository.AllReadonly<Step>().TagWith(nameof(GetStepList));
            var list = new List<StepMessage>();
            var countAll = 0;
            (query, countAll) = await request.GetFilteredData(query);
            var data = (await query.ToListAsync())
                                  .Select(x => new StepMessage
                                  {
                                      Id = x.Id,
                                      Name = x.Name,
                                      Type = x.Type,
                                      Method = x.Method,
                                      IsForOfficialUse = x.IsForOfficialUse,
                                      IsForPublicUse = x.IsForPublicUse,
                                  })
                                  .ToList();

            return (data, countAll);
        }

        /// <summary>
        /// Запис на стъпка към услуга
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task AppendUpdateStep(StepMessage request)
        {
            var step = new Step();
            if (request.Id > 0)
            {
                step = await objectCatalogRepository.GetByIdAsync<Step>(request.Id);
            }
            else
            {
                await objectCatalogRepository.AddAsync(step);
            }
            step.Name = request.Name;
            step.Type = request.Type;
            step.Method = request.Method;
            await objectCatalogRepository.SaveChangesAsync();
        }

        /// <summary>
        /// Четене на стъпка към услуга по ид
        /// </summary>
        /// <param name="id">Идентификатор на стъпката</param>
        /// <returns></returns>
        public async Task<StepMessage> GetStep(int id)
        {
            var step = await objectCatalogRepository.GetByIdAsync<Step>(id)!;
            return new StepMessage
            {
                Id = step.Id,
                Name = step.Name,
                Type = step.Type,
                Method = step.Method
            };
        }

        /// <summary>
        /// Четене на тип услуга
        /// </summary>
        /// <param name="id">Идентификатор на услугата</param>
        /// <returns></returns>
        public async Task<GetServiceTypeMessage> GetServiceType(int id)
        {
            var serviceType = await objectCatalogRepository.AllReadonly<ServiceType>()
                .TagWith(nameof(GetServiceType))
                                                           .Include(x => x.ServiceTypeSteps)
                                                           .Where(x => x.Id == id)
                                                           .FirstAsync();
            var reply = new GetServiceTypeMessage
            {
                Id = serviceType.Id,
                Name = serviceType.Name,
            };
            var steps = await objectCatalogRepository.AllReadonly<Step>()
                .TagWith(nameof(GetServiceType))
                                                     .Select(x => new CheckListItem
                                                     {
                                                         Id = x.Id,
                                                         Label = x.Name,
                                                     })
                                                     .ToListAsync();
            steps.ForEach(x =>
            {
                x.Value = serviceType.ServiceTypeSteps.Any(s => s.StepId == x.Id);
            });
            reply.Steps.AddRange(steps);
            return reply;
        }

        /// <summary>
        /// Запис на тип услуга
        /// </summary>
        /// <param name="request">Заявка с инфромация</param>
        /// <returns></returns>
        public async Task AppendUpdate(ServiceTypeMessage request)
        {
            var serviceType = new ServiceType();
            if (request.Id > 0)
            {
                serviceType = await objectCatalogRepository.All<ServiceType>()
                    .TagWith(nameof(AppendUpdate))
                                   .Include(x => x.ServiceTypeSteps)
                                   .Where(x => x.Id == request.Id)
                                   .FirstAsync();
            }
            else
            {
                await objectCatalogRepository.AddAsync(serviceType);
            }
            serviceType.Name = request.Name;
            serviceType.ServiceTypeSteps.Clear();
            serviceType.ServiceTypeSteps.AddRange(
                request.StepIds.Select(x => new ServiceTypeStep
                {
                    ServiceTypeId = serviceType.Id,
                    StepId = x
                })
                .ToList());
            await objectCatalogRepository.SaveChangesAsync();
        }
    }
}
