using EDelivery.Contracts;
using EDelivery.Extensions;
using EDelivery.Integration;
using EDelivery.Integration.Clients;
using EDelivery.Integration.Contracts;
using EDelivery.Model;
using EDelivery.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

var configuration = new ConfigurationBuilder()
              .SetBasePath(Path.Combine(AppContext.BaseDirectory))
              .AddJsonFile("appsettings.json", optional: false)
              .Build();

var services = new ServiceCollection()
                   .AddLogging()
                   .AddSingleton<IConfiguration>(configuration);
services.ConfigureEDeliveryClient(new EDeliveryOptions{
    CertPath = configuration.GetValue<string>("EDelivery:Certificate") ?? string.Empty,
    CertPass = configuration.GetValue<string>("EDelivery:Password") ?? string.Empty,
    ClientId = configuration.GetValue<string>("EDelivery:ClientId") ?? string.Empty,
    EDeliveryUrl = configuration.GetValue<string>("EDelivery:EDeliveryUrl") ?? string.Empty,
    TokenUrl = configuration.GetValue<string>("EDelivery:TokenUrl") ?? string.Empty,
});

var provider = services.BuildServiceProvider();
var edClient = provider.GetRequiredService<IEDeliveryClientService>();
var messages = await edClient.GetMessageList(100, 1000);
Console.WriteLine(messages);
