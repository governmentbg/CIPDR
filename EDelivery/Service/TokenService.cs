using EDelivery.Constants;
using EDelivery.Contracts;
using EDelivery.Integration.Clients;
using EDelivery.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EDelivery.Service
{
    public class TokenService(IHttpClientFactory clientFactory,
           IOptionsMonitor<EDeliveryOptions> optionsAccessor,
           IMemoryCache cache,
           ILogger<MessagesClient> logger): ITokenService
    {
        public async Task<string?> GetToken()
        {
            string? token;
            if (cache.TryGetValue(CashConstants.Token, out token))
            {
                return token;
            }
            var clientId = optionsAccessor.CurrentValue.ClientId;
            var httpClient = clientFactory.CreateClient("tokenClient");
            var result = await httpClient.PostAsync($"token?grant_type=client_credentials&client_id={clientId}&scope=/ed2*", null);

            var contentResponse = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(contentResponse);
            token = tokenResponse?.access_token;
            cache.Set(CashConstants.Token, token, TimeSpan.FromMinutes(30));
            return token;
        }
        public async Task<string?> GetMiscinfo()
        {
            string? miscinfo = string.Empty;
            if (cache.TryGetValue(CashConstants.Miscinfo, out miscinfo))
            {
                return miscinfo;
            }
            var token = await GetToken();
            if (!string.IsNullOrEmpty(token)) {
                var httpClient = clientFactory.CreateClient("tokenClient");
                var result = await httpClient.PostAsync($"/introspect?token={token}&token_type_hint=access_token", null);
                var contentResponse = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                var introResponse = JsonSerializer.Deserialize<IntrospectResponse>(contentResponse);
                miscinfo = introResponse?.miscinfo;
                cache.Set(CashConstants.Miscinfo, miscinfo);
            }

            return miscinfo;
        }
    }
}
