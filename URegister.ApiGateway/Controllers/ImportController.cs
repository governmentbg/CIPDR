using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using URegister.Common;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Extensions;
using URegister.Infrastructure.Model.EDelivery;
using URegister.RegistersCatalog;

namespace URegister.ApiGateway.Controllers
{
    public class ImportController : BaseController
    {
        private readonly ILogger<ProcessController> _logger;
        private readonly IHttpClientFactory _httpFactory;
        private readonly RegistersCatalogGrpc.RegistersCatalogGrpcClient _registerGrpcClient;
        private readonly IConfiguration _configuration;

        public ImportController(ILogger<ProcessController> logger,
            IHttpClientFactory httpFactory,
            RegistersCatalogGrpc.RegistersCatalogGrpcClient registerGrpcClient,
            IConfiguration configuration)
        {
            _logger = logger;
            _httpFactory = httpFactory;
            _registerGrpcClient = registerGrpcClient;
            _configuration = configuration;
        }

        /// <summary>
        /// Импорт на данни за заявена услуга през json от .pdf файл
        /// </summary>
        /// <param name="registerCode">Код на регистър във формат RXXXX</param>
        /// <param name="jsonFromFile">json данни на заявена услуга.</param>
        [HttpPost("import-json")]
        public async Task<IActionResult> ImportJson([FromBody] ImportJsonVM model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.JsonFromFile))
                {
                    return BadRequest("Празен json");
                }

                if (string.IsNullOrWhiteSpace(model.RegisterCode))
                {
                    return BadRequest("Не е подаден код на регистър.");
                }

                
                var client = _httpFactory.CreateClient();

                var register = await _registerGrpcClient.GetRegisterByRegisterCodeAsync(new GetRegisterByCodeRequest
                {
                    RegisterCode = model.RegisterCode
                });

                if (register.Status.Code != ResultCodes.Ok)
                {
                    return StatusCode(500, new { Message = register.Status.Message });
                }

                if (string.IsNullOrWhiteSpace(register.Data.BaseAddress))
                {
                    return BadRequest("Избраният регистър няма посочен URL в базата данни");
                }
                client.BaseAddress = new Uri(register.Data.BaseAddress);
                var registerUrlDebug = _configuration.GetValue<string>("RegisterUrlDebug");
                if (!string.IsNullOrEmpty(registerUrlDebug))
                {
                    client.BaseAddress = new Uri(registerUrlDebug);
                }

                var endpoint = $"Import/import-json";

                var content = new StringContent(model.ToJson(), System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(endpoint, content);

                string responseMessage = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var importResult = responseMessage.FromJson<ImportResultVM>();
                    return  Ok(importResult);
                }
                return StatusCode((int)response.StatusCode, responseMessage);
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, $"Проблем със заявката: {nameof(ImportApplication)}");
                return StatusCode(500, new { Message = "Грешка по време на обработка на заявката. Проверете лога" });
            }
        }

        /// <summary>
        /// Импорт на данни за заявена услуга през json от .pdf файл
        /// </summary>
        /// <param name="registerCode">Код на регистър във формат RXXXX</param>
        /// <param name="file">Pdf файл с json данни на заявена услуга.</param>
        /// <param name="attachedFileData">Инфромация за качените фалове. Речник с ключ името, и стойност идентификатор на файл от Storage-а</param>
        [HttpPost("{registerCode}/import-application")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportApplication(string registerCode,
            IFormFile file, string attachedFileData = null)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Не е прикачен файл със съдържание.");
                }

                if (string.IsNullOrWhiteSpace(registerCode))
                {
                    return BadRequest("Не е подаден код на регистър.");
                }

                registerCode = registerCode.Trim().ToUpper();

                if (!Regex.IsMatch(registerCode, RegexPatterns.RegisterNumber))
                {
                    return BadRequest("Грешен формат на код на регистър. Очакван формат R#####");
                }

                if (file.ContentType != "application/pdf")
                {
                    return BadRequest("Само PDF са позволени.");
                }

                var client = _httpFactory.CreateClient();

                var register = await _registerGrpcClient.GetRegisterByRegisterCodeAsync(new GetRegisterByCodeRequest
                {
                    RegisterCode = registerCode
                });

                if (register.Status.Code != ResultCodes.Ok)
                {
                    return StatusCode(500, new { Message = register.Status.Message });
                }

                if (string.IsNullOrWhiteSpace(register.Data.BaseAddress))
                {
                    return BadRequest("Избраният регистър няма посочен URL в базата данни");
                }
                client.BaseAddress = new Uri(register.Data.BaseAddress);

                var endpoint = $"Import/import-application";
            
                using var content = new MultipartFormDataContent();
                using var fileStream = file.OpenReadStream();
                content.Add(new StreamContent(fileStream), "file", file.FileName);

                if (!string.IsNullOrEmpty(attachedFileData))
                {
                    content.Add(new StringContent(attachedFileData, Encoding.UTF8), "attachedFileDataJson");
                }

                HttpResponseMessage response = await client.PostAsync(endpoint, content);
                string responseMessage = await response.Content.ReadAsStringAsync();

                return StatusCode((int)response.StatusCode, responseMessage);
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, $"Проблем със заявката: {nameof(ImportApplication)}");
                return StatusCode(500, new { Message = "Грешка по време на обработка на заявката. Проверете лога" });
            }
        }

        [HttpPost("import-edelivery-file")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public async Task<IActionResult> ImportEDeliveryFile(EDeliveryMessageVM model)
        {
            var register = await _registerGrpcClient.GetRegisterAsync(new GetRegisterRequest
            {
                RegisterId = model.RegisterId,
            });

            if (register.Status.Code != ResultCodes.Ok)
            {
                return StatusCode(500, new { Message = register.Status.Message });
            }

            if (string.IsNullOrWhiteSpace(register.Data.BaseAddress))
            {
                return BadRequest("Избраният регистър няма посочен URL в базата данни");
            }
            var client = _httpFactory.CreateClient();
            client.BaseAddress = new Uri(register.Data.BaseAddress);

            var registerUrlDebug = _configuration.GetValue<string>("RegisterUrlDebug");
            if (!string.IsNullOrEmpty(registerUrlDebug))
            {
                client.BaseAddress = new Uri(registerUrlDebug);
            }

            var jsonData = JsonSerializer.Serialize(model);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var endpoint = $"Import/import-edelivery-file";
            HttpResponseMessage response = await client.PostAsync(endpoint, content);
            string responseMessage = await response.Content.ReadAsStringAsync();
            return StatusCode((int)response.StatusCode, responseMessage);
        }
    }
}
