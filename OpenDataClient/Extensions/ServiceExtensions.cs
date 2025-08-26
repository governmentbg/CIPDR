using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenDataClient.Models;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;

namespace OpenDataClient.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureOpenDataClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<OpenDataOptions>(configuration.GetSection("OpenData"));
            services.AddHttpClient("openDataClient");
            services.AddScoped<IOpenDataClientService, OpenDataClientService>();
        }
    }
}
