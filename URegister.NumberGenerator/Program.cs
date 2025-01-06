using URegister.NumberGenerator.Services;

var builder = WebApplication.CreateBuilder(args);

// Добавяне на общи настройки на приложението
builder.AddServiceDefaults();

// Добавяне на услуги на приложението
builder.Services.AddApplicationServices();

// Добавяне на поддръжка за база данни
builder.Services.AddDbSupport(builder.Configuration);

// Добавяне на gRPC услуги
builder.Services.AddGrpc();

var app = builder.Build();

app.MapDefaultEndpoints();

// Конфигуриране на pipeline
app.MapGrpcService<NumberService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

await app.RunAsync();
