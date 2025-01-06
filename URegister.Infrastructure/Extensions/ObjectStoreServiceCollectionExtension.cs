using Amazon.S3;
using Microsoft.Extensions.Configuration;
using URegister.Infrastructure.Contracts;
using URegister.Infrastructure.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ObjectStoreServiceCollectionExtension
    {
        public static void AddObjectStore(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
            services.AddAWSService<IAmazonS3>();
            services.AddScoped<IObjectStoreService, ObjectStoreService>();
        }
    }
}
