using DataTables.AspNet.AspNetCore;
using Microsoft.EntityFrameworkCore;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Contracts;
using URegister.Infrastructure.Services;
using URegister.NomenclaturesCatalog.Constants;
using URegister.NomenclaturesCatalog.Contracts;
using URegister.NomenclaturesCatalog.Data.Models;
using URegister.NomenclaturesCatalog.Infrastructure.Data.Models.Nomenclatures;
using URegister.NomenclaturesCatalog.Model.Ekatte;
using URegister.NomenclaturesCatalog.Data;

namespace URegister.NomenclaturesCatalog.Services
{
    public class ImportNrnmNsiService(
        INomenclaturesCatalogRepository repo,
        IHttpRequester httpRequester,
        IConfiguration configuration
    ) : IImportNrnmNsiService
    {
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
            codeableConcept.Status = (int)CodeableConceptStatus.Confirmed;
            codeableConcept.StatusOn = DateTime.UtcNow;
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
            var area1List = await httpRequester.GetAsync<List<Ek_reg1>>(string.Empty, configuration.GetValue<string>("NrnmNsi:BaseAddr")! + "/ek_reg1");
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
            var area2List = await httpRequester.GetAsync<List<Ek_reg2>>(string.Empty, configuration.GetValue<string>("NrnmNsi:BaseAddr")! + "/ek_reg2");
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
            var ek_oblList = await httpRequester.GetAsync<List<Ek_obl>>(string.Empty, configuration.GetValue<string>("NrnmNsi:BaseAddr")! + "/ek_obl");
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
            var ek_obstList = await httpRequester.GetAsync<List<Ek_obst>>(string.Empty, configuration.GetValue<string>("NrnmNsi:BaseAddr")! + "/ek_obst");
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
            var ek_kmetList = await httpRequester.GetAsync<List<Ek_kmet>>(string.Empty, configuration.GetValue<string>("NrnmNsi:BaseAddr")! + "/ek_kmet");
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
            var ek_atteList = await httpRequester.GetAsync<List<Ek_atte>>(string.Empty, configuration.GetValue<string>("NrnmNsi:BaseAddr")! + "/ek_atte");
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
            var ek_sobrList = await httpRequester.GetAsync<List<Ek_sobr>>(string.Empty, configuration.GetValue<string>("NrnmNsi:BaseAddr")! + "/ek_sobr");
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
            var ek_raionList = await httpRequester.GetAsync<List<Ek_raion>>(string.Empty, configuration.GetValue<string>("NrnmNsi:BaseAddr")! + "/ek_raion");
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
            var docList = await httpRequester.GetAsync<List<Ek_doc>>(string.Empty, configuration.GetValue<string>("NrnmNsi:BaseAddr")! + "/ek_doc");
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
            var ek_streetList = await httpRequester.GetAsync<List<Ek_street>>(string.Empty, configuration.GetValue<string>("NrnmNsi:BaseAddr")! + "/streets");
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
    }
}
