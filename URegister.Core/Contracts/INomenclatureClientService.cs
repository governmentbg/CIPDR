using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Common;
using URegister.Core.Models.Nomenclature;
using URegister.NomenclaturesCatalog;

namespace URegister.Core.Contracts
{
    public interface INomenclatureClientService : IBaseService
    {
        /// <summary>
        /// Мапване на номенклатурен тип към GRPC
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        NomenclatureTypeRequest NomenclatureTypeToGrpcModel(NomenclatureTypeVM model);

        /// <summary>
        /// Добавя номенклатури за регистър във ViewData
        /// </summary>
        /// <param name="viewData">view data</param>
        /// <returns></returns>
        Task SetViewBagRegister(ViewDataDictionary viewData);

        /// <summary>
        /// Мапване на номенклатурен тип от GRPC
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        NomenclatureTypeVM GrpcNomenclatureTypeToModel(NomenclatureTypeResponse response);
            
        /// <summary>
        /// Мапване на номенклатурна стойност от GRPC
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        CodeableConceptVM GrpcCodeableConceptToModel(CodeableConceptResponse response);


        /// <summary>
        /// Мапване на номенклатурна стойност към GRPC
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        CodeableConceptRequest CodeableConceptToGrpcModel(CodeableConceptVM model);

        /// <summary>
        /// Списък на номенклатурнани стойности за регистър
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<IActionResult> GetNomenclatureTypeRegisterList(IDataTablesRequest request, NomenclatureTypeRegisterFilterVM filter);

        /// <summary>
        /// Списък на номенклатурнани стойности за регистър
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<IActionResult> GetCodeableConceptRegisterList(IDataTablesRequest request, NomenclatureTypeRegisterFilterVM filter);

        /// <summary>
        /// Запис на влогове за номенклатурен тип
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<ResultStatus> UpdateNomenclatureTypeRegister(NomenclatureTypeRegisterUpdateVM model);
        Task<ResultStatus> UpdateCodeableConceptRegister(CodeableConceptRegisterUpdateVM model);
        string GetNomenclatureValue(List<NomenclatureTypePublicResponse> nomenclatureTypes, string nomType, string code);
        Task<List<NomenclatureTypePublicResponse>> GetNomenclaturePublic(int registerId, string[] types);
    }
}
