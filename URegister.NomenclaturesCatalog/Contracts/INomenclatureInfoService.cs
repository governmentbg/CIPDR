
using Grpc.Core;
using URegister.Common;

namespace URegister.NomenclaturesCatalog.Contracts
{
    public interface INomenclatureInfoService
    {
        Task AddCodeableConcept(CodeableConceptRequest request);
        Task AddNomenclatureType(NomenclatureTypeRequest request);
        Task<CodeableConceptResponse> CreateNewCodeableConcept(string nomenclatureType);
        Task<NomenclatureTypeResponse> CreateNewNomenclatureType();
        Task EditCodeableConcept(CodeableConceptRequest request);
        Task<bool> EditNomenclatureType(NomenclatureTypeRequest request);
        Task<CodeableConceptResponse> GetCodeableConcept(long id);
        Task<CodeableConceptListResponse> GetCodeableConceptList(CodeableConceptListRequest request);
        Task<List<EkatteItemPublic>> GetEkattePublic(EkattePublicRequest request);
        Task<NomenclaturePublicResponse> GetNomenclaturePublic(NomenclaturePublicRequest request);
        Task<NomenclatureTypeRegisterListResponse> GetNomenclatureTypeRegisterList(NomenclatureTypeRegisterListRequest request);
        Task<NomenclatureTypeResponse> GetNomenclatureType(int id);
        Task<NomenclatureTypeListResponse> GetNomenclatureTypeList(NomenclatureTypeListRequest request);
        Task<NomenclatureTypeResponse> GetNomenclatureTypeOnType(string nomenclatureType);
        Task<NomenclatureTypeListPublicResponse> GetNomenclatureTypesPublic(int registerId);
        Task UpdateNomenclatureTypeRegister(UpdateNomenclatureTypeRegisterRequest request);
        Task<CodeableConceptRegisterListResponse> GetCodeableConceptRegisterList(CodeableConceptRegisterListRequest request);
        Task<NomenclatureTypeRegisterResponse> GetNomenclatureTypeRegisterOnType(string nomenclatureType, int registerId);
        Task UpdateCodeableConceptRegister(UpdateCodeableConceptRegisterRequest request);
        Task<List<CheckNomenclatureResponseItem>> CheckNomenclature(CheckNomenclatureRequest request);
        Task<NomenclatureTypePublicResponse> GetNomenclatureOnHolderPublic(NomenclatureHolderRequest request);

        /// <summary>
        /// Проверява дали подадената стойност е измежу позволените стойности за регистъра
        /// </summary>
        /// <param name="request">Заявка с параметри</param>
        /// <returns></returns>
        Task<bool> AreNomenclatureCodesAllowed(AreNomenclatureCodesAllowedRequest request);

        /// <summary>
        /// Върща текста на номенклатура по кода и
        /// </summary>
        /// <param name="request">Заявка с параметри</param>
        /// <returns></returns>
        Task<string> GetValueByCode(GetValueRequest request);
        Task UpdateCodeableConceptStatus(UpdateCodeableConceptStatusRequest request);

        /// <summary>
        /// Изтриване на номенклатура
        /// </summary>
        /// <param name="nomenclatureTypeId">Идентификатор на номенклатура</param>
        /// <returns></returns>
        Task<ResultStatus> DeleteNomenclatureType(int nomenclatureTypeId);

        /// <summary>
        /// Четене на номенклатура с категории
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns> request);
        Task<List<NomenclatureWithHolderItem>> GetNomenclatureWithHolderValues(NomenclatureWithHolderValuesRequest request);

        Task<CodeableConceptListResponse> GetCodeableConceptListExport(CodeableConceptListExportRequest request);

        /// <summary>
        /// Четене на имена на населени места с област и община по екатте код
        /// </summary>
        /// <param name="ekatteCode"></param>
        /// <returns></returns>
        Task<string> GetSettlementFullInfo(string ekatteCode);
    }
}
