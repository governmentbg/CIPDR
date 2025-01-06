using EAuthIntegration.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.IdentityModel.Claims;
using URegister.Core.Contracts;
using URegister.Core.Data;
using URegister.Core.Services;
using URegister.Infrastructure.Constants;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddHttpClient("insecureClient")
            .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    return new HttpClientHandler()
                    {
                        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; }
                    };
                });
        services.AddHttpClient();

        services.AddScoped<IFormFieldsLayoutService, FormFieldsLayoutService>();
        services.AddScoped<IFormValidationService, FormValidationService>();
        services.AddScoped<IFormConfigurationPersistenceService, FormConfigurationPersistenceService>();
        services.AddScoped<IRegisterService, RegisterService>();
        services.AddScoped<IServiceService, ServiceService>();
        services.AddScoped<IProcessService, ProcessService>();
        services.AddScoped< INomenclatureClientService, NomenclatureClientService>();
        return services;
    }

    public static IServiceCollection AddApplicationDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention());

        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        services.AddDataProtection().PersistKeysToDbContext<ApplicationDbContext>();
        services.AddDatabaseDeveloperPageExceptionFilter();

        return services;
    }

    public static IServiceCollection AddApplicationIdentityPublic(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEAuth(configuration, opt =>
        {
            opt.Issuer = configuration["EAuth:Issuer"] ?? string.Empty;
            opt.ServiceName = configuration["EAuth:ServiceName"] ?? string.Empty;
            opt.Service = configuration["EAuth:Service"] ?? string.Empty;
            opt.Provider = configuration["EAuth:Provider"] ?? string.Empty;
            opt.LevelOfAssurance = configuration["EAuth:LevelOfAssurance"] ?? string.Empty;
            opt.IdPMetadata = configuration["EAuth:IdPMetadata"] ?? string.Empty;
            opt.SigningCertificateFile = configuration["EAuth:SigningCertificateFile"] ?? string.Empty;
            opt.SigningCertificatePassword = configuration["EAuth:SigningCertificatePassword"] ?? string.Empty;
            opt.SignatureAlgorithm = configuration["EAuth:SignatureAlgorithm"] ?? string.Empty;
            opt.AdministrativeContact = new EAuthAdministrativeContact()
            {
                Company = "Ministry of health",
                GivenName = "Zornitsa",
                SurName = "Ivanova",
                EmailAddress = "zivanova@mh.government.bg"
            };
            opt.CertificateValidationMode = configuration["EAuth:CertificateValidationMode"] ?? string.Empty;
            opt.RevocationMode = configuration["EAuth:RevocationMode"] ?? string.Empty;
            opt.IgnoreCertificateValidity = configuration.GetValue<bool>("EAuth:IgnoreCertificateValidity");
            opt.TechnicalContact = new EAuthTechnicalContact()
            {
                Company = "Information Services Plc",
                GivenName = "Ivaylo",
                SurName = "Stoychev",
                EmailAddress = "i.stoychev@is-bg.net"
            };
            opt.RequestedAttributes = new EAuthRequestAttribute[]
            {
                new EAuthRequestAttribute(EAuthRequestAttributesEnum.PersonIdentifier, true),
                new EAuthRequestAttribute(EAuthRequestAttributesEnum.PersonName, true),
                new EAuthRequestAttribute(EAuthRequestAttributesEnum.X509Certificate, true),
                new EAuthRequestAttribute(EAuthRequestAttributesEnum.Email, true),
                new EAuthRequestAttribute(EAuthRequestAttributesEnum.DateOfBirth, false)
            };
            opt.CookieConfiguration = new EAuthCookieConfiguration()
            {
                ExpireTimeSpan = TimeSpan.FromMinutes(120),
                SlidingExpiration = true,
                LoginPath = new PathString("/Account/Login"),
                LogoutPath = new PathString("/Account/LogOff")
            };
        });

        return services;
    }

    public static IServiceCollection AddApplicationIdentityAdmin(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(x =>
        {
            x.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        }).AddCookie()
        .AddStampIT(options =>
            {
                options.AppId = configuration.GetValue<string>("Authentication:StampIT:AppId");
                options.AppSecret = configuration.GetValue<string>("Authentication:StampIT:AppSecret");
                options.Scope.Add("certno");
                options.Scope.Add("pid");
                options.ClaimActions.DeleteClaim(ClaimTypes.NameIdentifier);
                options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "pid");
                options.ClaimActions.MapJsonKey(CustomClaimType.IdStampit.CertificateNumber, "certno");
                options.ClaimActions.MapJsonKey(CustomClaimType.IdStampit.CertificateNumber, "certno");
                options.AuthorizationEndpoint = configuration.GetValue<string>("Authentication:StampIT:AuthorizationEndpoint");
                options.TokenEndpoint = configuration.GetValue<string>("Authentication:StampIT:TokenEndpoint");
                options.UserInformationEndpoint = configuration.GetValue<string>("Authentication:StampIT:UserInformationEndpoint");
                options.Events = new OAuthEvents()
                {
                    OnRemoteFailure = context =>
                    {
                        context.Response.Redirect($"/login/logincerterror?error={context.Failure}");
                        context.HandleResponse();

                        return Task.FromResult(0);
                    }
                };
            });
        return services;
    }

    static Task HandleRemoteFailure(RemoteFailureContext context)
    {
        context.Response.Redirect($"/account/logincerterror?error={context.Failure}");
        context.HandleResponse();

        return Task.FromResult(0);
    }
}
