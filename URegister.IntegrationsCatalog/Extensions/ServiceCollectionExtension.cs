using DataTables.AspNet.AspNetCore;
using EDelivery.Extensions;
using EDelivery.Integration;
using EDelivery.Model;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System.Net.Http.Headers;
using System.Net.Mail;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Contracts;
using URegister.Infrastructure.Models;
using URegister.Infrastructure.Services;
using URegister.IntegrationsCatalog.Contracts;
using URegister.IntegrationsCatalog.Data;
using URegister.IntegrationsCatalog.Jobs;
using URegister.IntegrationsCatalog.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Методи за добавяне на услуги в контейнера на услуги
    /// </summary>
    public static class ServiceCollectionExtension
    {
        /// <summary>
        /// Добавяне на поддръжка за база данни
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddDbSupport(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<IntegrationsCatalogDbContext>(options =>
            {
                options.UseNpgsql(config.GetConnectionString("IntegrationsCatalogConnection"))
                    .UseSnakeCaseNamingConvention();
            });
            return services;
        }
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddQuartz(q =>
            {
                q.SchedulerId = Guid.NewGuid().ToString();
                q.SchedulerName = "IOScheduler";
                q.UseSimpleTypeLoader();
                q.UseDefaultThreadPool(tp =>
                {
                    tp.MaxConcurrency = 5;
                });
                q.UseInMemoryStore();


                q.UseXmlSchedulingConfiguration(x =>
                {
                    x.Files = new[] { "~/quartz_jobs.xml" };
                    x.ScanInterval = TimeSpan.FromMinutes(1);
                    x.FailOnFileNotFound = true;
                    x.FailOnSchedulingError = true;
                });
            });

            services.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = true;
            });

            services.AddTransient<EDeliveryReceiveJob>();
            services.AddTransient<EDeliverySendJob>();
            services.ConfigureEDeliveryClient(new EDeliveryOptions
            {
                CertPath = config.GetValue<string>("EDelivery:Certificate") ?? string.Empty,
                CertPass = config.GetValue<string>("EDelivery:Password") ?? string.Empty,
                ClientId = config.GetValue<string>("EDelivery:ClientId") ?? string.Empty,
                EDeliveryUrl = config.GetValue<string>("EDelivery:EDeliveryUrl") ?? string.Empty,
                TokenUrl = config.GetValue<string>("EDelivery:TokenUrl") ?? string.Empty,
            });

            services.AddHttpClient("apiGatewayClient", client =>
            {
                var endPoint = config.GetValue<string>("ApiGatewayUrl");
                client.BaseAddress = new Uri(endPoint!);
            });
            services.AddHttpClient("objectStoreClient");
            services.AddScoped<IIntegrationsCatalogRepository, IntegrationsCatalogRepository>();
            services.AddScoped<IEDeliveryService, EDeliveryService>();
            services.AddScoped<IHttpRequester, HttpRequester>();

            services.AddScoped<SmtpClient>((serviceProvider) =>
            {
                var config = serviceProvider.GetRequiredService<IConfiguration>();
                return new SmtpClient()
                {
                    Host = config.GetValue<String>("Email:Smtp:Host"),
                    Port = config.GetValue<int>("Email:Smtp:Port")
                    //Credentials = new NetworkCredential(
                    //                     config.GetValue<String>("Email:Smtp:Username"),
                    //                        config.GetValue<String>("Email:Smtp:Password")
                    //                    )
                };
            });
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IEMailService, EMailService>();
            services.AddObjectStore(config);

            return services;
        }
    }
}
