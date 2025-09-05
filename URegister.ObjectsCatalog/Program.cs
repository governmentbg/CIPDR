using URegister.AuditLog;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Filters;
using URegister.ObjectsCatalog.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddDbSupport(builder.Configuration);

builder.Services.AddApplicationServices();

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

// Configure the HTTP request pipeline.
app.MapGrpcService<ObjectCatalogService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
