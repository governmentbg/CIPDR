using Microsoft.EntityFrameworkCore;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Contracts;
using URegister.Infrastructure.Models;
using URegister.Infrastructure.Services;
using URegister.RegistersCatalog.Contracts;
using URegister.RegistersCatalog.Data;
using URegister.RegistersCatalog.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Методи за добавяне на услуги в контейнера на услуги
    /// </summary>
    public static class ServiceCollectionExtension
    {
        /// <summary>
        /// Добавяне на услуги на приложението
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IRegisterInfoService, RegisterInfoService>();
            services.AddScoped<IHttpRequester, HttpRequester>();
            services.AddScoped<IAuditLogServiceClient, AuditLogServiceClient>();
            services.AddScoped<IAuditInfo>(x => new AuditInfo() { 
                TypeAuditTask = TypeAuditTask.GrpcClient ,
                ProjectName = "RegisterCatalog"
            });
            services.AddHttpClient();
            services.AddHttpClient("stampit",client =>
            {
                client.BaseAddress = new Uri("https://id.stampit.org/");
                var cert = File.ReadAllText("stampit.bearer");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {cert}");
                client.DefaultRequestHeaders.Add("Referer", "https://id.stampit.org/");
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler
                {
                    AllowAutoRedirect = false,
                };
            });

            return services;
        }

        /// <summary>
        /// Добавяне на поддръжка за база данни
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddDbSupport(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<RegistersCatalogDbContext>(options =>
            {
                options.UseNpgsql(config.GetConnectionString("RegisterCatalogConnection"))
                    .UseSnakeCaseNamingConvention();
            });

            services.AddScoped<IRegistersCatalogRepository, RegistersCatalogRepository>();

            return services;
        }
    }
}
