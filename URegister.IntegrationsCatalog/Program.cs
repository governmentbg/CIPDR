using IO.RegixClient;
using Microsoft.Extensions.Configuration;
using URegister.Infrastructure.Constants;
using URegister.IntegrationsCatalog.Services;
using URegister.RegistersCatalog;
using URegister.Infrastructure.Extensions;
using URegister.NomenclaturesCatalog;
using URegister.Users;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddDbSupport(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);
// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcClient<RegistersCatalogGrpc.RegistersCatalogGrpcClient>(o =>
{
    o.Address = new(string.Format(builder.Configuration[ContainerNameConstants.ContainerAddressMask],
        ContainerNameConstants.RegistersCatalog));
    o.ChannelOptionsActions.Add(item => item.MaxReceiveMessageSize = 15000000);
})
.AddCallCredentialsGrpc();

builder.Services.AddGrpcClient<AppUserManager.AppUserManagerClient>(o =>
{
    o.Address = new(string.Format(builder.Configuration[ContainerNameConstants.ContainerAddressMask], ContainerNameConstants.UsersCatalog));
    o.ChannelOptionsActions.Add(item => item.MaxReceiveMessageSize = 15000000);
})
.AddCallCredentialsGrpc();

builder.Services.AddGrpcClient<NomenclatureGrpc.NomenclatureGrpcClient>(o =>
    {
        o.Address = new(string.Format(builder.Configuration[ContainerNameConstants.ContainerAddressMask], ContainerNameConstants.NomenclaturesCatalog));
        o.ChannelOptionsActions.Add(item => item.MaxReceiveMessageSize = 15000000);
    })
    .AddCallCredentialsGrpc();

builder.Services.AddIoRegixClient(options =>
{
    options.CertificatePath = builder.Configuration.GetValue<string>("Regix:Certificate");
    options.Password = builder.Configuration.GetValue<string>("Regix:Password");
    options.ClientType = builder.Configuration.GetValue<bool>("Regix:IsInProduction") ? ClientType.Production : ClientType.Test;
    options.UseNewEndpoint = builder.Configuration.GetValue<bool>("Regix:UseNewEndpoint", true);
});

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
app.MapGrpcService<IntegrationCatalogService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
