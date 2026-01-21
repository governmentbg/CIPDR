using Microsoft.AspNetCore.Mvc;
using URegister.Common;
using URegister.RegistersCatalog;

namespace URegister.ApiGateway.Controllers
{
    public class ProcessController : BaseController
    {
        private readonly ILogger<ProcessController> _logger;
        private readonly IHttpClientFactory _httpFactory;
        private readonly RegistersCatalogGrpc.RegistersCatalogGrpcClient _registerGrpcClient;
        private readonly IConfiguration configuration;

        public ProcessController(ILogger<ProcessController> logger, 
            IHttpClientFactory httpFactory,
            RegistersCatalogGrpc.RegistersCatalogGrpcClient registerGrpcClient,
            IConfiguration configuration)
        {
            _logger = logger;
            _httpFactory = httpFactory;
            _registerGrpcClient = registerGrpcClient;
            this.configuration = configuration;
        }

        /// <summary>
        /// Връща списък със заявени услуги за текущо логнатия потребител
        /// </summary>
        /// <param name="roleInProcessType"></param>
        /// <param name="skip">Колко записа да бъдат пропуснати</param>
        /// <param name="take">Колко записа да бъдат взети</param>
        /// <returns></returns>
        [HttpGet("get-processes")]
        public async Task<IActionResult> GetProcessList(int roleInProcessType, int skip, int take, int registerId)
        {
            var client = _httpFactory.CreateClient();

            var register = await _registerGrpcClient.GetRegisterAsync(new GetRegisterRequest
            {
                RegisterId = registerId
            });

            if (register.Status.Code != ResultCodes.Ok)
            {
                return StatusCode(500, new { Message = register.Status.Message });
            }

            client.BaseAddress = new Uri(register.Data.BaseAddress);
          
            // Define query string parameters
            var parameters = new Dictionary<string, string>
            {
                { "roleInProcessType", roleInProcessType.ToString() },
                { "skip", skip.ToString() },
                { "take", take.ToString() }
            };

            // Build the query string
            var queryString = new FormUrlEncodedContent(parameters).ReadAsStringAsync().Result;

            var endpoint = $"Process/get-processes?{queryString}";

            try
            {
                HttpResponseMessage response = await client.GetAsync(endpoint);
                response.EnsureSuccessStatusCode(); // Throw if not a success code.

                string responseBody = await response.Content.ReadAsStringAsync();

                // Return the response as a JSON result
                return Content(responseBody, "application/json");
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, $"Проблем със заявката: {nameof(GetProcessList)}");
                // Return an error response
                return StatusCode(500, new { Message = "Грешка по време на обработка на заявката." });
            }
        }

