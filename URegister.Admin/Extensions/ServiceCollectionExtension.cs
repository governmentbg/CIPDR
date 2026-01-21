using Grpc.Core;
using IdStampITAuthentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Security.Claims;
using URegister.Core.Contracts;
using URegister.Core.Identity;
using URegister.Core.Services;
using URegister.Core.Validation;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Contracts;
using URegister.Infrastructure.Models;
using URegister.Infrastructure.Services;
using OpenDataClient.Extensions;
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IHttpRequester, HttpRequester>();
            services.AddSingleton<IValidationAttributeAdapterProvider, URAttributeAdapterProvider>();
            services.AddScoped<IFormValidationService, FormValidationService>();
            services.AddScoped<IFormFieldsLayoutService, FormFieldsLayoutService>();
            services.AddScoped<IAuditLogServiceClient, AuditLogServiceClient>();
            services.AddScoped<ICalendarService, CalendarService>();
            services.AddScoped<IAuditInfo>(x => new AuditInfo()
            {
                TypeAuditTask = TypeAuditTask.Repository,
                ProjectName = "RegisterAdmin"
            });
            services.AddScoped<INomenclatureClientService, NomenclatureClientService>();
            services.AddScoped<IRegisterClientService, RegisterClientService>();
            services.AddScoped<IUserContext, UserContext>();
            return services;
        }

        public static IServiceCollection AddApplicationIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            int cookieMaxAgeMinutes = configuration.GetValue<int>("Authentication:CookieMaxAgeMinutes");
            services.AddAuthentication(x =>
            {
                x.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                x.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            }).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.LoginPath = "/account/login";
                options.LogoutPath = "/account/logoff";
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(cookieMaxAgeMinutes);
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            })
            .AddCookie(IdentityConstants.ExternalScheme, o =>
            {
                o.Cookie.Name = IdentityConstants.ExternalScheme;
                o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            })
            .AddStampIT(options =>
            {
                options.AppId = configuration.GetValue<string>("Authentication:StampIT:AppId");
                options.AppSecret = configuration.GetValue<string>("Authentication:StampIT:AppSecret");
                options.Scope.Add("pid");
                options.ClaimActions.DeleteClaim(ClaimTypes.NameIdentifier);
                options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "pid");
                options.ClaimActions.MapJsonKey(CustomClaimType.IdStampit.CertificateNumber, "certno");
                options.AuthorizationEndpoint = StampITIdDefaults.AuthorizationEndpoint;
                options.TokenEndpoint = StampITIdDefaults.TokenEndpoint;
                options.UserInformationEndpoint = StampITIdDefaults.UserInformationEndpoint;
                options.Events = new OAuthEvents()
                {
                    OnRemoteFailure = context => HandleRemoteFailure(context)
                };
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(cookieMaxAgeMinutes);
                options.LoginPath = "/account/login";
                options.LogoutPath = "/account/logoff";
            });
            services.ConfigureOpenDataClient(configuration);
            return services;
        }

        static Task HandleRemoteFailure(RemoteFailureContext context)
        {
            context.Response.Redirect($"/account/logincerterror?error={context.Failure}");
            context.HandleResponse();

            return Task.FromResult(0);
        }

        public static IHttpClientBuilder AddCallCredentialsAdmin(this IHttpClientBuilder builder)
        {
            return builder.AddCallCredentials((context, metadata, serviceProvider) =>
             {
                 var userContext = serviceProvider.GetRequiredService<IUserContext>();
                 metadata.Add("UserId", userContext.UserId.ToString());
                 metadata.Add("AdministrationId", userContext.AdministrationId.ToString());
                 metadata.Add("RegisterId", "0");
                 var auditInfo = (IAuditInfo?)serviceProvider.GetService(typeof(IAuditInfo));
                 metadata.Add("ActivityId", auditInfo?.ActivityId ?? string.Empty);
                 metadata.Add("UserFullName", auditInfo?.UserFullNameBase64 ?? string.Empty); 
                 return Task.CompletedTask;
             });
        }
    }
}
