
using DataTables.AspNet.Core;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using URegister.Common;
using URegister.Core.Contracts;
using URegister.Core.Models.Nomenclature;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Extensions;
using URegister.NomenclaturesCatalog;

namespace URegister.Core.Services
{
    public class NomenclatureClientService(NomenclatureGrpc.NomenclatureGrpcClient nomenclatureGrpcClient) : INomenclatureClientService
    {
        public async Task SetViewBag(ViewDataDictionary viewData, string[] types, int registerId)
        {
            var request = new NomenclaturePublicRequest
            {
                RegisterId = registerId,
            };
            request.NomenclatureTypes.Add(types);
            var result = await nomenclatureGrpcClient.GetNomenclaturePublicAsync(request);
            foreach (var nType in result.NomenclatureTypes)
            {
                var ddl = nType.CodeableConcepts.Select(x => new SelectListItem
                {
                    Value = x.Code,
                    Text = x.Value
                })
                .ToList();
                AddChoice(ddl, "Изберете");
                viewData[$"{nType.Type}_ddl"] = ddl;
            }

        }
        public void AddChoice(List<SelectListItem> ddl, string addChoiceText)
        {
            ddl.Insert(0,
                new SelectListItem
                {
                    Value = null,
                    Text = addChoiceText,
                    Disabled = true,
                    Selected = true,
                });
        }

        /// <summary>
        /// Добавя номенклатури за регистър във ViewData
        /// </summary>
        /// <param name="viewData">view data</param>
        /// <returns></returns>
        public async Task SetViewBagRegister(ViewDataDictionary viewData)
        {
            var types = new string[]{
                InternalNomenclatureTypes.RegisterType,
                InternalNomenclatureTypes.RegisterEntryType,
                InternalNomenclatureTypes.RegisterIdentitySecurityLevel,
                InternalNomenclatureTypes.PersonType,
            };
            await SetViewBag(viewData, types, 0);
        }


        /// <summary>
        /// Мапване на номенклатурен тип към GRPC
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public NomenclatureTypeRequest NomenclatureTypeToGrpcModel(NomenclatureTypeVM model)
        {
            var request = new NomenclatureTypeRequest
            {
                Type = model.Type,
                Name = model.Name,
                HolderType = model.HolderType,
                IsPublic = model.IsPublic,
            };
            return request;
        }

        /// <summary>
        /// Мапване на номенклатурен тип от GRPC
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public NomenclatureTypeVM GrpcNomenclatureTypeToModel(NomenclatureTypeResponse response)
        {
            return new NomenclatureTypeVM
            {
                Type = response.Type,
                Name = response.Name,
                HolderType = response.HolderType,
                IsInsert = response.IsInsert,
                IsPublic = response.IsPublic,
            };
        }

        /// <summary>
        /// Мапване на номенклатурна стойност от GRPC
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public CodeableConceptVM GrpcCodeableConceptToModel(CodeableConceptResponse response)
        {
            return new CodeableConceptVM
            {
                Type = response.Type,
                Code = response.Code,
                Value = response.Value,
                ValueEn = response.ValueEn,
                DateFrom = response.DateFrom?.ToDateTime() ?? DateTime.Now.Date,
                DateFromInit = response.DateFromInit?.ToDateTime() ?? DateTime.Now.Date,
                DateTo = response.DateTo?.ToDateTime(),
                IsInsert = response.IsInsert,
                HolderCode = response.HolderCode,
                AdditionalColumns = response.AdditionalColumns
                   .Select(x => new AdditionalColumnVM
                   {
                       Name = x.ColumnName,
                       Value = x.ValueBg,
                       ValueEn = x.ValueEn,
                   })
                   .ToList()
            };
        }

        /// <summary>
        /// Мапване на номенклатурна стойност към GRPC
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public CodeableConceptRequest CodeableConceptToGrpcModel(CodeableConceptVM model)
        {
            var request = new CodeableConceptRequest
            {
                Type = model.Type,
                Code = model.Code,
                Value = model.Value,
                ValueEn = model.ValueEn,
                DateFrom = model.DateFrom.SetToUtcIfUnspecified().ToTimestamp(),
                DateTo = model.DateTo?.SetToUtcIfUnspecified().ToTimestamp(),
                HolderCode = model.HolderCode,
            };
            request.AdditionalColumns.AddRange(
                model.AdditionalColumns.Select(x => new AdditionalColumn
                {
                    ColumnName = x.Name,
                    ValueBg = x.Value,
                    ValueEn = x.ValueEn,
                }).ToArray());
            return request;
        }

