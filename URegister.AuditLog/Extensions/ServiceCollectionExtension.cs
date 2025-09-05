using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using URegister.AuditLog.Contracts;
using URegister.AuditLog.Data;
using URegister.AuditLog.Services;
using URegister.NumberGenerator.Data;


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
            services.AddScoped<IAuditLogInfoService, AuditLogInfoService>();

            return services;
        }

        /// <summary>
        /// Добавяне на поддръжка за база данни
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddDbSupport(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<AuditLogDbContext>(options =>
            {
                options.UseNpgsql(config.GetConnectionString("AuditLogConnection"))
                    .UseSnakeCaseNamingConvention();
            });

            services.AddScoped<IAuditLogRepository, AuditLogRepository>();

            return services;
        }
    }
}
