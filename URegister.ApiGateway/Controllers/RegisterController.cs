using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using URegister.Common;
using URegister.Infrastructure.Constants;
using URegister.RegistersCatalog;

namespace URegister.ApiGateway.Controllers
{
    public class RegisterController(RegistersCatalogGrpc.RegistersCatalogGrpcClient registerGrpcClient,
        ILogger<RegisterController> logger)
        : BaseController
    {
        /// <summary>
        /// Връща списък от всички активни регистри
        /// </summary>
        /// <returns></returns>
        [HttpGet("all-registries")]
        public async Task<JsonResult> GetAllRegistries()
        {
            RegisterListRequest request = new RegisterListRequest
            {
                DataTableRequest = new DatatableRequest { Length = -1 },
                IsActive = true,
                DeployedOnly = true,
                Type = RegisterType.Public,
                StatusId = (int)RegisterStatusType.Register
            };
            try
            {
                RegisterFullListResponse response = await registerGrpcClient.GetRegisterFullListAsync(request);
                if (response.Status.Code != ResultCodes.Ok)
                {
                    logger.LogError($"Проблемен статус ({response.Status.Code}) на заявка в {nameof(RegisterController)}->{nameof(GetAllRegistries)}");
                }
                return Json(response.Data);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка в {nameof(RegisterController)}->{nameof(GetAllRegistries)}");
                return new JsonResult(null);
            }
        }

        /// <summary>
        /// Връща списък от всички администрации за регистър
        /// </summary>
        /// <param name="registerId">Идентификатор на регистъра</param>
        /// <returns></returns>
        [HttpGet("{registerId}/administrations")]
        public async Task<JsonResult> GetAdministrations(int registerId)
        {
            AdministrationListRequest request = new AdministrationListRequest
            {
                DataTableRequest = new DatatableRequest { Length = -1 },
                RegisterId = registerId
            };
            try
            {
                AdministrationListResponse response = await registerGrpcClient.GetAdministrationListAsync(request);
                if (response.Status.Code != ResultCodes.Ok)
                {
                    logger.LogError($"Проблемен статус ({response.Status.Code}) на заявка в {nameof(RegisterController)}->{nameof(GetAdministrations)} за регистър с идентификатор {registerId}");
                }
                return Json(response.Data);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Грешка в {nameof(RegisterController)}->{nameof(GetAdministrations)} за регистър с идентификатор {registerId}");
                return new JsonResult(null);
            }
        }
    }
}
