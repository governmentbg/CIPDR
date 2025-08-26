using Microsoft.AspNetCore.Cors;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using URegister.Infrastructure.Constants;
using URegister.NomenclaturesCatalog;
using URegister.RegistersCatalog;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Prevent Unicode escaping for non-ASCII characters
        options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
    });

//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddGrpcClient<RegistersCatalogGrpc.RegistersCatalogGrpcClient>(o =>
{
    o.Address = new(string.Format(builder.Configuration[ContainerNameConstants.ContainerAddressMask],
        ContainerNameConstants.RegistersCatalog));
    o.ChannelOptionsActions.Add(item => item.MaxReceiveMessageSize = 15000000);
});

builder.Services.AddGrpcClient<NomenclatureGrpc.NomenclatureGrpcClient>(o =>
{
    o.Address = new(string.Format(builder.Configuration[ContainerNameConstants.ContainerAddressMask], ContainerNameConstants.NomenclaturesCatalog));
    o.ChannelOptionsActions.Add(item => item.MaxReceiveMessageSize = 15000000);
});

var app = builder.Build();


// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors(cp => cp
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseAuthorization();
//app.MapDefaultEndpoints();

app.MapControllers();

app.Run();
