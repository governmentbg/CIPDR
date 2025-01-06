using DataTables.AspNet.AspNetCore;
using System.Globalization;
using URegister.NomenclaturesCatalog;
using URegister.NumberGenerator;
using URegister.ObjectsCatalog;
using URegister.RegistersCatalog;

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("bg-BG");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("bg-BG");

var builder = WebApplication.CreateBuilder(args);

// За добавяне на контексти, използвайте extension метода
builder.Services.AddApplicationDbContext(builder.Configuration);

//// За конфигуриране на Identity, използвайте extension метода
//builder.Services.AddApplicationIdentity(builder.Configuration);

builder.Services.AddGrpcClient<NomenclatureGrpc.NomenclatureGrpcClient>(o =>
{
    o.Address = new("https://uregister-nomenclaturescatalog");
    o.ChannelOptionsActions.Add(item => item.MaxReceiveMessageSize = 15000000);
});

builder.Services.AddGrpcClient<ObjectsCatalogGrpc.ObjectsCatalogGrpcClient>(o =>
{
    o.Address = new("https://uregister-objectscatalog");
    o.ChannelOptionsActions.Add(item => item.MaxReceiveMessageSize = 15000000);
});

builder.Services.AddGrpcClient<RegistersCatalogGrpc.RegistersCatalogGrpcClient>(o =>
{
    o.Address = new("https://uregister-registerscatalog");
    o.ChannelOptionsActions.Add(item => item.MaxReceiveMessageSize = 15000000);
});
builder.Services.AddGrpcClient<NumberGenerator.NumberGeneratorClient>(o =>
{
    o.Address = new("https://uregister-numbergenerator");
    o.ChannelOptionsActions.Add(item => item.MaxReceiveMessageSize = 15000000);
});


builder.Services.RegisterDataTables();

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
builder.Services.AddControllersWithViews();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
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
