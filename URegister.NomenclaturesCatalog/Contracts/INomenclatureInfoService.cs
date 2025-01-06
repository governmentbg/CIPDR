
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
        Task ImportArea1(string nomenclatureType);
        Task ImportNrnmNsi();
        Task UpdateNomenclatureTypeRegister(UpdateNomenclatureTypeRegisterRequest request);
        Task<CodeableConceptRegisterListResponse> GetCodeableConceptRegisterList(CodeableConceptRegisterListRequest request);
        Task<NomenclatureTypeRegisterResponse> GetNomenclatureTypeRegisterOnType(string nomenclatureType, int registerId);
        Task UpdateCodeableConceptRegister(UpdateCodeableConceptRegisterRequest request);
        Task<List<CheckNomenclatureResponseItem>> CheckNomenclature(CheckNomenclatureRequest request);
        Task ImportEkStreet(string nomenclatureType);
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
    }
}
