using Microsoft.AspNetCore.Mvc;
using URegister.Common;
using URegister.NomenclaturesCatalog;

namespace URegister.ApiGateway.Controllers
{
    public class NomenclatureController(
        NomenclatureGrpc.NomenclatureGrpcClient nomenclatureGrpcClient,
        ILogger<NomenclatureController> logger)
        : BaseController
    {
        /// <summary>
        /// Връща списък от всички номенклатурни типове
        /// </summary>
        /// <returns></returns>
        [HttpGet("all-nomenclature-types")]
        public async Task<JsonResult> GetAllNomenclaturesTypes()
        {
            NomenclatureTypeListRequest request = new NomenclatureTypeListRequest
            {
                DataTableRequest = new DatatableRequest { Length = -1 }
            };
            try
            {
                NomenclatureTypeListResponse response = 
                    await nomenclatureGrpcClient.GetNomenclatureTypeListAsync(request);

                if (response.ResultStatus.Code != ResultCodes.Ok)
                {
                    logger.LogError($"Проблемен статус ({response.ResultStatus.Code}) на заявка в {nameof(NomenclatureController)}->{nameof(GetAllNomenclaturesTypes)}");
                }
                return Json(response.Data);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка в {nameof(NomenclatureController)}->{nameof(GetAllNomenclaturesTypes)}");
                return new JsonResult(null);
            }
        }

        /// <summary>
        /// Връща списък от всички стойности за номенклатурен тип
        /// </summary>
        /// <returns></returns>
        [HttpGet("nomenclature-values")]
        public async Task<JsonResult> GetNomenclatureValues(string nomenclatureType)
        {
            CodeableConceptListRequest request = new CodeableConceptListRequest
            {
                DataTableRequest = new DatatableRequest { Length = -1 },
                Type = nomenclatureType
            };
            try
            {
                CodeableConceptListResponse response =
                    await nomenclatureGrpcClient.GetCodeableConceptListAsync(request);

                if (response.ResultStatus.Code != ResultCodes.Ok)
                {
                    logger.LogError($"Проблемен статус ({response.ResultStatus.Code}) на заявка в {nameof(NomenclatureController)}->{nameof(GetNomenclatureValues)}");
                }
                return Json(response.Data);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка в {nameof(NomenclatureController)}->{nameof(GetNomenclatureValues)}");
                return new JsonResult(null);
            }
        }
    }
}
