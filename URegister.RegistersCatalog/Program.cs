using URegister.AuditLog;
using URegister.Infrastructure.Filters;
using URegister.NomenclaturesCatalog;
using URegister.RegistersCatalog.Services;
using URegister.Infrastructure.Extensions;
using URegister.Infrastructure.Constants;
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Добавяне на услуги на приложението
builder.Services.AddApplicationServices();
builder.Services.AddObjectStore(builder.Configuration);

builder.Services.AddGrpcClient<NomenclatureGrpc.NomenclatureGrpcClient>(o =>
{
    o.Address = new(string.Format(builder.Configuration[ContainerNameConstants.ContainerAddressMask], ContainerNameConstants.NomenclaturesCatalog));
})
.AddCallCredentialsGrpc();

builder.Services.AddGrpcClient<AuditLogGrpc.AuditLogGrpcClient>(o =>
{
    o.Address = new(string.Format(builder.Configuration[ContainerNameConstants.ContainerAddressMask], ContainerNameConstants.AuditLog));
});
// Добавяне на поддръжка за база данни
builder.Services.AddDbSupport(builder.Configuration);

// Add services to the container.
builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<ServerAuditLogInterceptor>();
}); ;

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
app.MapGrpcService<RegisterService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
