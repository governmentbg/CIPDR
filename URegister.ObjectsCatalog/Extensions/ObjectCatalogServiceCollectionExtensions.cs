using Microsoft.EntityFrameworkCore;
using Npgsql;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Contracts;
using URegister.Infrastructure.Models;
using URegister.Infrastructure.Services;
using URegister.ObjectsCatalog.Contracts;
using URegister.ObjectsCatalog.Data;
using URegister.ObjectsCatalog.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ObjectCatalogServiceCollectionExtensions
    {
        /// <summary>
        /// Добавяне на услуги на приложението
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IObjectService, ObjectService>();
            services.AddScoped<IAuditLogServiceClient, AuditLogServiceClient>();
            services.AddScoped<IAuditInfo>(x => new AuditInfo()
            {
                TypeAuditTask = TypeAuditTask.GrpcClient,
                ProjectName = "ObjectCatalog"
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
            var dataSource = new NpgsqlDataSourceBuilder(config.GetConnectionString("ObjectCatalogConnection"))
                .EnableDynamicJson()//Добавено за да работят jsonb полета от колекции във EF 8. За EF 6 няма нужда
                .Build();

            services.AddDbContext<ObjectCatalogDbContext>(options =>
            {
                options.UseNpgsql(dataSource)
                    .UseSnakeCaseNamingConvention();
            });

            services.AddScoped<IObjectCatalogRepository, ObjectCatalogRepository>();

            return services;
        }
    }
}
