using EDelivery.Contracts;
using EDelivery.Integration;
using EDelivery.Integration.Clients;
using EDelivery.Integration.Contracts;
using EDelivery.Model;
using EDelivery.Service;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;

namespace EDelivery.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureEDeliveryClient(this IServiceCollection services, EDeliveryOptions options)
        {
            Action<EDeliveryOptions> actionOptions = option => {
                option.EDeliveryUrl = options.EDeliveryUrl;
                option.TokenUrl = options.TokenUrl;
                option.CertPath = options.CertPath;
                option.CertPass = options.CertPass;
                option.ClientId = options.ClientId;
            };
            services.Configure(actionOptions);
            services.AddMemoryCache();
            services.AddHttpClient("tokenClient", client =>
            {
                var endPoint = options.TokenUrl;
                client.BaseAddress = new Uri(endPoint);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                var certificatePath = options.CertPath;
                var certificatePassword = options.CertPass;
                HttpClientHandler result = new HttpClientHandler();
                if (!string.IsNullOrEmpty(certificatePath))
                {
                    var _cert = new X509Certificate2(certificatePath, certificatePassword);
                    result.ClientCertificates.Add(_cert);
                }
                return result;
            });

            services.AddHttpClient("edeliveryClient", client =>
            {
                var endPoint = options.EDeliveryUrl;
                client.BaseAddress = new Uri(endPoint);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                var certificatePath = options.CertPath;
                var certificatePassword = options.CertPass;
                HttpClientHandler result = new HttpClientHandler();
                if (!string.IsNullOrEmpty(certificatePath))
                {
                    var _cert = new X509Certificate2(certificatePath, certificatePassword);
                    result.ClientCertificates.Add(_cert);
                }
                return result;
            });

            services.AddHttpClient("uploadClient", client =>
            {
                var endPoint = options.EDeliveryUrl;
                client.BaseAddress = new Uri(endPoint);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                var certificatePath = options.CertPath;
                var certificatePassword = options.CertPass;
                HttpClientHandler result = new HttpClientHandler();
                if (!string.IsNullOrEmpty(certificatePath))
                {
                    var _cert = new X509Certificate2(certificatePath, certificatePassword);
                    result.ClientCertificates.Add(_cert);
                }
                return result;
            });
            services.AddHttpClient("downLoadClient");

            services.AddScoped<ITokenService, TokenService>()
                    .AddScoped<IFileService, FileService>()
                    .AddScoped<IMessagesClient, MessagesClient>()
                    .AddScoped<IBlobsClient, BlobsClient>()
                    .AddScoped<IOboBlobsClient, OboBlobsClient>()
                    .AddScoped<ITemplatesClient, TemplatesClient>()
                    .AddScoped<IProfilesClient, ProfilesClient>()
                    .AddScoped<ITargetGroupsClient, TargetGroupsClient>()
                    .AddScoped<IEDeliveryClientService, EDeliveryClientService>();
        }
    }
}
