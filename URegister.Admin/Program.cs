using DataTables.AspNet.AspNetCore;
using URegister.NomenclaturesCatalog;
using URegister.ObjectsCatalog;
using URegister.RegistersCatalog;
using URegister.Users;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllersWithViews();


builder.Services.AddGrpcClient<NomenclatureGrpc.NomenclatureGrpcClient>(o =>
{
    o.Address = new ("https://uregister-nomenclaturescatalog");
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

builder.Services.AddGrpcClient<AppUserManager.AppUserManagerClient>(o =>
{
    o.Address = new("https://uregister-users");
    o.ChannelOptionsActions.Add(item => item.MaxReceiveMessageSize = 15000000);
});


builder.Services.RegisterDataTables();

builder.Services.AddApplicationServices();

builder.Services.AddObjectStore(builder.Configuration);

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
    //pattern: "{controller=Designer}/{action=Index}/{id?}");

app.Run();
