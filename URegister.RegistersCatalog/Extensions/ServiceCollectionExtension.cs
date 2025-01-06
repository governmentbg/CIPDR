using Microsoft.EntityFrameworkCore;
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