        /// <summary>
        /// Връща списък със завършени заявени услуги за собственик на партида, за потребител, който не е логнат
        /// </summary>
        /// <param name="pid">Идентификатор на лице, при непосочен връща всички</param>
        /// <param name="serviceId">Идентификатор на услугата, за която се извличат данни</param>
        /// <param name="skip">Колко записа да бъдат пропуснати</param>
        /// <param name="take">Колко записа да бъдат взети</param>
        /// <returns></returns>
        [HttpGet("get-process-list-for-master-person-record")]
        public async Task<IActionResult> GetProcessListForMasterPersonRecord(string? pid, int serviceId, int skip, int take, int registerId)
        {
            var client = _httpFactory.CreateClient();

            var register = await _registerGrpcClient.GetRegisterAsync(new GetRegisterRequest
            {
                RegisterId = registerId
            });

            if (register.Status.Code != ResultCodes.Ok)
            {
                return StatusCode(500, new { Message = register.Status.Message });
            }

            client.BaseAddress = new Uri(register.Data.BaseAddress);

            var parameters = new Dictionary<string, string>
            {
                { "pid", pid.ToString() },
                { "serviceId", serviceId.ToString() },
                { "skip", skip.ToString() },
                { "take", take.ToString() }
            };

            // Build the query string
            var queryString = new FormUrlEncodedContent(parameters).ReadAsStringAsync().Result;

            var endpoint = $"Process/get-process-list-for-master-person-record?{queryString}";

            try
            {
                HttpResponseMessage response = await client.GetAsync(endpoint);
                response.EnsureSuccessStatusCode(); // Throw if not a success code.

                string responseBody = await response.Content.ReadAsStringAsync();

                // Return the response as a JSON result
                return Content(responseBody, "application/json");
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, $"Проблем със заявката: {nameof(GetProcessListForMasterPersonRecord)}");
                // Return an error response
                return StatusCode(500, new { Message = "Грешка по време на обработка на заявката." });
            }
        }

        /// <summary>
        /// Връща модел на форма от записани данни
        /// </summary>
        /// <param name="processId">Идентификатор на заявена услуга</param>
        /// <param name="registerId">Идентификатор на регистър</param>
        /// <returns></returns>
        [HttpGet("get-form-model-for-saved-data")]
        public async Task<IActionResult> GetFormModelForSavedData(Guid processId, int registerId)
        {
            var client = _httpFactory.CreateClient();

            var register = await _registerGrpcClient.GetRegisterAsync(new GetRegisterRequest
            {
                RegisterId = registerId
            });

            if (register.Status.Code != ResultCodes.Ok)
            {
                return StatusCode(500, new { Message = register.Status.Message });
            }

            client.BaseAddress = new Uri(register.Data.BaseAddress);

            // Define query string parameters
            var parameters = new Dictionary<string, string>
            {
                { "processId", processId.ToString() }
            };

            // Build the query string
            var queryString = new FormUrlEncodedContent(parameters).ReadAsStringAsync().Result;

            var endpoint = $"Process/get-form-model-for-saved-data?{queryString}";

            try
            {
                HttpResponseMessage response = await client.GetAsync(endpoint);
                response.EnsureSuccessStatusCode(); // Throw if not a success code.

                string responseBody = await response.Content.ReadAsStringAsync();

                // Return the response as a JSON result
                return Content(responseBody, "application/json");
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, $"Проблем със заявката: {nameof(GetFormModelForSavedData)}");
                // Return an error response
                return StatusCode(500, new { Message = "Грешка по време на обработка на заявката." });
            }
        }

        /// <summary>
        /// Връща всички въведени от потребителят данни за дадена форма
        /// </summary>
        /// <param name="processId">Идентификатор на заявена услуга</param>
        /// <returns></returns>
        [HttpGet("form-data-submitted-by-person")]
        public async Task<IActionResult> GetFormData(Guid processId, int registerId) 
        {
            var client = _httpFactory.CreateClient();

            var register = await _registerGrpcClient.GetRegisterAsync(new GetRegisterRequest
            {
                RegisterId = registerId
            });

            if (register.Status.Code != ResultCodes.Ok)
            {
                return StatusCode(500, new { Message = register.Status.Message });
            }

            client.BaseAddress = new Uri(register.Data.BaseAddress);

            // Define query string parameters
            var parameters = new Dictionary<string, string>
            {
                { "processId", processId.ToString() }
            };

            // Build the query string
            var queryString = new FormUrlEncodedContent(parameters).ReadAsStringAsync().Result;

            var endpoint = $"Process/form-data-submitted-by-person?{queryString}";

            try
            {
                HttpResponseMessage response = await client.GetAsync(endpoint);
                response.EnsureSuccessStatusCode(); // Throw if not a success code.

                string responseBody = await response.Content.ReadAsStringAsync();

                // Return the response as a JSON result
                return Content(responseBody, "application/json");
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, $"Проблем със заявката: {nameof(GetFormData)}");
                // Return an error response
                return StatusCode(500, new { Message = "Грешка по време на обработка на заявката." });
            }
        }

        /// <summary>
        /// Връща списък с приключени заявени услуги от тип Вписване
        /// </summary>
        /// <param name="administrationId">Идентификатор на администрация</param>
        /// <param name="perPage">Колко записа са на страница</param>
        /// <param name="pageNumber">Номер на страницата, почващ от 1</param>
        /// <param name="registerId">Идентификатор на регистъра</param>
        /// <param name="fromDate">От дата на вписване, включително</param>
        /// <param name="toDate">До дата на вписване, включително</param>
        /// <param name="toSearchDate">До дата за търсене по критерии от тип дата</param>
        /// <param name="fromSearchDate">От дата за търсене по критерии от тип дата</param>
        /// <param name="searchKey">Критерии за търсене</param>
        /// <param name="searchPattern">Низ за търсене</param>
        /// <returns></returns>
        [HttpGet("get-registration-processes")]
        //[Authorize]
        public async Task<IActionResult> GetRegistrationProcessList(Guid administrationId, 
            int perPage, 
            int pageNumber, 
            int registerId,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            DateTime? toSearchDate = null,
            DateTime? fromSearchDate = null,
            string searchKey = "", 
            string searchPattern = "")
        {
            _logger.LogInformation(
                string.Format("Method: {0}, Parameters: administrationId={1}, perPage={2}, pageNumber={3}, registerId={4}, fromDate={5}, toDate={6}, searchKey={7}, searchPattern={8}, searchToDate={9}, searchFromDate={10}",
                nameof(GetRegistrationProcessList),
                administrationId,
                perPage,
                pageNumber,
                registerId,
                fromDate?.ToString("o") ?? "null",
                toDate?.ToString("o") ?? "null",
                searchKey,
                searchPattern,
                toSearchDate?.ToString("o") ?? "null",
                fromSearchDate?.ToString("o") ?? "null"));

            var client = _httpFactory.CreateClient();

            var register = await _registerGrpcClient.GetRegisterAsync(new GetRegisterRequest
            {
                RegisterId = registerId
            });

            if (register.Status.Code != ResultCodes.Ok)
            {
                return StatusCode(500, new { Message = register.Status.Message });
            }

            client.BaseAddress = new Uri(register.Data.BaseAddress);
            if (!string.IsNullOrEmpty(configuration.GetValue<string>("RegisterUrlDebug")))
            {
                client.BaseAddress = new Uri(configuration.GetValue<string>("RegisterUrlDebug"));
            }
            // Define query string parameters
            var parameters = new Dictionary<string, string>
            {
                { nameof(administrationId), administrationId.ToString()},
                { "skip", (perPage * (pageNumber - 1)).ToString() },
                { "take", perPage.ToString() },
                { nameof(searchKey), searchKey },
                { nameof(searchPattern), searchPattern },
            };

            if (fromDate.HasValue)
            {
                parameters.Add(nameof(fromDate) , fromDate.Value.ToString("o")); // ISO 8601 format
            }

            if (toDate.HasValue)
            {
                parameters.Add(nameof(toDate), toDate.Value.ToString("o")); // ISO 8601 format
            }

            if (fromSearchDate.HasValue)
            {
                parameters.Add(nameof(fromSearchDate), fromSearchDate.Value.ToString("o")); // ISO 8601 format
            }

            if (toSearchDate.HasValue)
            {
                parameters.Add(nameof(toSearchDate), toSearchDate.Value.ToString("o")); // ISO 8601 format
            }

            var queryString = new FormUrlEncodedContent(parameters).ReadAsStringAsync().Result;

            var endpoint = $"Process/get-registration-processes?{queryString}";

            _logger.LogInformation($"Извикване на ендпойнт: {endpoint}");

            try
            {
                HttpResponseMessage response = await client.GetAsync(endpoint);
                response.EnsureSuccessStatusCode(); // Throw if not a success code.

                string responseBody = await response.Content.ReadAsStringAsync();

                // Return the response as a JSON result
                return Content(responseBody, "application/json");
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, $"Проблем със заявката: {nameof(GetRegistrationProcessList)}");
                // Return an error response
                return StatusCode(500, new { Message = "Грешка по време на обработка на заявката." });
            }
        }

        /// <summary>
        /// Връща историята на заявена услуга
        /// </summary>
        /// <param name="processId">Идентификатор на процес</param>
        /// <param name="registerId">Идентификатор на регистъра</param>
        /// <returns></returns>
        [HttpGet("get-processes-history")]
        //[Authorize]
        public async Task<IActionResult> GetProcessHistory(
            Guid processId,
            int registerId)
        {
            var client = _httpFactory.CreateClient();

            var register = await _registerGrpcClient.GetRegisterAsync(new GetRegisterRequest
            {
                RegisterId = registerId
            });

            if (register.Status.Code != ResultCodes.Ok)
            {
                return StatusCode(500, new { Message = register.Status.Message });
            }

            client.BaseAddress = new Uri(register.Data.BaseAddress);

            // Define query string parameters
            var parameters = new Dictionary<string, string>
            {
                { nameof(processId), processId.ToString()}
            };

            var queryString = new FormUrlEncodedContent(parameters).ReadAsStringAsync().Result;

            var endpoint = $"Process/get-processes-history?{queryString}";

            try
            {
                HttpResponseMessage response = await client.GetAsync(endpoint);
                response.EnsureSuccessStatusCode(); // Throw if not a success code.

                string responseBody = await response.Content.ReadAsStringAsync();

                // Return the response as a JSON result
                return Content(responseBody, "application/json");
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, $"Проблем със заявката: {nameof(GetProcessHistory)}");
                // Return an error response
                return StatusCode(500, new { Message = "Грешка по време на обработка на заявката." });
            }
        }
    }
}
