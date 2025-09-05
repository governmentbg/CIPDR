using Azure;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using URegister.Core.Services;
using URegister.Infrastructure.Constants;
using URegister.Infrastucture.Extensions;

namespace URegister.Admin.Controllers
{
    [Display(Name = "Интеграции")]
    public class IntegrationController : BaseController
    {
        private readonly ILogger<IntegrationController> _logger;

        public IntegrationController(ILogger<IntegrationController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Display(Name = "Извличане на данни за лице")]
        public async Task<JsonResult> GetPersonData(PidTypes pidType, string pid)
        {
            var validationResult = PidValidateService.ValidatePersonalId(pid, (int)pidType);

            if (!validationResult)
            {
                return new JsonResult(new { success = false, message = $"'{pid}' e невалиден {pidType.GetDescription()}" });
            }
            
            //TODO : да връща истински данни
            return new JsonResult(new
            {
                success = true, 
                firstName = "Карлос",
                middleName = "Рей",
                lastName = "Норис",
            });
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

            if (cidType == CidTypes.EIK)
            {
                //TODO : да връща истински данни?
                return new JsonResult(new
                {
                    success = true,
                    companyName = "Николас Исциприс",
                    legalFormCode = "1",
                    legalFormName = "ЕТ"
                });
            }
            else
            {
                return new JsonResult(new
                {
                    success = true,
                    companyName = "Диблър ССПГ",
                    legalFormCode = "512",
                    legalFormName = "Цирк",
                    apartmentNumber = "25",
                    buildingNumber = "29",
                    countryCode = "BG",
                    entranceName = "А",
                    floorNumber = "3",
                    foreignAddress = "Папуа Нова Гвинея, център",
                    postCode = 3700,
                    regionCode = "",
                    settlementCode = 67338,
                    streetName = "31-ви Февруари"
                });
            }
        }
    }
}
