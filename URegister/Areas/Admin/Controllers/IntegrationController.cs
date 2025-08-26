using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using URegister.Common;
using URegister.Core.Contracts;
using URegister.Infrastructure.Constants;
using URegister.Infrastucture.Extensions;
using URegister.IntegrationsCatalog;
using URegister.Core.Services;

namespace URegister.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Editor},{UserRoles.Registrator}")]
    [Display(Name = "Интеграции")]
    public class IntegrationController : BaseController
    {
        private readonly IntegrationGrpc.IntegrationGrpcClient _integrationGrpcClient;
        private readonly ILogger<IntegrationController> _logger;
        private readonly IRegixReportService _regixReportService;

        public IntegrationController(IntegrationGrpc.IntegrationGrpcClient integrationGrpcClient,
            ILogger<IntegrationController> logger,
            IRegixReportService regixReportService)
        {
            _integrationGrpcClient = integrationGrpcClient;
            _logger = logger;
            _regixReportService = regixReportService;
        }

        [HttpGet]
        [Display(Name = "Извличане на данни за лице")]
        public async Task<JsonResult> GetPersonData(PidTypes pidType, string pid)
        {
            try
            {
                var validationResult = PidValidateService.ValidatePersonalId(pid, (int)pidType);

                if (!validationResult)
                {
                    return new JsonResult(new
                        { success = false, message = $"'{pid}' e невалиден {pidType.GetDescription()}" });
                }

                GetPersonInfoRequest request = new GetPersonInfoRequest()
                {
                    Pid = pid,
                    ContextInfo = GetRegexContextInfo(GetUserRoles())
                };

                GetPersonInfoResponse response = await _integrationGrpcClient.GetPersonInfoAsync(request);

                if (response.ResultStatus.Code != ResultCodes.Ok)
                {
                    _logger.LogError($"Не може да се извлекат данни за лице в {nameof(GetPersonData)}. {response.ResultStatus.Message}");
                    return new JsonResult(new
                    {
                        success = false,
                        //message = response.ResultStatus.Message,
                        message = "Проблем при извличане на данни за лице",
                    });
                }

                await _regixReportService.CreateRegixReport(
                    JsonSerializer.Serialize(request),
                    JsonSerializer.Serialize(response),
                    ((int)RegixRequestTypes.DataRequestForPerson).ToString());

                return new JsonResult(new
                {
                    success = true,
                    firstName = response.FirstName,
                    middleName = response.MiddleName,
                    lastName = response.LastName,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Проблем при извличане на данни за лице в {nameof(GetPersonData)}");
                return new JsonResult(new
                {
                    success = false,
                    message = "Проблем при извличане на данни за лице"
                });
            }
        }

        private IEnumerable<string> GetUserRoles()
        {
            var roles = typeof(UserRoles)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Select(f => new
                {
                    Role = f.GetValue(null).ToString(),
                    Description = f.GetCustomAttribute<DescriptionAttribute>()?.Description
                })
                .ToList();

            return roles.Where(r => User.IsInRole(r.Role)).Select(r => r.Role).ToList();
        }

        [HttpGet]
        [Display(Name = "Извличане на данни за компания")]
        public async Task<JsonResult> GetCompanyData(CidTypes cidType, string cid)
        {
            var validationResult = PidValidateService.ValidateCompanyId(cid, (int)cidType);

            if (!validationResult)
            {
                return new JsonResult(new { success = false, message = $"'{cid}' e невалиден {cidType.GetDescription()}" });
            }

            GetCompanyInfoRequest request = new GetCompanyInfoRequest()
            {
                Cid = cid,
                CidType = (int)cidType,
                ContextInfo = GetRegexContextInfo(GetUserRoles())
            };

            GetCompanyInfoResponse response = await _integrationGrpcClient.GetCompanyInfoAsync(request);

            if (response.ResultStatus.Code != ResultCodes.Ok)
            {
                _logger.LogError($"Не може да се извлекат данни за компания в {nameof(GetCompanyData)} {response.ResultStatus.Message}");
                return new JsonResult(new
                {
                    success = false,
                    //message = response.ResultStatus.Message,
                    message = "Проблем при извличане на данни за компания",
                });
            }

            await _regixReportService.CreateRegixReport(
                JsonSerializer.Serialize(request),
                JsonSerializer.Serialize(response),
                ((int)RegixRequestTypes.DataRequestForCompany).ToString());

            return new JsonResult(new
            {
                success = true,
                companyName = response.Name,
                legalFormCode = response.LegalFormCode,
                legalFormName = response.LegalFormName,
                apartmentNumber = response.ApartmentNumber,
                buildingNumber = response.BuildingNumber,
                countryCode = response.CountryCode,
                countryName = response.CountryName,
                entranceName = response.EntranceName,
                floorNumber = response.FloorNumber,
                foreignAddress = response.ForeignAddress,
                postCode = response.PostCode,
                regionCode = response.RegionCode,
                regionName = response.RegionName,
                settlementCode = response.SettlementCode,
                settlementName = response.SettlementName,
                streetName = response.StreetName,
                streetNumber = response.StreetNumber
            });
        }
    }
}
