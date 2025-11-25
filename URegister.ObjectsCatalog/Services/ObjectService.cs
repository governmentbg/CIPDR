using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Nodes;
using URegister.Common;
using URegister.Infrastructure.Extensions;
using URegister.Infrastructure.Helper;
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

            return field.ToJson();
        }

        /// <summary>
        /// Вземане на списък на полетата
        /// </summary>
        /// <param name="requestRegisterCode"></param>
        /// <returns></returns>
        public async
            Task<ICollection<(string type, string label, bool isComplex, string template, int fieldTypeId, List<string>?
                registerRestrictionCodes)>> GetFieldTypesAsync(string requestRegisterCode)
        {
            var jsonValue = JsonSerializer.Serialize(new[] { requestRegisterCode });  // Serializes to ["value"] as JSON array snippet

            var types = await objectCatalogRepository.AllReadonly<FieldType>()
                .Where(f => string.IsNullOrWhiteSpace(requestRegisterCode) || 
                            f.RegisterRestrictionCodes == null || 
                            !f.RegisterRestrictionCodes.Any() ||
                            EF.Functions.JsonContains(f.RegisterRestrictionCodes, jsonValue))
                .TagWith(nameof(GetFieldTypesAsync))
                .Select(t => new { t.Name, t.Label, t.IsComplexField, t.Template, t.Id, t.RegisterRestrictionCodes })
                .ToListAsync();

            return types.Select(t => (t.Name, t.Label, t.IsComplexField, t.Template, t.Id, t.RegisterRestrictionCodes)).ToList();
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
        /// Запис или редактиране на тип поле
        /// </summary>
        /// <param name="newType">Тип поле за запис или редактиране</param>
        /// <returns>Успешен ли е записът или редакцията</returns>
        public async Task<bool> SetFieldTypeAsync(CatalogFieldType newType)
        {
            FieldType? existingFieldType = null;
            try
            {
                existingFieldType = await GetFieldTypeByName(newType.Type);                
                if (existingFieldType != null)
                {                   
                    existingFieldType.Label = newType.Label;
                    //existingFieldType.Name = newType.Type;                   
                    existingFieldType.RegisterRestrictionCodes = newType.RegisterRestrictionCodes?.ToList();                 
                }
                else
                {
                    await objectCatalogRepository.AddAsync(new FieldType
                    {
                        IsComplexField = newType.IsComplex,
                        Label = newType.Label,
                        Name = newType.Type,
                        Template = newType.TemplateName,
                        RegisterRestrictionCodes = newType.RegisterRestrictionCodes?.ToList()
                    });
                }
                
                int savedEntries = await objectCatalogRepository.SaveChangesAsync();
                return savedEntries > 0;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка при {(existingFieldType != null ? "редактиране" : "запис")} на тип поле {newType.Type} в {nameof(SetFieldTypeAsync)}");
                return false;
            }
        }

        /// <summary>
        /// Връща тип поле по име на тип
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<FieldType?> GetFieldTypeByName(string typeName)
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
                                      RoleId = x.RoleId?.ToString(),
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
            step.RoleId = request.RoleId == null ? null : Guid.Parse(request.RoleId);
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
                RoleId = step.RoleId?.ToString(),
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

        /// <summary>
        /// Изтриване на тип услуга
        /// </summary>
        /// <param name="serviceTypeId">Идентификатор на тип услуга</param>
        /// <returns></returns>
        public async Task<ResultStatus> DeleteServiceType(int serviceTypeId)
        {
            try
            {

                ServiceType serviceTypeToDelete = await objectCatalogRepository.All<ServiceType>()
                    .TagWith(nameof(DeleteServiceType))
                    .Where(s => s.Id == serviceTypeId)
                    .Include(s => s.ServiceTypeSteps)
                    .SingleOrDefaultAsync();

                if (serviceTypeToDelete == null)
                {
                    return new ResultStatus
                    {
                        Code = ResultCodes.NotFound,
                        Message = $"Тип услуга с идентификатор {serviceTypeId} не е намерена"
                    };
                }

                objectCatalogRepository.DeleteRange(serviceTypeToDelete.ServiceTypeSteps);
                objectCatalogRepository.Delete(serviceTypeToDelete);
                await objectCatalogRepository.SaveChangesAsync();
                return new ResultStatus
                {
                    Code = ResultCodes.Ok,
                };
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Проблем при триене на тип услуга с идентификатор {serviceTypeId} в {nameof(DeleteServiceType)}");
                return new ResultStatus
                {
                    Code = ResultCodes.InternalServerError,
                    Message = $"Проблем при триене на тип услуга с идентификатор {serviceTypeId}"
                };
            }
        }

        /// <summary>
        /// Изтриване на тип поле
        /// </summary>
        /// <param name="fieldTypeId">Идентификатор на тип поле</param>
        /// <returns></returns>
        public async Task<ResultStatus> DeleteFieldType(int fieldTypeId)
        {
            try
            {

                FieldType fieldTypeToDelete = await objectCatalogRepository.All<FieldType>()
                          .TagWith(nameof(DeleteFieldType))
                          .Where(ft => ft.Id == fieldTypeId)
                          .Include(ft => ft.Fields)
                          .SingleOrDefaultAsync();

                if (fieldTypeToDelete == null)
                {
                    logger.LogError($"Тип поле с идентификатор {fieldTypeId} не е намерено");

                    return new ResultStatus
                    {
                        Code = ResultCodes.NotFound,
                        Message = $"Тип поле с идентификатор {fieldTypeId} не е намерено"
                    };
                }

                objectCatalogRepository.DeleteRange(fieldTypeToDelete.Fields);
                objectCatalogRepository.Delete(fieldTypeToDelete);
                await objectCatalogRepository.SaveChangesAsync();
                return new ResultStatus
                {
                    Code = ResultCodes.Ok,
                };
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Проблем при триене на тип поле с идентификатор {fieldTypeId} в {nameof(DeleteFieldType)}");
                return new ResultStatus
                {
                    Code = ResultCodes.InternalServerError,
                    Message = $"Проблем при триене на тип поле с идентификатор {fieldTypeId}"
                };
            }
        }

        public async Task<ServiceTypeNameExistsReply> CheckServiceNameExists(string name)
        {
            try
            {
                return new ServiceTypeNameExistsReply
                {
                    Status = CommonGrpcHelper.CreateStatusOK(),
                    IsExists = objectCatalogRepository.AllReadonly<ServiceType>()
                    .TagWith(nameof(CheckServiceNameExists))
                    .Any(s => EF.Functions.ILike(s.Name, name))
                };
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Проблем при проверка за съществуващо име на тип услуга в {nameof(CheckServiceNameExists)}");
                return new ServiceTypeNameExistsReply
                {
                    Status = new ResultStatus
                    {
                        Code = ResultCodes.InternalServerError,
                        Message = $"Проблем при проверка за съществуващо име на тип услуга"
                    }
                };
            }
        }

        /// <summary>
        /// Изтриване на стъпка
        /// </summary>
        /// <param name="stepId">Идентификатор на стъпка</param>
        /// <returns></returns>
        public async Task<ResultStatus> DeleteStep(int stepId)
        {
            try
            {

                Step stepToDelete = await objectCatalogRepository.All<Step>()
                    .TagWith(nameof(DeleteStep))
                    .Where(s => s.Id == stepId)
                    .Include(s => s.ServiceTypeSteps)
                    .SingleOrDefaultAsync();

                if (stepToDelete == null)
                {
                    return new ResultStatus
                    {
                        Code = ResultCodes.NotFound,
                        Message = $"Стъпка с идентификатор {stepId} не е намерена"
                    };
                }

                if (stepToDelete.ServiceTypeSteps.Any())
                {
                    return new ResultStatus
                    {
                        Code = ResultCodes.BadRequest,
                        Message = "Премахнете стъпката от съществуващите типове услуги, в които е включена, за да може да бъде изтрита."
                    };
                }

                objectCatalogRepository.Delete(stepToDelete);
                await objectCatalogRepository.SaveChangesAsync();
                return new ResultStatus
                {
                    Code = ResultCodes.Ok,
                };
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Проблем при триене на стъпка с идентификатор {stepId} в {nameof(DeleteStep)}");
                return new ResultStatus
                {
                    Code = ResultCodes.InternalServerError,
                    Message = $"Проблем при триене на стъпка с идентификатор {stepId}"
                };
            }
        }
        /// <summary>
        /// Списък бланки
        /// </summary>
        /// <param name="request">Заявка с инфромация</param>
        /// <returns></returns>
        public async Task<(List<FieldTemplateMessage>, int)> GetFieldTemplateList(DatatableRequest request)
        {
            var query = objectCatalogRepository.AllReadonly<FieldTemplate>()
                                               .TagWith(nameof(GetFieldTemplateList));
            var countAll = 0;
            (query, countAll) = await request.GetFilteredData(query);
            var data = await query.Select(x => new FieldTemplateMessage
            {
                Id = x.Id,
                BlankIfNoValue = x.BlankIfNoValue,
                Name = x.Name,
                FieldTypeId = x.FieldTypeId,
                FieldTypeName = x.FieldType.Label,
                FieldType = x.FieldType.Name
            })
                                  .ToListAsync();
            return (data, countAll);
        }
        /// <summary>
        /// Списък бланки
        /// </summary>
        /// <param name="request">Заявка с инфромация</param>
        /// <returns></returns>
        public async Task<List<FieldTemplateContentMessage>> GetFieldTemplateContentList()
        {
            var query = objectCatalogRepository.AllReadonly<FieldTemplate>()
                                               .TagWith(nameof(GetFieldTemplateList));
            var countAll = 0;
            var data = await query.Select(x => new FieldTemplateContentMessage
            {
                Id = x.Id,
                BlankIfNoValue = x.BlankIfNoValue,
                Name = x.Name,
                FieldTypeId = x.FieldTypeId,
                FieldTypeName = x.FieldType.Label,
                FieldType = x.FieldType.Name,
                Content = x.Content,
                ContentText = x.ContentText,
            })
            .ToListAsync();
            return data;
        }

        /// <summary>
        /// Данни за бланка
        /// </summary>
        /// <param name="request">Заявка с инфромация</param>
        /// <returns></returns>
        public async Task<FieldTemplateResponse> GetFieldTemplate(int id)
        {
            return await objectCatalogRepository.AllReadonly<FieldTemplate>()
                                           .Where(x => x.Id == id)
                                           .TagWith(nameof(GetFieldTemplate))
                                           .Select(x => new FieldTemplateResponse
                                           {
                                               FieldTemplate = new FieldTemplateMessage
                                               {
                                                   Id = x.Id,
                                                   BlankIfNoValue = x.BlankIfNoValue,
                                                   Name = x.Name,
                                                   FieldTypeId = x.FieldTypeId,
                                                   FieldType = x.FieldType.Name,
                                                   FieldTypeName = x.FieldType.Label,
                                               },
                                           })
                                           .FirstAsync();
        }
        /// <summary>
        /// Данни за бланка
        /// </summary>
        /// <param name="request">Заявка с инфромация</param>
        /// <returns></returns>
        public async Task<FieldTemplateContentResponse> GetFieldTemplateContent(int id)
        {
            return await objectCatalogRepository.AllReadonly<FieldTemplate>()
                                           .Where(x => x.Id == id)
                                           .TagWith(nameof(GetFieldTemplateContent))
                                           .Select(x => new FieldTemplateContentResponse
                                           {
                                               FieldTemplate = new FieldTemplateContentMessage
                                               {
                                                   Id = x.Id,
                                                   BlankIfNoValue = x.BlankIfNoValue,
                                                   Name = x.Name,
                                                   FieldTypeId = x.FieldTypeId,
                                                   FieldType = x.FieldType.Name,
                                                   FieldTypeName = x.FieldType.Label,
                                                   Content = x.Content,
                                                   ContentText = x.ContentText,
                                               }
                                           })
                                           .FirstAsync();
        }

        public async Task AppendUpdateFieldTemplate(FieldTemplateMessage request)
        {
            FieldTemplate data;
            if (request.Id <= 0)
            {
                data = new FieldTemplate();
                await objectCatalogRepository.AddAsync(data);
            }
            else
            {
                data = await objectCatalogRepository.All<FieldTemplate>()
                                                    .Where(x => x.Id == request.Id)
                                                    .FirstAsync();
            }
            data.FieldTypeId = request.FieldTypeId;
            data.BlankIfNoValue = request.BlankIfNoValue;
            data.Name = request.Name;
            await objectCatalogRepository.SaveChangesAsync();
        }
        public async Task UpdateFieldTemplateContent(FieldTemplateContentMessage request)
        {
            var data = await objectCatalogRepository.All<FieldTemplate>()
                                                    .Where(x => x.Id == request.Id)
                                                    .FirstAsync();
            data.Content = request.Content;
            data.ContentText = request.ContentText;
            await objectCatalogRepository.SaveChangesAsync();
        }
        /// <summary>
        /// Изтрива бланка по идентификатор
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteFieldTemplate(int id)
        {
            var template = await objectCatalogRepository.All<FieldTemplate>()
              .TagWith(nameof(DeleteFieldTemplate))
              .IgnoreQueryFilters()
              .SingleOrDefaultAsync(f => f.Id == id);

            template.IsActive = false;
            await objectCatalogRepository.SaveChangesAsync();
        }
    }

}
