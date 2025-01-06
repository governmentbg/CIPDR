using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Contracts;
using URegister.Infrastructure.Extensions;
using URegister.NomenclaturesCatalog.Constants;
using URegister.NomenclaturesCatalog.Contracts;
using URegister.NomenclaturesCatalog.Data.Models;
using URegister.NomenclaturesCatalog.Infrastructure.Data.Models.Nomenclatures;
using URegister.NomenclaturesCatalog.Model.Ekatte;
using URegister.RegistersCatalog.Data;

namespace URegister.NomenclaturesCatalog.Services
{

    /// <summary>
    /// Управление на номенклатури и
    /// </summary>
    /// <param name="repo">Repository към базата данни</param>
    public class NomenclatureService(
        INomenclaturesCatalogRepository repo,
        IHttpRequester httpRequester,
        IConfiguration configuration
        ) : INomenclatureInfoService
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
            var codeableConcept = CodeableConceptGrpcToData(request);
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
                                                 x.Code == request.Code &&
                                                 x.DateTo == null)
                                     .ToListAsync();
            if (!concepts.Any())
            {
                throw new ArgumentException($"Няма запис за код {request.Code}");
            }
            var dateFrom = request.DateFrom.ToDateTime();
            foreach (var concept in concepts)
            {
                concept.DateTo = dateFrom;
            }
            var codeableConcept = CodeableConceptGrpcToData(request);
            await repo.AddAsync(codeableConcept);
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
                Type = codeableConcept.Type,
                Code = codeableConcept.Code,
                Value = codeableConcept.Value,
                ValueEn = codeableConcept.ValueEn,
                DateFrom = codeableConcept.DateFrom.SetToUtcIfUnspecified().ToTimestamp(),
                DateTo = codeableConcept.DateTo?.SetToUtcIfUnspecified().ToTimestamp(),
                ParentCode = codeableConcept.ParentCode,
                HolderCode = codeableConcept.HolderCode,
            };
            if (codeableConcept.DateTo == null)
            {
                result.DateFromInit = result.DateFrom;
            }
            else
            {
                result.DateFromInit = result.DateTo;
            }
            result.DateFromInit = result.DateFromInit.ToDateTime().AddDays(1).ToTimestamp();
            if (result.DateFromInit < DateTime.Today.SetToUtc().ToTimestamp())
            {
                result.DateFromInit = DateTime.Today.SetToUtc().ToTimestamp();
            }
            result.DateFrom = result.DateFromInit;
            result.DateTo = null;
            result.AdditionalColumns.AddRange(
                codeableConcept.AdditionalColumns.Select(x => new NomenclaturesCatalog.AdditionalColumn
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
        public CodeableConcept CodeableConceptGrpcToData(CodeableConceptRequest request)
        {
            CodeableConcept codeableConcept = new();
            codeableConcept.Type = request.Type;
            codeableConcept.Code = request.Code;
            codeableConcept.Value = request.Value;
            codeableConcept.ValueEn = request.ValueEn;
            codeableConcept.DateFrom = request.DateFrom.ToDateTime();
            codeableConcept.DateTo = request.DateTo?.ToDateTime();
            codeableConcept.CreatedBy = request.CreatedBy;
            codeableConcept.ParentCode = request.ParentCode;
            codeableConcept.CreatedOn = DateTime.UtcNow;
            codeableConcept.HolderCode = request.HolderCode;
            codeableConcept.AdditionalColumns = request.AdditionalColumns
                .Select(x => new Data.Models.AdditionalColumn
                {
                    Name = x.ColumnName,
                    Value = x.ValueBg,
                    ValueEn = x.ValueEn,
                })
                .ToList();

            return codeableConcept;
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
                IsPublic = request.IsPublic,
            };
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
                    HolderCode = item.ParentCode,
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
                var category = $"общ. {mun?.Value} обл. {region?.Value}";
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

            var codeableConceptsAll = await repo.AllReadonly<CodeableConcept>()
                                                 .Where(x => nomenclatureTypeArr.Contains(x.Type) &&
                                                             x.DateFrom <= forDate &&
                                                             forDate <= (x.DateTo ?? endDate))
                                                 .Where(filterCodeableConceptExp)
                                                 .Where(filterValueExp)
                                                 .ToListAsync();
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

        public async Task SaveCodeableConceptImport(CodeableConcept codeableConcept)
        {
            var prevList = await repo.All<CodeableConcept>()
                .TagWith(nameof(SaveCodeableConceptImport))
                                      .Where(x => x.Type == codeableConcept.Type &&
                                                  x.Code == codeableConcept.Code)
                                      .ToListAsync();
            foreach (var prev in prevList)
            {
                if (prev.DateTo == null)
                {
                    prev.DateTo = codeableConcept.DateFrom.AddDays(-1);
                }
            }
            codeableConcept.CreatedOn = DateTime.UtcNow;
            await repo.AddAsync(codeableConcept);
        }


        public async Task<List<EkDoc>> GetEkDocForImport(int status)
        {
            return await repo.All<EkDoc>()
                .TagWith(nameof(GetEkDocForImport))
                             .Where(x => x.Status < status)
                             .OrderBy(x => x.Doc_date)
                             .ToListAsync();
        }


        /// <summary>
        /// Импорт на Екатте данни за подразделения на населени места
        /// </summary>
        /// <returns></returns>
        public async Task ImportNrnmNsi()
        {
            // ImportEkDoc()
            await ImportArea1(NomenclatureTypes.EkArea1);
            await ImportArea2(NomenclatureTypes.EkArea2);
            await ImportRegions(NomenclatureTypes.EkRegion);
            await ImportMunicipalities(NomenclatureTypes.EkMunicipality);
            await ImportTownHalls(NomenclatureTypes.EkTownHall);
            await ImportEkatte(NomenclatureTypes.Ekatte);
            await ImportSobr(NomenclatureTypes.Ekatte);
            await ImportEkRaion(NomenclatureTypes.EkRaion);
        }

        /// <summary>
        /// Импорт на Area1
        /// </summary>
        /// <param name="nomenclatureType"></param>
        /// <returns></returns>
        public async Task ImportArea1(string nomenclatureType)
        {
            var status = ImportNsi.ImportStatus.Area1;
            var ekDocList = await GetEkDocForImport(status);
            if (!ekDocList.Any())
                return;
            var area1List = await httpRequester.GetAsync<List<Ek_reg1>>(configuration.GetValue<string>("NrnmNsi:BaseAddr")! + "/ek_reg1");
            foreach (var ekDoc in ekDocList)
            {
                foreach (var area1 in area1List.Where(x => x.Document == ekDoc.Document))
                {
                    var codeableConcept = new CodeableConcept
                    {
                        Code = area1.Region,
                        Value = area1.Name,
                        ValueEn = area1.Name_en,
                        Type = nomenclatureType,
                        DateFrom = ekDoc.Doc_date ?? DateTime.MinValue,
                    };
                    codeableConcept.AdditionalColumn(AdditionalColumnNames.Document, ekDoc.Document.ToString());
                    await SaveCodeableConceptImport(codeableConcept);
                }
                ekDoc.Status = status;
                await repo.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Импорт на Area2
        /// </summary>
        /// <param name="nomenclatureType"></param>
        /// <returns></returns>
        public async Task ImportArea2(string nomenclatureType)
        {
            var status = ImportNsi.ImportStatus.Area2;
            var ekDocList = await GetEkDocForImport(status);
            if (!ekDocList.Any())
                return;
            var area2List = await httpRequester.GetAsync<List<Ek_reg2>>(configuration.GetValue<string>("NrnmNsi:BaseAddr")! + "/ek_reg2");
            foreach (var ekDoc in ekDocList)
            {
                foreach (var area2 in area2List.Where(x => x.Document == ekDoc.Document))
                {
                    var codeableConcept = new CodeableConcept
                    {
                        Code = area2.Region,
                        Value = area2.Name,
                        ValueEn = area2.Name_en,
                        Type = nomenclatureType,
                        DateFrom = ekDoc.Doc_date ?? DateTime.MinValue,
                        HolderCode = area2.Nuts1,
                    };
                    codeableConcept.AdditionalColumn(AdditionalColumnNames.Document, ekDoc.Document.ToString());
                    await SaveCodeableConceptImport(codeableConcept);
                }
                ekDoc.Status = status;
                await repo.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Импорт на области
        /// </summary>
        /// <param name="nomenclatureType"></param>
        /// <returns></returns>
        public async Task ImportRegions(string nomenclatureType)
        {
            var status = ImportNsi.ImportStatus.Regions;
            var ekDocList = await GetEkDocForImport(status);
            if (!ekDocList.Any())
                return;
            var ek_oblList = await httpRequester.GetAsync<List<Ek_obl>>(configuration.GetValue<string>("NrnmNsi:BaseAddr")! + "/ek_obl");
            foreach (var ekDoc in ekDocList)
            {
                foreach (var ek_obl in ek_oblList.Where(x => x.Document == ekDoc.Document))
                {
                    var codeableConcept = new CodeableConcept
                    {
                        Code = ek_obl.Oblast,
                        Value = ek_obl.Name,
                        ValueEn = ek_obl.Name_en,
                        Type = nomenclatureType,
                        DateFrom = ekDoc.Doc_date ?? DateTime.MinValue,
                        HolderCode = ek_obl.Nuts2,
                    };
                    codeableConcept.AdditionalColumn(AdditionalColumnNames.Document, ekDoc.Document.ToString());
                    codeableConcept.AdditionalColumn(AdditionalColumnNames.Nuts3, ek_obl.Nuts3);
                    codeableConcept.AdditionalColumn(AdditionalColumnNames.Ekatte, ek_obl.Ekatte);
                    await SaveCodeableConceptImport(codeableConcept);
                }
                ekDoc.Status = status;
                await repo.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Импорт на общини
        /// </summary>
        /// <param name="nomenclatureType"></param>
        /// <returns></returns>
        public async Task ImportMunicipalities(string nomenclatureType)
        {
            var status = ImportNsi.ImportStatus.Municipalities;
            var ekDocList = await GetEkDocForImport(status);
            if (!ekDocList.Any())
                return;
            var ek_obstList = await httpRequester.GetAsync<List<Ek_obst>>(configuration.GetValue<string>("NrnmNsi:BaseAddr")! + "/ek_obst");
            foreach (var ekDoc in ekDocList)
            {
                foreach (var ek_obst in ek_obstList.Where(x => x.Document == ekDoc.Document))
                {
                    var codeableConcept = new CodeableConcept
                    {
                        Code = ek_obst.Obshtina,
                        Value = ek_obst.Name,
                        ValueEn = ek_obst.Name_en,
                        Type = nomenclatureType,
                        DateFrom = ekDoc.Doc_date ?? DateTime.MinValue,
                        HolderCode = ek_obst.Obshtina.Substring(0, 3),
                    };
                    codeableConcept.AdditionalColumn(AdditionalColumnNames.Document, ekDoc.Document.ToString());
                    codeableConcept.AdditionalColumn(AdditionalColumnNames.Nuts3, ek_obst.Nuts3);
                    codeableConcept.AdditionalColumn(AdditionalColumnNames.Ekatte, ek_obst.Ekatte);
                    codeableConcept.AdditionalColumn(AdditionalColumnNames.Category, ek_obst.Category.ToString());
                    await SaveCodeableConceptImport(codeableConcept);
                }
                ekDoc.Status = status;
                await repo.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Импорт на кметства
        /// </summary>
        /// <param name="nomenclatureType"></param>
        /// <returns></returns>
        public async Task ImportTownHalls(string nomenclatureType)
        {
            var status = ImportNsi.ImportStatus.Ek_kmet;
            var ekDocList = await GetEkDocForImport(status);
            if (!ekDocList.Any())
                return;
            var ek_kmetList = await httpRequester.GetAsync<List<Ek_kmet>>(configuration.GetValue<string>("NrnmNsi:BaseAddr")! + "/ek_kmet");
            foreach (var ekDoc in ekDocList)
            {
                foreach (var ek_kmet in ek_kmetList.Where(x => x.Document == ekDoc.Document))
                {
                    var codeableConcept = new CodeableConcept
                    {
                        Code = ek_kmet.Kmetstvo,
                        Value = ek_kmet.Name,
                        ValueEn = ek_kmet.Name_en,
                        Type = nomenclatureType,
                        DateFrom = ekDoc.Doc_date ?? DateTime.MinValue,
                        HolderCode = ek_kmet.Kmetstvo.Substring(0, 5),
                    };
                    codeableConcept.AdditionalColumn(AdditionalColumnNames.Document, ekDoc.Document.ToString());
                    codeableConcept.AdditionalColumn(AdditionalColumnNames.Ekatte, ek_kmet.Ekatte);
                    codeableConcept.AdditionalColumn(AdditionalColumnNames.Category, ek_kmet.Category?.ToString() ?? string.Empty);
                    await SaveCodeableConceptImport(codeableConcept);
                }
                ekDoc.Status = status;
                await repo.SaveChangesAsync();
            }
        }


        /// <summary>
        /// Импорт на населени места
        /// </summary>
        /// <param name="nomenclatureType"></param>
        /// <returns></returns>
        public async Task ImportEkatte(string nomenclatureType)
        {
            var status = ImportNsi.ImportStatus.Ekatte;
            var ekDocList = await GetEkDocForImport(status);
            if (!ekDocList.Any())
                return;
            var ek_atteList = await httpRequester.GetAsync<List<Ek_atte>>(configuration.GetValue<string>("NrnmNsi:BaseAddr")! + "/ek_atte");
            foreach (var ekDoc in ekDocList)
            {
                foreach (var ek_atte in ek_atteList.Where(x => x.Document == ekDoc.Document))
                {
                    var codeableConcept = new CodeableConcept
                    {
                        Code = ek_atte.Ekatte,
                        Value = ek_atte.T_v_m + " " + ek_atte.Name,
                        ValueEn = ek_atte.Name_en,
                        Type = nomenclatureType,
                        DateFrom = ekDoc.Doc_date ?? DateTime.MinValue,
                        HolderCode = ek_atte.Obshtina,
                    };
                    codeableConcept.AdditionalColumn(AdditionalColumnNames.Document, ekDoc.Document.ToString());
                    codeableConcept.AdditionalColumn(AdditionalColumnNames.Category, ek_atte.Category.ToString());
                    codeableConcept.AdditionalColumn(AdditionalColumnNames.Kind, ek_atte.Kind.ToString());
                    codeableConcept.AdditionalColumn(AdditionalColumnNames.Kmetstvo, ek_atte.Kmetstvo);
                    codeableConcept.AdditionalColumn(AdditionalColumnNames.TVM, ek_atte.T_v_m);
                    codeableConcept.AdditionalColumn(AdditionalColumnNames.Altitude, ek_atte.Altitude.ToString());
                    await SaveCodeableConceptImport(codeableConcept);
                }
                ekDoc.Status = status;
                await repo.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Импорт на селищни образувания
        /// </summary>
        /// <param name="nomenclatureType"></param>
        /// <returns></returns>
        public async Task ImportSobr(string nomenclatureType)
        {
            var status = ImportNsi.ImportStatus.Sobr;
            var ekDocList = await GetEkDocForImport(status);
            if (!ekDocList.Any())
                return;
            var ek_sobrList = await httpRequester.GetAsync<List<Ek_sobr>>(configuration.GetValue<string>("NrnmNsi:BaseAddr")! + "/ek_sobr");
            foreach (var ekDoc in ekDocList)
            {
                foreach (var sobr in ek_sobrList.Where(x => x.Document == ekDoc.Document))
                {
                    var codeableConcept = new CodeableConcept
                    {
                        Code = sobr.Ekatte,
                        Value = sobr.Name,
                        ValueEn = sobr.Name_en,
                        Type = nomenclatureType,
                        DateFrom = ekDoc.Doc_date ?? DateTime.MinValue,
                    };
                    var holderCode = sobr.Area1.Substring(1, 5);
                    var municipality = await repo.AllReadonly<CodeableConcept>()
                                                 .Where(x => x.Type == NomenclatureTypes.EkMunicipality &&
                                                             x.Code == holderCode)
                                                 .FirstOrDefaultAsync();
                    codeableConcept.HolderCode = municipality?.Code;

                    if (string.IsNullOrEmpty(codeableConcept.HolderCode))
                    {
                        var ekatte = await repo.AllReadonly<CodeableConcept>()
                                               .Where(x => x.Type == NomenclatureTypes.Ekatte &&
                                                           x.Code == holderCode)
                                               .FirstOrDefaultAsync();
                        codeableConcept.HolderCode = ekatte?.HolderCode;
                    }
                    codeableConcept.AdditionalColumn(AdditionalColumnNames.Document, ekDoc.Document.ToString());
                    codeableConcept.AdditionalColumn(AdditionalColumnNames.Kind, sobr.Kind.ToString());
                    codeableConcept.AdditionalColumn(AdditionalColumnNames.Area1, sobr.Area1);
                    codeableConcept.AdditionalColumn(AdditionalColumnNames.Area2, sobr.Area2);
                    await SaveCodeableConceptImport(codeableConcept);
                }
                ekDoc.Status = status;
                await repo.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Импорт на градски райони
        /// </summary>
        /// <param name="nomenclatureType"></param>
        /// <returns></returns>
        public async Task ImportEkRaion(string nomenclatureType)
        {
            var status = ImportNsi.ImportStatus.Raion;
            var ekDocList = await GetEkDocForImport(status);
            if (!ekDocList.Any())
                return;
            var ek_raionList = await httpRequester.GetAsync<List<Ek_raion>>(configuration.GetValue<string>("NrnmNsi:BaseAddr")! + "/ek_raion");
            foreach (var ekDoc in ekDocList)
            {
                foreach (var ek_raion in ek_raionList.Where(x => x.Document == ekDoc.Document))
                {
                    var codeableConcept = new CodeableConcept
                    {
                        Code = ek_raion.Raion,
                        Value = ek_raion.Name,
                        ValueEn = ek_raion.Name_en,
                        Type = nomenclatureType,
                        DateFrom = ekDoc.Doc_date ?? DateTime.MinValue,
                        HolderCode = ek_raion.Raion.Substring(0, 5),
                    };
                    codeableConcept.AdditionalColumn(AdditionalColumnNames.Document, ekDoc.Document.ToString());
                    await SaveCodeableConceptImport(codeableConcept);
                }
                ekDoc.Status = status;
                await repo.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Импорт ба документи
        /// </summary>
        /// <returns></returns>
        public async Task ImportEkDoc()
        {
            var docList = await httpRequester.GetAsync<List<Ek_doc>>(configuration.GetValue<string>("NrnmNsi:BaseAddr")! + "/ek_doc");
            foreach (var doc in docList)
            {
                var ekDoc = await repo.AllReadonly<EkDoc>()
                                      .Where(x => x.Document == doc.Document)
                                      .FirstOrDefaultAsync();
                if (ekDoc != null)
                    continue;
                ekDoc = new EkDoc
                {
                    Document = doc.Document ?? 0,
                    Doc_act = doc.Doc_act?.DateTime,
                    Doc_date = doc.Doc_date?.DateTime,
                    Doc_inst = doc.Doc_inst,
                    Doc_name = doc.Doc_name,
                    Doc_kind = doc.Doc_kind,
                    Doc_name_en = doc.Doc_name_en,
                    Doc_num = doc.Doc_num,
                    Dv_danni = doc.Dv_danni,
                    Dv_date = doc.Dv_date?.DateTime,
                };
                await repo.AddAsync(ekDoc);
            }
            await repo.SaveChangesAsync();
        }


        /// <summary>
        /// Импорт на улици
        /// </summary>
        /// <param name="nomenclatureType"></param>
        /// <returns></returns>
        public async Task ImportEkStreet(string nomenclatureType)
        {
            var dateStart = new DateTime(1900, 1, 1);
            var ek_streetList = await httpRequester.GetAsync<List<Ek_street>>(configuration.GetValue<string>("NrnmNsi:BaseAddr")! + "/streets");
            var ek_streets = await repo.All<CodeableConcept>()
                                       .Where(x => x.Type == nomenclatureType)
                                       .ToListAsync();
            // 1 - ПЛ.
            //2 - БЪЛ.
            //3 - УЛ.

            //4 - Ж.К.
            //5 - КВ.
            //9 - Друго
            int[] categories = [];
            if (nomenclatureType == NomenclatureTypes.EkStreet)
            {
                categories = [1, 2, 3];
            }
            if (nomenclatureType == NomenclatureTypes.EkKvartal)
            {
                categories = [4, 5, 9];
            }
            ek_streetList = ek_streetList.Where(x => categories.Contains(x.Street_type ?? 0)).ToList();
            foreach (var ek_street in ek_streetList)
            {
                var codeableConcept = new CodeableConcept
                {
                    Code = $"{ek_street.Street_code}_{ek_street.City_code}",
                    Value = ek_street.Name,
                    Type = nomenclatureType,
                    DateFrom = ek_street.Valid_from?.DateTime ?? dateStart,
                    HolderCode = ek_street.Actual_city,
                };
                codeableConcept.AdditionalColumn(AdditionalColumnNames.CityCode, ek_street.City_code);
                if (ek_street.Street_type != null)
                {
                    codeableConcept.AdditionalColumn(AdditionalColumnNames.Category, ek_street.Street_type.ToString()!);
                }
                var ek_streetsPrev = ek_streets.Where(x => x.Code == codeableConcept.Code);
                var ek_streetTo = ek_streetsPrev.Where(x => x.DateFrom == codeableConcept.DateFrom).FirstOrDefault();
                if (ek_streetTo != null)
                {
                    ek_streetTo.DateTo = codeableConcept.DateTo;
                    continue;
                }
                foreach (var prev in ek_streetsPrev)
                {
                    if (prev.DateTo == null)
                    {
                        prev.DateTo = codeableConcept.DateFrom.AddDays(-1);
                    }
                }
                codeableConcept.CreatedOn = DateTime.UtcNow;
                await repo.AddAsync(codeableConcept);
            }
            await repo.SaveChangesAsync();
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
                    IsValid = result.IsValidAllType ? true : isValidList?.Any(x => x == item.Code) ?? false
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
                .Where(x => x.Type == request.NomenclatureType &&
                            x.DateFrom <= forDate &&
                            forDate <= (x.DateTo ?? endDate))
                .FirstOrDefaultAsync(x => x.Code == request.NomenclatureCode);

            if (concept == null)
            {
                return string.Empty;
            }

            return concept.Value;
        }
    }
}

