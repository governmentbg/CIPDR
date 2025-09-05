using EAuthIntegration.Models;
using IdStampITAuthentication;
using IO.HtmlToPdf.Models;
using IO.SignTools.Extensions;
using IO.SignTools.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using System.IdentityModel.Claims;
using URegister.Core.Contracts;
using URegister.Core.Data;
using URegister.Core.Identity;
using URegister.Core.Services;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Contracts;
using URegister.Infrastructure.Data.Common;
using URegister.Infrastructure.Models;
using URegister.Infrastructure.Services;
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
        services.AddHttpClient("objectStoreClient");
        services.AddScoped<IFormFieldsLayoutService, FormFieldsLayoutService>();
        services.AddScoped<IFormValidationService, FormValidationService>();
        services.AddScoped<IFormConfigurationPersistenceService, FormConfigurationPersistenceService>();
        services.AddScoped<IRegisterService, RegisterService>();
        services.AddScoped<IServiceService, ServiceService>();
        services.AddScoped<IProcessService, ProcessService>();
        services.AddScoped<INomenclatureClientService, NomenclatureClientService>();
        services.AddScoped<IRegisterClientService, RegisterClientService>();
        services.AddScoped<IImportService, ImportService>();
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IHttpRequester, HttpRequester>();
        services.AddScoped<IRegixReportService, RegixReportService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IDeadlineService, DeadlineService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IPublicFieldTemplateService, PublicFieldTemplateService>();
        services.AddScoped<IProcessTemplateService, ProcessTemplateService>();
        services.AddIOHtmlToPdf(options =>
        {
            options.PdfCreatorUrl = config.GetValue<string>("PdfCreator:Url");
            options.IgnoreSSLErrors = config.GetValue<bool>("PdfCreator:IgnoreSSLErrors");
            options.PdfOptions = new PDFOptions() { Timeout = 0 };
            options.RequestTimeout = TimeSpan.FromMinutes(15);
        });
        TimestampClientOptions tsOptions = new TimestampClientOptions()
        {
            Token = config.GetValue<string>("Signer:Token"),
            TimestampEndpoint = config.GetValue<string>("Signer:TimestampUrl"),
        };
        services.AddIOSignTools(options =>
        {
            options.HashAlgorithm = System.Security.Cryptography.HashAlgorithmName.SHA256.Name;
            options.TimestampOptions = tsOptions;
        });
        return services;
    }

    public static IServiceCollection AddApplicationDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        var dataSource = new NpgsqlDataSourceBuilder(connectionString)
            .EnableDynamicJson()//Добавено за да работят jsonb полета от колекции във EF 8. За EF 6 няма нужда
            .Build();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(dataSource)
            .UseSnakeCaseNamingConvention());
        services.AddScoped<IAuditInfo>(x => new AuditInfo()
        {
            TypeAuditTask = TypeAuditTask.Repository,
            ProjectName = "Register"
        });
        //services.AddScoped<IAuditLogServiceClient, AuditLogServiceClient>();
        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        services.AddScoped<IRepository, ApplicationRepository>();
        services.AddDataProtection().PersistKeysToDbContext<ApplicationDbContext>();
        services.AddDatabaseDeveloperPageExceptionFilter();

        return services;
    }

    public static IServiceCollection AddApplicationIdentityAdmin(this IServiceCollection services, IConfiguration configuration)
    {
        int cookieMaxAgeMinutes = configuration.GetValue<int>("Authentication:CookieMaxAgeMinutes");
        services.AddAuthentication(x =>
        {
            x.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            x.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options => 
        {
            options.LoginPath = "/admin/account/login";
            options.LogoutPath = "/admin/account/logoff";
            options.AccessDeniedPath = "/admin/account/accessDenied";
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
            options.LoginPath = "/admin/account/login";
            options.LogoutPath = "/admin/account/logoff";
            options.AccessDeniedPath = "/admin/account/accessDenied";
        });

        return services;
    }

    static Task HandleRemoteFailure(RemoteFailureContext context)
    {
        context.Response.Redirect($"/admin/account/logincerterror?error={context.Failure}");
        context.HandleResponse();

        return Task.FromResult(0);
    }

    public static IHttpClientBuilder AddCallCredentialsRegister(this IHttpClientBuilder builder)
    {
        return builder.AddCallCredentials((context, metadata, serviceProvider) =>
        {
            var userContext = serviceProvider.GetRequiredService<IUserContext>();
            var registerService = serviceProvider.GetRequiredService<IRegisterService>();
            metadata.Add("UserId", userContext.UserId.ToString());
            metadata.Add("AdministrationId", userContext.AdministrationId.ToString());
            metadata.Add("RegisterId", registerService.GetCurrentRegisterIdForAudit().ToString());
            var auditInfo = (IAuditInfo?)serviceProvider.GetService(typeof(IAuditInfo));
            metadata.Add("ActivityId", auditInfo?.ActivityId ?? string.Empty);
            metadata.Add("UserFullName", auditInfo?.UserFullNameBase64 ?? string.Empty);
            return Task.CompletedTask;
        });
    }
}
