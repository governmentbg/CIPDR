using URegister.AuditLog;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Filters;
using URegister.NomenclaturesCatalog;

var builder = WebApplication.CreateBuilder(args);


// Добавяне на общи настройки на приложението
builder.AddServiceDefaults();

// Добавяне на услуги на приложението
builder.Services.AddApplicationServices();

// Добавяне на поддръжка за база данни
builder.Services.AddDbSupport(builder.Configuration);

// Добавяне на gRPC услуги
builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<ServerAuditLogInterceptor>();
});

builder.Services.AddGrpcClient<AuditLogGrpc.AuditLogGrpcClient>(o =>
{
    o.Address = new(string.Format(builder.Configuration[ContainerNameConstants.ContainerAddressMask], ContainerNameConstants.AuditLog));
});


var app = builder.Build();

app.MapDefaultEndpoints();

// Конфигуриране на pipeline
app.MapGrpcService<NomenclatureCatalogService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
