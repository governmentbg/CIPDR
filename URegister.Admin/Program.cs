using DataTables.AspNet.AspNetCore;
using URegister.Admin.ModelBinders;
using URegister.AuditLog;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Data.Common;
using URegister.Infrastructure.Filters;
using URegister.IntegrationsCatalog;
using URegister.NomenclaturesCatalog;
using URegister.ObjectsCatalog;
using URegister.RegistersCatalog;
using URegister.Users;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAntiforgery(x => {
    x.HeaderName = "X-CSRF-TOKEN";
});

builder.AddServiceDefaults();

builder.Services.AddControllersWithViews().AddMvcOptions(config =>
{
    config.Filters.Add<AuditLogFilter<IRepository>>();
    config.MaxModelBindingCollectionSize = 50000;
    config.ModelBinderProviders.Insert(0, new DecimalModelBinderProvider());
    config.ModelBinderProviders.Insert(1, new DoubleModelBinderProvider());
    config.ModelBinderProviders.Insert(2, new DateTimeModelBinderProvider(FormattingConstant.NormalDateFormat));
    config.ModelBinderProviders.Insert(3, new DateOnlyModelBinderProvider(FormattingConstant.NormalDateFormat));
});


builder.Services.AddGrpcClient<NomenclatureGrpc.NomenclatureGrpcClient>(o =>
{
    o.Address = new(string.Format(builder.Configuration[ContainerNameConstants.ContainerAddressMask], ContainerNameConstants.NomenclaturesCatalog));
    o.ChannelOptionsActions.Add(item => item.MaxReceiveMessageSize = 15000000);

})
.AddCallCredentialsAdmin();

builder.Services.AddGrpcClient<ObjectsCatalogGrpc.ObjectsCatalogGrpcClient>(o =>
{
    o.Address = new(string.Format(builder.Configuration[ContainerNameConstants.ContainerAddressMask], ContainerNameConstants.ObjectsCatalog));
    o.ChannelOptionsActions.Add(item => item.MaxReceiveMessageSize = 15000000);
})
.AddCallCredentialsAdmin();

builder.Services.AddGrpcClient<RegistersCatalogGrpc.RegistersCatalogGrpcClient>(o =>
    {
        o.Address = new(string.Format(builder.Configuration[ContainerNameConstants.ContainerAddressMask],
            ContainerNameConstants.RegistersCatalog));
    o.ChannelOptionsActions.Add(item => item.MaxReceiveMessageSize = 15000000);
})
.AddCallCredentialsAdmin();

builder.Services.AddGrpcClient<AppUserManager.AppUserManagerClient>(o =>
{
    o.Address = new(string.Format(builder.Configuration[ContainerNameConstants.ContainerAddressMask], ContainerNameConstants.UsersCatalog));
    o.ChannelOptionsActions.Add(item => item.MaxReceiveMessageSize = 15000000);
})
.AddCallCredentialsAdmin();
builder.Services.AddGrpcClient<IntegrationGrpc.IntegrationGrpcClient>(o =>
{
    o.Address = new(string.Format(builder.Configuration[ContainerNameConstants.ContainerAddressMask], ContainerNameConstants.IntegrationsCatalog));
    o.ChannelOptionsActions.Add(item => item.MaxReceiveMessageSize = 15000000);
})
.AddCallCredentialsAdmin();

builder.Services.AddGrpcClient<AuditLogGrpc.AuditLogGrpcClient>(o =>
{
    o.Address = new(string.Format(builder.Configuration[ContainerNameConstants.ContainerAddressMask], ContainerNameConstants.AuditLog));
});

builder.Services.RegisterDataTables();

builder.Services.AddApplicationServices();

builder.Services.AddApplicationIdentity(builder.Configuration);

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
