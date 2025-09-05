using EDelivery.Constants;
using EDelivery.Contracts;
using EDelivery.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.Integration.Clients
{
    public partial class TargetGroupsClient
    {
        private readonly IMemoryCache cache;
        private readonly ILogger<MessagesClient> logger;
        private readonly ITokenService tokenService;
        public TargetGroupsClient(
           IHttpClientFactory clientFactory,
           IOptionsMonitor<EDeliveryOptions> optionsAccessor,
           IMemoryCache cache,
           ITokenService tokenService,
           ILogger<MessagesClient> logger)
        {
            _httpClient = clientFactory.CreateClient("edeliveryClient");
            this.cache = cache;
            this.logger = logger;
            this.tokenService = tokenService;
            ReadResponseAsString = true;
        }

        partial void PrepareRequest(System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, string url)
        {
            var token = tokenService.GetToken().Result;
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        partial void ProcessResponse(HttpClient client, HttpResponseMessage response)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                cache.Remove(CashConstants.Token);
            }
        }
    }
}
