using Microsoft.EntityFrameworkCore;
using URegister.NumberGenerator.Contracts;
using URegister.NumberGenerator.Data;
using URegister.NumberGenerator.Services;

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
            services.AddScoped<INumberGeneratorService, NumberGeneratorService>();

            return services;
        }

        /// <summary>
        /// Добавяне на поддръжка за база данни
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddDbSupport(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<NumberGeneratorDbContext>(options =>
            {
                options.UseNpgsql(config.GetConnectionString("NumberGeneratorConnection"))
                    .UseSnakeCaseNamingConvention();
            });

            services.AddScoped<INumberGeneratorRepository, NumberGeneratorRepository>();

            return services;
        }
    }
}
