using System.Data;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using URegister.Common;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Extensions;
using URegister.NomenclaturesCatalog.Contracts;
using URegister.NomenclaturesCatalog.Data.Models;
using URegister.NomenclaturesCatalog.Data;

namespace URegister.NomenclaturesCatalog.Services
{

    /// <summary>
    /// Управление на номенклатури и
    /// </summary>
    /// <param name="repo">Repository към базата данни</param>
    public class NomenclatureInfoService(
        INomenclaturesCatalogRepository repo,
        ILogger<NomenclatureInfoService> logger) : INomenclatureInfoService
    {
        /// <summary>
        /// Добавяне на номенклатурна стойност
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task AddCodeableConcept(CodeableConceptRequest request)
        {
            var haveConcepts = await repo.All<CodeableConcept>()
                                    .TagWith(nameof(AddCodeableConcept))
                                    .AnyAsync(x => x.Type == request.Type &&
                                                 x.Code == request.Code &&
                                                 x.DateTo == null);
            if (haveConcepts)
            {
                throw new ArgumentException($"Има запис за код {request.Code}");
            }

            var duplicateName = await repo.All<CodeableConcept>()
                .TagWith(nameof(AddCodeableConcept))
                .AnyAsync(x => x.Type == request.Type &&
                               EF.Functions.ILike(x.Value,  request.Value) &&
                               x.DateTo == null);
            if (duplicateName)
            {
                throw new ArgumentException($"Има запис за стойност {request.Value}");
            }

            var duplicateNameEn = await repo.All<CodeableConcept>()
                .TagWith(nameof(AddCodeableConcept))
                .AnyAsync(x => x.Type == request.Type &&
                               EF.Functions.ILike(x.ValueEn, request.ValueEn) &&
                               x.DateTo == null);
            if (duplicateNameEn)
            {
                throw new ArgumentException($"Има запис за стойност EN {request.ValueEn}");
            }

            var codeableConcept = new CodeableConcept();
            SetCodeableConceptFromGrpcToData(request, codeableConcept);
            await repo.AddAsync(codeableConcept);
            await repo.SaveChangesAsync();
        }

        /// <summary>
        /// Редакция на номенклатурна стойност
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task EditCodeableConcept(CodeableConceptRequest request)
        {
            var concepts = await repo.All<CodeableConcept>()
                                     .TagWith(nameof(EditCodeableConcept))
                                     .Where(x => x.Type == request.Type &&
                                                 x.Code == request.Code)
                                     .ToListAsync();
            if (!concepts.Any())
            {
                throw new ArgumentException($"Няма запис за код {request.Code}");
            }
            var dateFrom = request.DateFrom.ToDateTime();
            var conceptPrev = concepts.Where(x => x.Id != request.Id)
                                      .OrderByDescending(x => x.DateFrom)
                                      .FirstOrDefault();
            if (conceptPrev != null)
            {
                conceptPrev.DateTo = dateFrom.AddDays(-1);
            }
            var codeableConcept = new CodeableConcept();
            if (request.Id > 0)
            {
                codeableConcept = await repo.All<CodeableConcept>()
                                     .TagWith(nameof(EditCodeableConcept))
                                     .Include(x => x.AdditionalColumns)
                                     .Where(x => x.Id == request.Id)
                                     .FirstAsync();
            }
            else
            {
                await repo.AddAsync(codeableConcept);
            }
            SetCodeableConceptFromGrpcToData(request, codeableConcept);

            await repo.SaveChangesAsync();
        }
        /// <summary>
        /// Четене на номенклатурна стойност
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<CodeableConceptResponse> GetCodeableConcept(long id)
        {
            var codeableConcept = await repo.AllReadonly<CodeableConcept>()
                                    .TagWith(nameof(GetCodeableConcept))
                                    .Include(x => x.AdditionalColumns)
                                    .Where(x => x.Id == id)
                                    .FirstOrDefaultAsync();

            if (codeableConcept == null)
                throw new ArgumentException($"Няма запис за идентификатор {id}");

            var result = new CodeableConceptResponse
            {
                Id = codeableConcept.Id,
                Type = codeableConcept.Type,
                Code = codeableConcept.Code,
                Value = codeableConcept.Value,
                ValueEn = codeableConcept.ValueEn,
                DateFrom = codeableConcept.DateFrom.SetToUtc().ToTimestamp(),
                DateTo = codeableConcept.DateTo?.SetToUtc().ToTimestamp(),
                ParentCode = codeableConcept.ParentCode,
                HolderCode = codeableConcept.HolderCode,
            };
            if (codeableConcept.DateFrom > DateTime.Today)
            {
                result.DateFromInit = DateTime.Today.AddDays(1).SetToUtc().ToTimestamp();
            }
            else
            {
                result.Id = 0;
                if (codeableConcept.DateTo == null)
                {
                    result.DateFromInit = result.DateFrom;
                }
                else
                {
                    result.DateFromInit = result.DateTo;
                }
                if (result.DateFromInit < DateTime.Today.SetToUtc().ToTimestamp())
                {
                    result.DateFromInit = DateTime.Today.SetToUtc().ToTimestamp();
                }
                result.DateFromInit = result.DateFromInit.ToDateTime().AddDays(1).ToTimestamp();
                result.DateFrom = result.DateFromInit;
                result.DateTo = null;
            }
            result.AdditionalColumns.AddRange(
                codeableConcept.AdditionalColumns!.Select(x => new AdditionalColumn
                {
                    ColumnName = x.Name,
                    ValueBg = x.Value,
                    ValueEn = x.ValueEn
                })
                .ToArray());
            return result;
        }

        /// <summary>
        /// Мапване на номенклатурна стойност
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public void SetCodeableConceptFromGrpcToData(CodeableConceptRequest request, CodeableConcept codeableConcept)
        {
            codeableConcept.Type = request.Type;
            codeableConcept.Code = request.Code;
            codeableConcept.Value = request.Value;
            codeableConcept.ValueEn = request.ValueEn;
            codeableConcept.DateFrom = request.DateFrom.ToDateTime();
            codeableConcept.DateTo = request.DateTo?.ToDateTime();
            // TODO: codeableConcept.CreatedBy = request.CreatedBy;
            codeableConcept.ParentCode = request.ParentCode;
            codeableConcept.CreatedOn = DateTime.UtcNow;
            codeableConcept.HolderCode = request.HolderCode;
            codeableConcept.Status = request.Status;
            if (codeableConcept.AdditionalColumns != null)
            {
                codeableConcept.AdditionalColumns.Clear();
            }
            codeableConcept.AdditionalColumns = request.AdditionalColumns
                .Select(x => new Data.Models.AdditionalColumn
                {
                    Name = x.ColumnName,
                    Value = x.ValueBg,
                    ValueEn = x.ValueEn,
                })
                .ToList();
        }

        /// <summary>
        /// апване на номенклатурен тип
        /// </summary>
        /// <param name="nomType"></param>
        /// <returns></returns>
        private NomenclatureTypeResponse NomenclatureTypeToGrpc(NomenclatureType nomType)
        {
            var result = new NomenclatureTypeResponse
            {
                Type = nomType.Type,
                Name = nomType.Name,
                HolderType = nomType.HolderType,
                IsPublic = nomType.IsPublic,
            };
            return result;
        }

        /// <summary>
        /// Четене на номенклатурен тип
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<NomenclatureTypeResponse> GetNomenclatureType(int id)
        {
            var nomType = await repo.AllReadonly<NomenclatureType>()
                                    .TagWith(nameof(GetNomenclatureType))
                                    .Include(x => x.Registers)
                                    .Where(x => x.Id == id)
                                    .FirstOrDefaultAsync();

            if (nomType == null)
                throw new ArgumentException($"Няма запис за идентификатор {id}");
            return NomenclatureTypeToGrpc(nomType);
        }

        /// <summary>
        /// Четене на номенклатурен тип
        /// </summary>
        /// <param name="nomenclatureType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<NomenclatureTypeResponse> GetNomenclatureTypeOnType(string nomenclatureType)
        {
            var nomType = await repo.AllReadonly<NomenclatureType>()
                                    .TagWith(nameof(GetNomenclatureTypeOnType))
                                    .Where(x => x.Type == nomenclatureType)
                                    .FirstOrDefaultAsync();
            if (nomType == null)
                throw new ArgumentException($"Няма запис за код {nomenclatureType}");
            return NomenclatureTypeToGrpc(nomType);
        }

        /// <summary>
        /// Четене на номенклатурен тип
        /// </summary>
        /// <param name="nomenclatureType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<NomenclatureTypeRegisterResponse> GetNomenclatureTypeRegisterOnType(string nomenclatureType, int registerId)
        {
            var nomType = await repo.AllReadonly<NomenclatureType>()
                                    .TagWith(nameof(GetNomenclatureTypeRegisterOnType))
                                    .Include(x => x.Registers.Where(x => x.RegisterId == registerId))
                                    .Where(x => x.Type == nomenclatureType)
                                    .FirstOrDefaultAsync();
            if (nomType == null)
                throw new ArgumentException($"Няма запис за код {nomenclatureType}");
            return new NomenclatureTypeRegisterResponse
            {

                Type = nomType.Type,
                Name = nomType.Name,
                HolderType = nomType.HolderType,
                IsValidAll = nomType.Registers.FirstOrDefault()?.IsValidAllItems ?? false,
            };
        }


        /// <summary>
        /// Инициализиране на номенклатурен тип
        /// </summary>
        /// <returns></returns>
        public async Task<NomenclatureTypeResponse> CreateNewNomenclatureType()
        {
            var result = new NomenclatureTypeResponse
            {
                Type = string.Empty,
                Name = string.Empty,
                IsInsert = true
            };
            return result;
        }

        /// <summary>
        /// Списък номенклатурни типове
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<NomenclatureTypeListResponse> GetNomenclatureTypeList(NomenclatureTypeListRequest request)
        {
            var query = repo.AllReadonly<NomenclatureType>();
            if (!string.IsNullOrEmpty(request.Name))
            {
                query = query.Where(x => EF.Functions.ILike(x.Name, request.Name.ToPaternSearch()));
            }
            if (!string.IsNullOrEmpty(request.Type))
            {
                query = query.Where(x => EF.Functions.ILike(x.Type, request.Type.ToPaternSearch()));
            }

            var result = new NomenclatureTypeListResponse();
            (query, result.CountAll) = await request.DataTableRequest.GetFilteredData(query);
            var data = await query.ToListAsync();
            foreach (var item in data)
            {
                var resultItem = new NomenclatureTypeItem
                {
                    Id = item.Id,
                    Type = item.Type,
                    Name = item.Name,
                };
                result.Data.Add(resultItem);
            }

            result.ResultStatus = new ResultStatus { Code = ResultCodes.Ok };
            return result;
        }


        /// <summary>
        /// Списък номенклатурни типове
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<NomenclatureTypeRegisterListResponse> GetNomenclatureTypeRegisterList(NomenclatureTypeRegisterListRequest request)
        {
            var query = repo.AllReadonly<NomenclatureType>()
                            .Where(x => x.IsPublic);
            if (!string.IsNullOrEmpty(request.Name))
            {
                query = query.Where(x => EF.Functions.ILike(x.Name, request.Name.ToPaternSearch()));
            }
            if (!string.IsNullOrEmpty(request.Type))
            {
                query = query.Where(x => EF.Functions.ILike(x.Type, request.Type.ToPaternSearch()));
            }

            var result = new NomenclatureTypeRegisterListResponse();
            (query, result.CountAll) = await request.DataTableRequest.GetFilteredData(query);
            var data = await query.Include(x => x.Registers.Where(r => r.RegisterId == request.RegisterId))
                                  .ToListAsync();
            foreach (var item in data)
            {
                var resultItem = new NomenclatureTypeRegisterItem
                {
                    Id = item.Id,
                    Type = item.Type,
                    Name = item.Name,
                    IsValid = item.Registers.FirstOrDefault()?.IsValid == true,
                    IsValidAll = item.Registers.FirstOrDefault()?.IsValidAllItems == true,
                };
                result.Data.Add(resultItem);
            }

            return result;
        }

        /// <summary>
        /// Добавяне на номенклатурен тип
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task AddNomenclatureType(NomenclatureTypeRequest request)
        {
            NomenclatureType nType = new()
            {
                Type = request.Type,
                Name = request.Name,
                HolderType = request.HolderType,
                IsPublic = request.IsPublic
            };

            if (await repo.AllReadonly<NomenclatureType>()
                    .TagWith(nameof(AddNomenclatureType))
                    .AnyAsync(t =>
                        EF.Functions.ILike(t.Type, nType.Type) ||
                        EF.Functions.ILike(t.Name, nType.Name)))
            {
                throw new DuplicateNameException($"Тип '{nType.Type}' или име '{nType.Name}' вече съществуват");
            }

            await repo.AddAsync(nType);
            await repo.SaveChangesAsync();
        }

        /// <summary>
        /// Редакция на номенклатурен тип
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<bool> EditNomenclatureType(NomenclatureTypeRequest request)
        {
            var nType = await repo.All<NomenclatureType>()
                                  .TagWith(nameof(EditNomenclatureType))
                                  .Include(x => x.Registers)
                                  .Where(x => x.Type == request.Type)
                                  .FirstOrDefaultAsync();
            if (nType == null)
            {
                return false;
            }
            nType.Type = request.Type;
            nType.Name = request.Name;
            nType.HolderType = request.HolderType;
            nType.IsPublic = request.IsPublic;
            await repo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CheckIfIdExists(long? id)
        {
            return id == null || await repo.All<CodeableConcept>().AnyAsync(c => c.Id == id);
        }

        /// <summary>
        /// Списък номенклатурни стойности
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<CodeableConceptListResponse> GetCodeableConceptList(CodeableConceptListRequest request)
        {
            NomenclaturePublicRequest nomRequest = new();
            nomRequest.NomenclatureTypes.Add(InternalNomenclatureTypes.CodeableConceptStatus);
            var nomResponce = await GetNomenclaturePublic(nomRequest);
            var statusList = nomResponce.NomenclatureTypes.First().CodeableConcepts;
            var cQuery = repo.AllReadonly<CodeableConcept>()
                            .TagWith(nameof(GetCodeableConceptList))
                            .Where(x => x.Type == request.Type);
            var query = cQuery.Where(x => x.DateTo == null)
                              .Union(cQuery.Where(x => DateTime.Today <= x.DateTo)
                                           .Where(x => !repo.AllReadonly<CodeableConcept>().Any(
                                                                   c => x.Type == c.Type &&
                                                                        x.Code == c.Code &&
                                                                        x.DateFrom < c.DateFrom)
                                           )
                               );

            var result = new CodeableConceptListResponse();
            (query, result.CountAll) = await request.DataTableRequest.GetFilteredData(query);
            var data = await query.ToListAsync();
            foreach (var item in data)
            {
                var resultItem = new CodeableConceptItem
                {
                    Id = item.Id,
                    Code = item.Code,
                    Value = item.Value,
                    ValueEn = item.ValueEn,
                    DateFrom = item.DateFrom.SetToUtcIfUnspecified().ToTimestamp(),
                    DateTo = item.DateTo?.SetToUtcIfUnspecified().ToTimestamp(),
                    ParentCode = item.ParentCode,
                    HolderCode = item.HolderCode,
                    StatusId = item.Status,
                    Status = statusList.Where(l => l.Code == item.Status.ToString()).Select(x => x.Value).FirstOrDefault()
                };

                result.Data.Add(resultItem);
            }

            return result;
        }

        /// <summary>
        /// Инициализиране на номенклатурна стойност
        /// </summary>
        /// <param name="nomenclatureType"></param>
        /// <returns></returns>
        public async Task<CodeableConceptResponse> CreateNewCodeableConcept(string nomenclatureType)
        {
            var result = new CodeableConceptResponse
            {
                Type = nomenclatureType,
                IsInsert = true,
            };
            result.DateFromInit = DateTime.Today.SetToUtc().ToTimestamp();
            result.DateFrom = result.DateFromInit;
            return result;
        }

        /// <summary>
        /// Връща Екатте резултати по зададена заявка
        /// </summary>
        /// <param name="request">Заявка</param>
        /// <returns></returns>
        public async Task<List<EkatteItemPublic>> GetEkattePublic(EkattePublicRequest request)
        {
            var endDate = new DateTime(2100, 1, 1);
            var forDate = DateTime.Today;

            var reply = new List<EkatteItemPublic>();

            var nomenclatureTypeArr = new string[]{
                NomenclatureTypes.EkMunicipality,
                NomenclatureTypes.EkRegion,
                NomenclatureTypes.Ekatte,
            };
            Expression<Func<CodeableConcept, bool>> filterValueExp = x => true;
            if (request.HasFilterValue)
            {
                filterValueExp = x => EF.Functions.ILike(x.Value, request.FilterValue.ToPaternSearch());
            }
            Expression<Func<NomenclatureType, bool>> filterNomenclatureTypeExp = x => true;
            if (request.RegisterId > 0)
            {
                filterNomenclatureTypeExp = x => !x.IsPublic || x.Registers.Any(r => r.RegisterId == request.RegisterId && r.IsValidAllItems);
            }
            var nomenclatureTypeValidAllItems = await repo.AllReadonly<NomenclatureType>()
                                                          .TagWith(nameof(GetEkattePublic))
                                                          .Where(filterNomenclatureTypeExp)
                                                          .Where(x => nomenclatureTypeArr.Contains(x.Type))
                                                          .Select(x => x.Type)
                                                          .ToArrayAsync();

            Expression<Func<CodeableConcept, bool>> filterCodeableConceptExp = x => true;
            if (request.RegisterId > 0)
            {
                var registers = repo.AllReadonly<CodeableConceptRegister>()
                    .TagWith(nameof(GetEkattePublic))
                    .Where(x => x.IsValid);
                filterCodeableConceptExp = x => nomenclatureTypeValidAllItems.Contains(x.Type) ||
                                                          registers.Any(a => a.RegisterId == request.RegisterId &&
                                                                             a.Type == x.Type &&
                                                                             a.Code == x.Code);
            }
            var codeableConcepts = await repo.AllReadonly<CodeableConcept>()
                                               .TagWith(nameof(GetEkattePublic))
                                               .Where(x => x.Type == NomenclatureTypes.Ekatte &&
                                                           x.DateFrom <= forDate &&
                                                           forDate <= (x.DateTo ?? endDate))
                                               .Where(filterCodeableConceptExp)
                                               .Where(filterValueExp)
                                               .OrderBy(x => x.Value)
                                               .ToListAsync();
            nomenclatureTypeArr =
            [
                NomenclatureTypes.EkMunicipality,
                NomenclatureTypes.EkRegion
            ];

            var codeableConceptMR = await repo.AllReadonly<CodeableConcept>()
                                              .TagWith(nameof(GetEkattePublic))
                                              .Where(x => nomenclatureTypeArr.Contains(x.Type) &&
                                                          x.DateFrom <= forDate &&
                                                          forDate <= (x.DateTo ?? endDate))
                                              .Where(filterCodeableConceptExp)
                                              .ToListAsync();

            foreach (var codeableConcept in codeableConcepts)
            {
                var mun = codeableConceptMR.Where(x => x.Type == NomenclatureTypes.EkMunicipality && x.Code == codeableConcept.HolderCode).FirstOrDefault();
                var region = codeableConceptMR.Where(x => x.Type == NomenclatureTypes.EkRegion && x.Code == mun?.HolderCode).FirstOrDefault();
                var category = $"обл. {region?.Value} общ. {mun?.Value}";
                var item = reply.Where(x => x.Category == category).FirstOrDefault();
                if (item == null)
                {
                    item = new EkatteItemPublic
                    {
                        Category = category
                    };
                    reply.Add(item);
                }
                item.Cities.Add(new EkatteSettlementPublic { Code = codeableConcept.Code, Name = codeableConcept.Value });
            }
            return reply;
        }

        /// <summary>
        /// Връща Екатте резултати по зададена заявка
        /// </summary>
        /// <param name="request">Заявка</param>
        /// <returns></returns>
        public async Task<List<NomenclatureWithHolderItem>> GetNomenclatureWithHolderValues(NomenclatureWithHolderValuesRequest request)
        {
            var endDate = new DateTime(2100, 1, 1);
            var forDate = DateTime.Today;

            var reply = new List<NomenclatureWithHolderItem>();

            Expression<Func<CodeableConcept, bool>> filterValueExp = x => true;
            if (request.HasFilterValue)
            {
                filterValueExp = x => EF.Functions.ILike(x.Value, request.FilterValue.ToPaternSearch());
            }
            Expression<Func<NomenclatureType, bool>> filterNomenclatureTypeExp = x => true;
            if (request.RegisterId > 0)
            {
                filterNomenclatureTypeExp = x => !x.IsPublic || x.Registers.Any(r => r.RegisterId == request.RegisterId && r.IsValidAllItems);
            }

            NomenclatureType nomenclatureType = await repo.AllReadonly<NomenclatureType>()
                .TagWith(nameof(GetNomenclatureWithHolderValues))
                .Where(filterNomenclatureTypeExp)
                .FirstOrDefaultAsync(x => x.Type == request.NomenclatureCode);

            if (nomenclatureType == null)
            {
                logger.LogError($"Не е намерен номенклатурен тип {request.NomenclatureCode} в {nameof(GetNomenclatureWithHolderValues)}");
                return reply;
            }

            var codeableConcepts = await repo.AllReadonly<CodeableConcept>()
                                               .TagWith(nameof(GetNomenclatureWithHolderValues))
                                               .Where(x => x.Type == request.NomenclatureCode &&
                                                           x.DateFrom <= forDate &&
                                                           forDate <= (x.DateTo ?? endDate))
                                               //.Where(filterCodeableConceptExp)
                                               .Where(filterValueExp)
                                               .OrderBy(x => x.Value)
                                               .ToListAsync();

            List<string> holderCodes = codeableConcepts.Select(c => c.HolderCode).Distinct().ToList();

            var codeableConceptsHolder = await repo.AllReadonly<CodeableConcept>()
                                              .TagWith(nameof(GetNomenclatureWithHolderValues))
                                              .Where(x => x.Type == nomenclatureType.HolderType &&
                                                          x.DateFrom <= forDate &&
                                                          forDate <= (x.DateTo ?? endDate))
                                              .Where(x => holderCodes.Contains(x.Code))
                                              .ToListAsync();

            foreach (var codeableConcept in codeableConcepts)
            {
                string category;
                var holderValue = codeableConceptsHolder.FirstOrDefault(c => c.Code == codeableConcept.HolderCode);
                if (holderValue != null)
                {
                    category = holderValue.Value;
                }
                else
                {
                    category = "Неизвестна категория";
                }

                var item = reply.Where(x => x.Category == category).FirstOrDefault();
                if (item == null)
                {
                    item = new NomenclatureWithHolderItem
                    {
                        Category = category
                    };
                    reply.Add(item);
                }
                item.Entities.Add(new EkatteSettlementPublic { Code = codeableConcept.Code, Name = codeableConcept.Value });
            }
            return reply;
        }

        /// <summary>
        /// Четене на номенклатури за регистър
        /// </summary>
        /// <param name="request">Заявка с инфромация</param>
        /// <returns></returns>
        public async Task<NomenclaturePublicResponse> GetNomenclaturePublic(NomenclaturePublicRequest request)
        {
            var endDate = new DateTime(2100, 1, 1);
            var forDate = DateTime.Today;
            Expression<Func<NomenclatureType, bool>> typesExp = x => true;
            if (request.NomenclatureTypes.Any())
            {
                typesExp = x => request.NomenclatureTypes.Contains(x.Type);
            }
            Expression<Func<NomenclatureType, bool>> filterNomenclatureTypeExp = x => true;
            if (request.RegisterId > 0)
            {
                filterNomenclatureTypeExp = x => !x.IsPublic || x.Registers.Any(r => r.RegisterId == request.RegisterId && r.IsValid);
            }
            var nomenclatureTypes = await repo.AllReadonly<NomenclatureType>()
                                              .TagWith(nameof(GetNomenclaturePublic))
                                              .Include(x => x.Registers.Where(a => a.RegisterId == request.RegisterId))
                                              .Where(filterNomenclatureTypeExp)
                                              .Where(typesExp)
                                              .ToListAsync();
            var reply = new NomenclaturePublicResponse();
            Expression<Func<CodeableConcept, bool>> filterValueExp = x => true;
            if (request.HasFilterValue)
            {
                filterValueExp = x => EF.Functions.ILike(x.Value, request.FilterValue.ToPaternSearch());
            }
            var nomenclatureTypeArr = nomenclatureTypes
                .Select(x => x.Type)
                .ToArray();
            var nomenclatureTypeValidAllItems = nomenclatureTypes
                .Where(x => !x.IsPublic || x.Registers.Any(x => x.IsValidAllItems))
                .Select(x => x.Type)
                .ToArray();
            Expression<Func<CodeableConcept, bool>> filterCodeableConceptExp = x => true;
            if (request.RegisterId > 0)
            {
                var registers = repo.AllReadonly<CodeableConceptRegister>().Where(x => x.IsValid);
                filterCodeableConceptExp = x => nomenclatureTypeValidAllItems.Contains(x.Type) ||
                                                          registers.Any(a => a.RegisterId == request.RegisterId &&
                                                                             a.Type == x.Type &&
                                                                             a.Code == x.Code);
            }

            var query = repo.AllReadonly<CodeableConcept>()
                .Where(x => nomenclatureTypeArr.Contains(x.Type))
                .Where(x => x.Status == (int)CodeableConceptStatus.Confirmed)
                .Where(filterCodeableConceptExp)
                .Where(filterValueExp);

            if (request.SkipDateCheck)
            {
                query = query
                    .GroupBy(x => new { x.Type, x.Code })
                    .Select(g => g.OrderByDescending(x => x.DateFrom).First());
            }
            else
            {
                query = query.Where(x => x.DateFrom <= forDate && forDate <= (x.DateTo ?? endDate));
            }

            var codeableConceptsAll = await query.ToListAsync();

            foreach (var nomenclatureType in nomenclatureTypes)
            {
                var nomenclatureTypeTo = new NomenclatureTypePublicResponse
                {
                    Type = nomenclatureType.Type,
                    Name = nomenclatureType.Name,
                };
                var codeableConcepts = codeableConceptsAll
                    .Where(x => x.Type == nomenclatureTypeTo.Type)
                    .Select(x => new CodeableConceptPublicResponse
                    {
                        Code = x.Code,
                        Value = x.Value,
                        ValueEn = x.ValueEn,
                    })
                    .ToArray();
                nomenclatureTypeTo.CodeableConcepts.AddRange(codeableConcepts);
                reply.NomenclatureTypes.Add(nomenclatureTypeTo);
            }
            return reply;
        }




        /// <summary>
        /// Четене на номенклатури за регистър
        /// </summary>
        /// <param name="registerId"></param>
        /// <returns></returns>
        public async Task<NomenclatureTypeListPublicResponse> GetNomenclatureTypesPublic(int registerId)
        {
            Expression<Func<NomenclatureType, bool>> filterNomenclatureTypeExp = x => true;
            if (registerId > 0)
            {
                filterNomenclatureTypeExp = x => x.Registers.Any(r => r.RegisterId == registerId && r.IsValid);
            }
            var nomenclatureTypes = await repo.AllReadonly<NomenclatureType>()
                                              .Where(filterNomenclatureTypeExp)
                                              .ToListAsync();
            var reply = new NomenclatureTypeListPublicResponse();
            foreach (var nomenclatureType in nomenclatureTypes)
            {
                var nomenclatureTypeTo = new NomenclatureTypeItem
                {
                    Id = nomenclatureType.Id,
                    Type = nomenclatureType.Type,
                    Name = nomenclatureType.Name,
                };
                reply.NomenclatureTypes.Add(nomenclatureTypeTo);
            }
            return reply;
        }

        /// <summary>
        /// Обновяване на регистър за номенглатурни типове
        /// </summary>
        /// <param name="request">Заявка с информация</param>
        /// <returns></returns>
        public async Task UpdateNomenclatureTypeRegister(UpdateNomenclatureTypeRegisterRequest request)
        {

            Expression<Func<NomenclatureType, bool>> filterTypeExp = string.IsNullOrEmpty(request.Type) ?
                                                    x => true :
                                                    x => x.Type == request.Type;

            Expression<Func<NomenclatureType, bool>> filterTypeLikeExp = string.IsNullOrEmpty(request.FilterType) ?
                                                    x => true :
                                                    x => EF.Functions.ILike(x.Type, request.FilterType.ToPaternSearch());

            Expression<Func<NomenclatureType, bool>> filterNameLikeExp = string.IsNullOrEmpty(request.FilterName) ?
                                                    x => true :
                                                    x => EF.Functions.ILike(x.Name, request.FilterName.ToPaternSearch());

            using (var transaction = await repo.BeginTransactionAsync())
            {
                var nomenclatureTypes = await repo.AllReadonly<NomenclatureType>()
                    .TagWith(nameof(UpdateNomenclatureTypeRegister))
                                                 .Where(x => x.IsPublic)
                                                 .Where(filterTypeExp)
                                                 .Where(filterTypeLikeExp)
                                                 .Where(filterNameLikeExp)
                                                 .Select(x => x.Id)
                                                 .ToArrayAsync();

                var queryRegisters = repo.All<NomenclatureTypeRegister>()
                                         .Where(x => nomenclatureTypes.Contains(x.NomenclatureTypeId) &&
                                                     x.RegisterId == request.RegisterId);


                var nomenclatureTypeRegisters = await queryRegisters.ToListAsync();
                DateTime now = DateTime.UtcNow;
                foreach (var id in nomenclatureTypes)
                {
                    if (!nomenclatureTypeRegisters.Any(x => x.NomenclatureTypeId == id))
                    {
                        var nomenclatureTypeRegister = new NomenclatureTypeRegister
                        {
                            NomenclatureTypeId = id,
                            RegisterId = request.RegisterId,
                            IsValid = request.IsValid,
                            IsValidAllItems = request.IsValidAll,
                            CreatedBy = request.UpdatedBy,
                            CreatedOn = now
                        };
                        await repo.AddAsync(nomenclatureTypeRegister);
                    }
                }
                await repo.SaveChangesAsync();

                if (request.HasIsValid && request.HasIsValidAll)
                {
                    await queryRegisters
                        .Where(x => x.IsValid != request.IsValid || x.IsValidAllItems != request.IsValidAll)
                        .ExecuteUpdateAsync(c => c
                            .SetProperty(p => p.IsValid, request.IsValid)
                            .SetProperty(p => p.IsValidAllItems, request.IsValidAll)
                            .SetProperty(p => p.CreatedBy, request.UpdatedBy)
                            .SetProperty(p => p.CreatedOn, now));
                }
                if (!request.HasIsValid && request.HasIsValidAll)
                {
                    await queryRegisters
                        .Where(x => x.IsValidAllItems != request.IsValidAll)
                        .ExecuteUpdateAsync(c => c
                            .SetProperty(p => p.IsValidAllItems, request.IsValidAll)
                            .SetProperty(p => p.CreatedBy, request.UpdatedBy)
                            .SetProperty(p => p.CreatedOn, now));
                }
                if (request.HasIsValid && !request.HasIsValidAll)
                {
                    await queryRegisters
                        .Where(x => x.IsValid != request.IsValid)
                        .ExecuteUpdateAsync(c => c
                            .SetProperty(p => p.IsValid, request.IsValid)
                            .SetProperty(p => p.CreatedBy, request.UpdatedBy)
                            .SetProperty(p => p.CreatedOn, now));
                }
                await transaction.CommitAsync();
            }
        }
        /// <summary>
        /// Списък номенклатурни стойности
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<CodeableConceptRegisterListResponse> GetCodeableConceptRegisterList(CodeableConceptRegisterListRequest request)
        {
            var result = new CodeableConceptRegisterListResponse();
            NomenclaturePublicRequest nomRequest = new();
            nomRequest.NomenclatureTypes.Add(InternalNomenclatureTypes.CodeableConceptStatus);
            var nomResponce = await GetNomenclaturePublic(nomRequest);
            var statusList = nomResponce.NomenclatureTypes.First().CodeableConcepts;


            var nomenclatureType = await repo.AllReadonly<NomenclatureType>()
                .TagWith(nameof(GetCodeableConceptRegisterList))
                                             .Include(x => x.Registers.Where(x => x.RegisterId == request.RegisterId))
                                             .Where(x => x.Type == request.Type)
                                             .FirstOrDefaultAsync();
            result.IsValidAllType = nomenclatureType?.Registers.FirstOrDefault()?.IsValidAllItems == true;
            var query = repo.AllReadonly<CodeableConcept>()
                            .Where(x => x.Type == request.Type &&
                                        (x.DateTo == null ||
                                         DateTime.Today <= x.DateTo))
                            .Where(x => !repo.AllReadonly<CodeableConcept>().Any(
                                c => x.Type == c.Type &&
                                     x.Code == c.Code &&
                                     x.DateFrom < c.DateFrom));

            (query, result.CountAll) = await request.DataTableRequest.GetFilteredData(query);
            var data = await query.ToListAsync();
            List<string>? isValidList = null;
            if (!result.IsValidAllType)
            {
                isValidList = await repo.AllReadonly<CodeableConceptRegister>()
                    .TagWith(nameof(GetCodeableConceptRegisterList))
                                        .Where(x => x.Type == request.Type &&
                                                    x.RegisterId == request.RegisterId &&
                                                    x.IsValid)
                                        .Select(x => x.Code)
                                        .ToListAsync();
            }
            foreach (var item in data)
            {
                var resultItem = new CodeableConceptRegisterItem
                {
                    Id = item.Id,
                    Code = item.Code,
                    Value = item.Value,
                    ValueEn = item.ValueEn,
                    DateFrom = item.DateFrom.SetToUtcIfUnspecified().ToTimestamp(),
                    DateTo = item.DateTo?.SetToUtcIfUnspecified().ToTimestamp(),
                    ParentCode = item.ParentCode,
                    HolderCode = item.ParentCode,
                    IsValid = result.IsValidAllType ? true : isValidList?.Any(x => x == item.Code) ?? false,
                    StatusId = item.Status,
                    Status = statusList.Where(l => l.Code == item.Status.ToString()).Select(x => x.Value).FirstOrDefault()
                };
                result.Data.Add(resultItem);
            }

            return result;
        }

        /// <summary>
        /// Обновяване на регистъра с номенклатурни стойности
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task UpdateCodeableConceptRegister(UpdateCodeableConceptRegisterRequest request)
        {

            Expression<Func<CodeableConcept, bool>> filterCodeExp = string.IsNullOrEmpty(request.Code) ?
                                                    x => true :
                                                    x => x.Code == request.Code;

            using (var transaction = await repo.BeginTransactionAsync())
            {
                var query = repo.AllReadonly<CodeableConcept>()
                    .TagWith(nameof(UpdateCodeableConceptRegister))
                                .Where(filterCodeExp)
                                .Where(x => x.Type == request.Type);

                if (!string.IsNullOrEmpty(request.Filter))
                {
                    query = query.Where(
                        x => EF.Functions.ILike(x.Code, request.Filter.ToPaternSearch()) ||
                             EF.Functions.ILike(x.Value, request.Filter.ToPaternSearch())
                     );
                }
                var codeableConcepts = await query.Select(x => x.Code)
                                                 .Distinct()
                                                 .ToArrayAsync();

                var queryRegisters = repo.All<CodeableConceptRegister>()
                    .TagWith(nameof(UpdateCodeableConceptRegister))
                                         .Where(x => codeableConcepts.Contains(x.Code) &&
                                                     x.Type == request.Type &&
                                                     x.RegisterId == request.RegisterId);


                var codeableConceptRegisters = await queryRegisters.ToListAsync();
                DateTime now = DateTime.UtcNow;
                foreach (var code in codeableConcepts)
                {
                    if (!codeableConceptRegisters.Any(x => x.Code == code))
                    {
                        var codeableConceptRegister = new CodeableConceptRegister
                        {
                            Code = code,
                            Type = request.Type,
                            RegisterId = request.RegisterId,
                            IsValid = request.IsValid,
                            CreatedBy = request.UpdatedBy,
                            CreatedOn = now
                        };
                        await repo.AddAsync(codeableConceptRegister);
                    }
                }
                await repo.SaveChangesAsync();

                await queryRegisters
                        .Where(x => x.IsValid != request.IsValid)
                        .ExecuteUpdateAsync(c => c
                            .SetProperty(p => p.IsValid, request.IsValid)
                            .SetProperty(p => p.CreatedBy, request.UpdatedBy)
                            .SetProperty(p => p.CreatedOn, now));
                transaction.Commit();
            }
        }


        /// <summary>
        /// Обновяване на статус на номенклатурни стойност
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task UpdateCodeableConceptStatus(UpdateCodeableConceptStatusRequest request)
        {
            var codeableConcept = await repo.All<CodeableConcept>()
                                      .TagWith(nameof(UpdateCodeableConceptStatus))
                                      .Where(x => x.Id == request.Id)
                                      .FirstAsync();
            codeableConcept.Status = request.StatusId;
            await repo.SaveChangesAsync();
        }
        /// <summary>
        /// Проверка на номенклатура
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<List<CheckNomenclatureResponseItem>> CheckNomenclature(CheckNomenclatureRequest request)
        {
            var types = request.Data.Select(x => x.Type).Distinct().ToList();
            var nomenclatureTypes = await repo.AllReadonly<NomenclatureType>()
                                           .Include(x => x.Registers.Where(x => x.RegisterId == request.RegisterId))
                                           .Where(x => types.Contains(x.Type))
                                           .ToListAsync();
            var result = request.Data.Select(x => new CheckNomenclatureResponseItem
            {
                FieldPath = x.FieldPath,
                Type = x.Type,
                Code = x.Code,
            })
            .ToList();
            foreach (var item in result)
            {
                item.IsValid = nomenclatureTypes.Any(x => x.Type == item.Type &&
                                                          (!x.IsPublic || x.Registers.Any(r => r.IsValid)));
                if (!item.IsValid)
                {
                    item.Error = "Номенклатурният тип не е допустим за регистъра";
                }
            }
            var codes = result.Where(x => x.IsValid)
                              .Select(x => x.Code)
                              .Distinct()
                              .ToList();

            var codeableConcepts = await repo.AllReadonly<CodeableConcept>()
                .TagWith(nameof(CheckNomenclature))
                                             .Where(x => types.Contains(x.Type))
                                             .Where(x => codes.Contains(x.Code))
                                             .ToListAsync();

            var checkTypes = nomenclatureTypes.Where(x => x.IsPublic && !x.Registers.Any(r => r.IsValidAllItems))
                                              .Select(x => x.Type)
                                              .ToList();
            foreach (var item in result)
            {
                if (item.IsValid)
                {
                    item.IsValid = codeableConcepts.Any(x => x.Type == item.Type && x.Code == item.Code);
                    if (!item.IsValid)
                    {
                        item.Error = "Номенклатурната стойност не е намерена";
                    }
                }
            }

            codes = result.Where(x => x.IsValid && checkTypes.Contains(x.Type))
                              .Select(x => x.Code)
                              .Distinct()
                              .ToList();

            var codeableConceptRegisters = await repo.AllReadonly<CodeableConceptRegister>()
                .TagWith(nameof(CheckNomenclature))
                                             .Where(x => x.RegisterId == request.RegisterId)
                                             .Where(x => codes.Contains(x.Code))
                                             .Where(x => checkTypes.Contains(x.Type))
                                             .ToListAsync();
            foreach (var item in result)
            {
                if (item.IsValid && checkTypes.Contains(item.Type))
                {
                    item.IsValid = codeableConceptRegisters.Any(x => x.Type == item.Type && x.Code == item.Code);
                    if (!item.IsValid)
                    {
                        item.Error = "Номенклатурната стойност не е допустима за регистъра";
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Четене на номенклатурни стойности за регистър
        /// </summary>
        /// <param name="request">Заявка с информация</param>
        /// <returns></returns>
        public async Task<NomenclatureTypePublicResponse> GetNomenclatureOnHolderPublic(NomenclatureHolderRequest request)
        {
            var endDate = new DateTime(2100, 1, 1);
            var forDate = DateTime.Today;
            Expression<Func<NomenclatureType, bool>> filterNomenclatureTypeExp = x => true;
            if (request.RegisterId > 0)
            {
                filterNomenclatureTypeExp = x => !x.IsPublic || x.Registers.Any(r => r.RegisterId == request.RegisterId && r.IsValid);
            }
            var nomenclatureType = await repo.AllReadonly<NomenclatureType>()
                .TagWith(nameof(GetNomenclatureOnHolderPublic))
                                              .Include(x => x.Registers.Where(a => a.RegisterId == request.RegisterId))
                                              .Where(filterNomenclatureTypeExp)
                                              .Where(x => x.Type == request.NomenclatureType)
                                              .FirstAsync();
            var reply = new NomenclaturePublicResponse();
            Expression<Func<CodeableConcept, bool>> filterValueExp = x => true;
            if (request.HasFilterValue)
            {
                filterValueExp = x => EF.Functions.ILike(x.Value, request.FilterValue.ToPaternSearch());
            }
            Expression<Func<CodeableConcept, bool>> filterCodeableConceptExp = x => true;
            if (request.RegisterId > 0 && nomenclatureType.IsPublic && !nomenclatureType.Registers.Any(x => x.IsValidAllItems))
            {
                var registers = repo.AllReadonly<CodeableConceptRegister>().Where(x => x.IsValid);
                filterCodeableConceptExp = x => registers.Any(a => a.RegisterId == request.RegisterId &&
                                                                             a.Type == x.Type &&
                                                                             a.Code == x.Code);
            }

            var codeableConceptsAll = await repo.AllReadonly<CodeableConcept>()
                                                 .Where(x => x.Type == request.NomenclatureType &&
                                                             x.HolderCode == request.Holder &&
                                                             x.DateFrom <= forDate &&
                                                             forDate <= (x.DateTo ?? endDate))
                                                 .Where(filterCodeableConceptExp)
                                                 .Where(filterValueExp)
                                                 .Where(x => x.Status == (int)CodeableConceptStatus.Confirmed)
                                                 .ToListAsync();
            var nomenclatureTypeTo = new NomenclatureTypePublicResponse
            {
                Type = nomenclatureType.Type,
                Name = nomenclatureType.Name,
            };
            var codeableConcepts = codeableConceptsAll
                .Where(x => x.Type == nomenclatureTypeTo.Type)
                .Select(x => new CodeableConceptPublicResponse
                {
                    Code = x.Code,
                    Value = x.Value,
                    ValueEn = x.ValueEn,
                })
                .ToArray();
            nomenclatureTypeTo.CodeableConcepts.AddRange(codeableConcepts);
            return nomenclatureTypeTo;
        }

        /// <summary>
        /// Проверява дали подадената стойност е измежу позволените стойности за регистъра
        /// </summary>
        /// <param name="request">Заявка с параметри</param>
        /// <returns></returns>
        public async Task<bool> AreNomenclatureCodesAllowed(AreNomenclatureCodesAllowedRequest request)
        {
            var endDate = new DateTime(2100, 1, 1);
            var forDate = DateTime.Today;
            Expression<Func<NomenclatureType, bool>> filterNomenclatureTypeExp = x => true;
            if (request.RegisterId > 0)
            {
                filterNomenclatureTypeExp = x => !x.IsPublic || x.Registers.Any(r => r.RegisterId == request.RegisterId && r.IsValid);
            }
            var nomenclatureType = await repo.AllReadonly<NomenclatureType>()
                                              .TagWith(nameof(AreNomenclatureCodesAllowed))
                                              .Include(x => x.Registers.Where(a => a.RegisterId == request.RegisterId))
                                              .Where(filterNomenclatureTypeExp)
                                              .Where(x => x.Type == request.NomenclatureType)
                                              .FirstAsync();

            Expression<Func<CodeableConcept, bool>> filterCodeableConceptExp = x => true;
            if (request.RegisterId > 0 && nomenclatureType.IsPublic && !nomenclatureType.Registers.Any(x => x.IsValidAllItems))
            {
                var registers = repo.AllReadonly<CodeableConceptRegister>().Where(x => x.IsValid);
                filterCodeableConceptExp = x => registers.Any(a => a.RegisterId == request.RegisterId &&
                                                                             a.Type == x.Type &&
                                                                             a.Code == x.Code);
            }

            int foundCodes = await repo.AllReadonly<CodeableConcept>()
                .TagWith(nameof(AreNomenclatureCodesAllowed))
                .Where(x => x.Type == request.NomenclatureType &&
                            (!request.HasHolder || x.HolderCode == request.Holder) &&
                            x.DateFrom <= forDate &&
                            forDate <= (x.DateTo ?? endDate))
                .Where(filterCodeableConceptExp)
                .Where(x => x.Status == (int)CodeableConceptStatus.Confirmed)
                .CountAsync(x => request.NomenclatureCodes.Contains(x.Code));


            return foundCodes == request.NomenclatureCodes.Count;
        }

        /// <summary>
        /// Върща текста на номенклатура по кода и
        /// </summary>
        /// <param name="request">Заявка с параметри</param>
        /// <returns></returns>
        public async Task<string> GetValueByCode(GetValueRequest request)
        {
            var endDate = new DateTime(2100, 1, 1);
            var forDate = DateTime.Today;

            CodeableConcept concept = await repo.AllReadonly<CodeableConcept>()
                .TagWith(nameof(GetValueByCode))
                .Where(x => x.Type == request.NomenclatureType)
                .Where(x => request.SkipDateCheck || (x.DateFrom <= forDate && forDate <= (x.DateTo ?? endDate)))
                .FirstOrDefaultAsync(x => x.Code == request.NomenclatureCode);

            if (concept == null)
            {
                return string.Empty;
            }

            return concept.Value;
        }

        /// <summary>
        /// Изтриване на номенклатура
        /// </summary>
        /// <param name="nomenclatureTypeId">Идентификатор на номенклатура</param>
        /// <returns></returns>
        public async Task<ResultStatus> DeleteNomenclatureType(int nomenclatureTypeId)
        {
            try
            {

                NomenclatureType nomenclatureTypeToDelete = await repo.All<NomenclatureType>()
                    .TagWith(nameof(DeleteNomenclatureType))
                    .Where(nt => nt.Id == nomenclatureTypeId)
                    .Include(nt => nt.Registers)
                    .SingleOrDefaultAsync();

                if (nomenclatureTypeToDelete == null)
                {
                    return new ResultStatus
                    {
                        Code = ResultCodes.NotFound,
                        Message = $"Номенклатура с идентификатор {nomenclatureTypeId} не е намерена"
                    };
                }

                if (nomenclatureTypeToDelete.Registers.Any())
                {
                    return new ResultStatus
                    {
                        Code = ResultCodes.BadRequest,
                        Message = "Номенклатурата се използва в съществуващи регистри."
                    };
                }

                repo.Delete(nomenclatureTypeToDelete);
                await repo.SaveChangesAsync();
                return new ResultStatus
                {
                    Code = ResultCodes.Ok,
                };
            }
            catch (Exception e)
            {
                //logger.LogError(e, $"Проблем при триене на стъпка с идентификатор {stepId} в {nameof(DeleteStep)}");
                return new ResultStatus
                {
                    Code = ResultCodes.InternalServerError,
                    Message = $"Проблем при триене на номенклатура с идентификатор {nomenclatureTypeId}"
                };
            }
        }

        /// <summary>
        /// Списък номенклатурни стойности
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<CodeableConceptListResponse> GetCodeableConceptListExport(CodeableConceptListExportRequest request)
        {
            NomenclaturePublicRequest nomRequest = new();
            nomRequest.NomenclatureTypes.Add(InternalNomenclatureTypes.CodeableConceptStatus);
            var nomResponce = await GetNomenclaturePublic(nomRequest);
            var statusList = nomResponce.NomenclatureTypes.First().CodeableConcepts;
            var cQuery = repo.AllReadonly<CodeableConcept>()
                            .TagWith(nameof(GetCodeableConceptList))
                            .Where(x => x.Type == request.Type);

            var query = cQuery.Where(x => x.DateTo == null)
                              .Union(cQuery.Where(x => DateTime.Today <= x.DateTo)
                                           .Where(x => !repo.AllReadonly<CodeableConcept>().Any(
                                                                   c => x.Type == c.Type &&
                                                                        x.Code == c.Code &&
                                                                        x.DateFrom < c.DateFrom)
                                           )
                               );

            var result = new CodeableConceptListResponse();
            var data = await query.ToListAsync();
            foreach (var item in data)
            {
                var resultItem = new CodeableConceptItem
                {
                    Id = item.Id,
                    Code = item.Code,
                    Value = item.Value,
                    ValueEn = item.ValueEn,
                    DateFrom = item.DateFrom.SetToUtcIfUnspecified().ToTimestamp(),
                    DateTo = item.DateTo?.SetToUtcIfUnspecified().ToTimestamp(),
                    ParentCode = item.ParentCode,
                    HolderCode = item.HolderCode,
                    StatusId = item.Status,
                    Status = statusList.Where(l => l.Code == item.Status.ToString()).Select(x => x.Value).FirstOrDefault()
                };

                result.Data.Add(resultItem);
            }

            result.CountAll = data.Count;
            return result;
        }

        /// <summary>
        /// Четене на имена на населени места с област и община по екатте код
        /// </summary>
        /// <param name="ekatteCode"></param>
        /// <returns></returns>
        public async Task<string> GetSettlementFullInfo(string ekatteCode)
        {
            var endDate = new DateTime(2100, 1, 1);
            var forDate = DateTime.Today;

            var reply = new List<EkatteItemPublic>();

            var nomenclatureTypeArr = new string[]{
                NomenclatureTypes.EkMunicipality,
                NomenclatureTypes.EkRegion,
                NomenclatureTypes.Ekatte,
            };

            var ekatteRecordForCode = await repo.AllReadonly<CodeableConcept>()
                                               .TagWith(nameof(GetEkattePublic))
                                               .Where(x => x.Type == NomenclatureTypes.Ekatte &&
                                                           x.DateFrom <= forDate &&
                                                           forDate <= (x.DateTo ?? endDate))
                                               .FirstOrDefaultAsync(x => x.Code == ekatteCode);

            nomenclatureTypeArr =
            [
                NomenclatureTypes.EkMunicipality,
                NomenclatureTypes.EkRegion
            ];

            var codeableConceptMR = await repo.AllReadonly<CodeableConcept>()
                                              .TagWith(nameof(GetEkattePublic))
                                              .Where(x => nomenclatureTypeArr.Contains(x.Type) &&
                                                          x.DateFrom <= forDate &&
                                                          forDate <= (x.DateTo ?? endDate))
                                              .ToListAsync();

            
            var municipality = codeableConceptMR.Where(x => x.Type == NomenclatureTypes.EkMunicipality && x.Code == ekatteRecordForCode.HolderCode).FirstOrDefault();
            var region = codeableConceptMR.Where(x => x.Type == NomenclatureTypes.EkRegion && x.Code == municipality?.HolderCode).FirstOrDefault();
            return $"обл. {region?.Value} общ. {municipality?.Value} {ekatteRecordForCode.Value}";
        }
    }
}

