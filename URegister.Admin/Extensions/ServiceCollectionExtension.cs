using Microsoft.AspNetCore.Mvc.DataAnnotations;
using URegister.Core.Validation;
using URegister.Core.Contracts;
using URegister.Core.Services;
using URegister.Infrastructure.Contracts;
using URegister.Infrastructure.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IHttpRequester, HttpRequester>();
            services.AddSingleton<IValidationAttributeAdapterProvider, URAttributeAdapterProvider>();
            services.AddSingleton<IFormValidationService, FormValidationService>();
            services.AddSingleton<IFormFieldsLayoutService, FormFieldsLayoutService>();

            services.AddSingleton<INomenclatureClientService, NomenclatureClientService>();
            services.AddSingleton<IRegisterClientService, RegisterClientService>();
            return services;
        }
    }
}
