using Microsoft.EntityFrameworkCore;
using URegister.Infrastructure.Contracts;
using URegister.Infrastructure.Services;
using URegister.NomenclaturesCatalog.Contracts;
using URegister.NomenclaturesCatalog.Services;
using URegister.RegistersCatalog.Data;

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
            services.AddScoped<INomenclatureInfoService, NomenclatureService>();
            services.AddScoped<IHttpRequester, HttpRequester>();
            services.AddHttpClient();
            return services;
        }

        /// <summary>
        /// Добавяне на поддръжка за база данни
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddDbSupport(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<NomenclaturesCatalogDbContext>(options =>
            {
                options.UseNpgsql(config.GetConnectionString("NomenclaturesCatalogConnection"))
                    .UseSnakeCaseNamingConvention();
            });

            services.AddScoped<INomenclaturesCatalogRepository, NomenclaturesCatalogRepository>();

            return services;
        }
    }
}
