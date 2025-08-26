using DataTables.AspNet.AspNetCore;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using URegister.Core.Data;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Filters;
using URegister.IntegrationsCatalog;
using URegister.ModelBinders;
using URegister.NomenclaturesCatalog;
using URegister.NumberGenerator;
using URegister.ObjectsCatalog;
using URegister.RegistersCatalog;
using URegister.Users;

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("bg-BG");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("bg-BG");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAntiforgery(x => {
    x.HeaderName = "X-CSRF-TOKEN";
});


// За добавяне на контексти, използвайте extension метода
builder.Services.AddApplicationDbContext(builder.Configuration);

//// За конфигуриране на Identity, използвайте extension метода
builder.Services.AddApplicationIdentityAdmin(builder.Configuration);

builder.Services.AddGrpcClient<NomenclatureGrpc.NomenclatureGrpcClient>(o =>
{
    o.Address = new(string.Format(builder.Configuration[ContainerNameConstants.ContainerAddressMask], ContainerNameConstants.NomenclaturesCatalog));
    o.ChannelOptionsActions.Add(item => item.MaxReceiveMessageSize = 15000000);
})
.AddCallCredentialsRegister();

builder.Services.AddGrpcClient<ObjectsCatalogGrpc.ObjectsCatalogGrpcClient>(o =>
{
    o.Address = new(string.Format(builder.Configuration[ContainerNameConstants.ContainerAddressMask], ContainerNameConstants.ObjectsCatalog));
    o.ChannelOptionsActions.Add(item => item.MaxReceiveMessageSize = 15000000);
})
.AddCallCredentialsRegister(); 

builder.Services.AddGrpcClient<RegistersCatalogGrpc.RegistersCatalogGrpcClient>(o =>
{
    o.Address = new(string.Format(builder.Configuration[ContainerNameConstants.ContainerAddressMask], ContainerNameConstants.RegistersCatalog));
    o.ChannelOptionsActions.Add(item => item.MaxReceiveMessageSize = 15000000);
})
.AddCallCredentialsRegister();

builder.Services.AddGrpcClient<NumberGenerator.NumberGeneratorClient>(o =>
{
    o.Address = new(string.Format(builder.Configuration[ContainerNameConstants.ContainerAddressMask], ContainerNameConstants.NumberGenerator));
    o.ChannelOptionsActions.Add(item => item.MaxReceiveMessageSize = 15000000);
})
.AddCallCredentialsRegister();

builder.Services.AddGrpcClient<IntegrationGrpc.IntegrationGrpcClient>(o =>
{
    o.Address = new(string.Format(builder.Configuration[ContainerNameConstants.ContainerAddressMask], ContainerNameConstants.IntegrationsCatalog));
    o.ChannelOptionsActions.Add(item => item.MaxReceiveMessageSize = 15000000);
})
.AddCallCredentialsRegister();

builder.Services.AddGrpcClient<AppUserManager.AppUserManagerClient>(o =>
{
    o.Address = new(string.Format(builder.Configuration[ContainerNameConstants.ContainerAddressMask], ContainerNameConstants.UsersCatalog));
    o.ChannelOptionsActions.Add(item => item.MaxReceiveMessageSize = 15000000);
})
.AddCallCredentialsRegister();

builder.Services.RegisterDataTables();

builder.Services.AddObjectStore(builder.Configuration);

// За добавяне на услуги, използвайте extension метода
builder.Services.AddApplicationServices(builder.Configuration);

builder.AddServiceDefaults();

// Add services to the container.
//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(connectionString));
//builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews()
   .AddMvcOptions(config =>
{
    config.Filters.Add<AuditLogFilter<IApplicationRepository>>();
    config.MaxModelBindingCollectionSize = 50000;
    config.ModelBinderProviders.Insert(0, new DecimalModelBinderProvider());
    config.ModelBinderProviders.Insert(1, new DoubleModelBinderProvider());
    config.ModelBinderProviders.Insert(2, new DateTimeModelBinderProvider(FormattingConstant.NormalDateFormat));
    config.ModelBinderProviders.Insert(3, new DateOnlyModelBinderProvider(FormattingConstant.NormalDateFormat));
})
.AddJsonOptions(options =>
{
    // Prevent Unicode escaping for non-ASCII characters
    options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
}); ;

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "My API",
        Version = "v1"
    });

    // Include controllers only in a specific Area (e.g., "Public")
    options.DocInclusionPredicate((docName, apiDesc) =>
    {
        var area = apiDesc.ActionDescriptor.RouteValues.TryGetValue("area", out var areaName) ? areaName : null;
        return area == "Public"; // Replace "Public" with your Area name
    });

    // To exclude controllers in a specific Area, reverse the logic:
    // return area != "Public";
});

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{area}/{controller}/{action}/{id?}",
    defaults: new { area = "", controller = "Home", action = "Index" });

app.MapControllerRoute(
    name: "areas",
    pattern: "/{controller=Home}/{action=Index}/{id?}");

app.Run();