        /// <summary>
        /// Списък на номенклатурнани стойности за регистър
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetNomenclatureTypeRegisterList(IDataTablesRequest request, NomenclatureTypeRegisterFilterVM filter)
        {
            var protoRequest = request!.GetDataTablesRequestProto();
            var result = await nomenclatureGrpcClient.GetNomenclatureTypeRegisterListAsync(new NomenclatureTypeRegisterListRequest
            {
                DataTableRequest = protoRequest,
                Type = filter.Type ?? string.Empty,
                Name = filter.Name ?? string.Empty,
                RegisterId = filter.RegisterId
            });
            return request.GetResponseServerPaging(result.Data, result.CountAll);
        }

        /// <summary>
        /// Списък на номенклатурнани стойности за регистър
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetCodeableConceptRegisterList(IDataTablesRequest request, NomenclatureTypeRegisterFilterVM filter)
        {
            var protoRequest = request!.GetDataTablesRequestProto();
            var result = await nomenclatureGrpcClient.GetCodeableConceptRegisterListAsync(
                new CodeableConceptRegisterListRequest
                {
                    DataTableRequest = protoRequest,
                    Type = filter.Type,
                    RegisterId = filter.RegisterId
                });
            return request.GetResponseServerPaging(result.Data, result.CountAll);
        }

        /// <summary>
        /// Запис на влогове за номенклатурен тип
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ResultStatus> UpdateNomenclatureTypeRegister(NomenclatureTypeRegisterUpdateVM model)
        {
            var request = new UpdateNomenclatureTypeRegisterRequest
            {
                RegisterId = model.RegisterId,
                Type = model.Type,
                FilterType = model.FilterType,
                FilterName = model.FilterName,
            };

            if (model.IsValid == false)
            {
                model.IsValidAll = false;
            }
            if (model.IsValidAll == true)
            {
                model.IsValid = true;
            }

            if (model.IsValid != null)
            {
                request.IsValid = model.IsValid == true;
            }
            if (model.IsValidAll != null)
            {
                request.IsValidAll = model.IsValidAll == true;
            }

            return await nomenclatureGrpcClient.UpdateNomenclatureTypeRegisterAsync(request);
        }

        /// <summary>
        /// Запис допустими номенклатурни стойности
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ResultStatus> UpdateCodeableConceptRegister(CodeableConceptRegisterUpdateVM model)
        {
            var request = new UpdateCodeableConceptRegisterRequest
            {
                Code = model.Code,
                Type = model.Type,
                RegisterId = model.RegisterId,
                IsValid = model.IsValid,
                Filter = model.Filter
            };
            return await nomenclatureGrpcClient.UpdateCodeableConceptRegisterAsync(request);
        }

        public async  Task<List<NomenclatureTypePublicResponse>> GetNomenclaturePublic(int registerId, string[] types)
        {
            var nomenclatureRequest = new NomenclaturePublicRequest
            {
                RegisterId = registerId,
            };
            nomenclatureRequest.NomenclatureTypes.AddRange(types);
            return (await nomenclatureGrpcClient.GetNomenclaturePublicAsync(nomenclatureRequest))
                                    .NomenclatureTypes
                                    .ToList();
        }

        /// <summary>
        /// Извличане стойност на номенклатура
        /// </summary>
        /// <param name="nomenclatureTypes">списък от номенклатуре каталог</param>
        /// <param name="nomType">тип</param>
        /// <param name="code">код</param>
        /// <returns>Стойност</returns>
        public string GetNomenclatureValue(List<NomenclatureTypePublicResponse> nomenclatureTypes, string nomType, string code)
        {
            var nomenclatureType = nomenclatureTypes.Where(x => x.Type == nomType).FirstOrDefault();
            return nomenclatureType?.CodeableConcepts.Where(x => x.Code == code)
                                                     .Select(x => x.Value)
                                                     .FirstOrDefault() ?? string.Empty;
        }
    }
}
