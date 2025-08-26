using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenDataClient;
using OpenDataClient.Extensions;

var configuration = new ConfigurationBuilder()
              .SetBasePath(Path.Combine(AppContext.BaseDirectory))
              .AddJsonFile("appsettings.json", optional: false)
              .Build();

var services = new ServiceCollection()
                   .AddLogging()
                   .AddSingleton<IConfiguration>(configuration);
services.ConfigureOpenDataClient(configuration);
var provider = services.BuildServiceProvider();
var client = provider.GetRequiredService<IOpenDataClientService>();
var uri = await client.AddDatasetAsync(39, "Тест 4", "Test 4", 7, 1);
Console.WriteLine(uri.ToString());
var uri2 = await client.AddResourceAsync(uri, "Тест 3", "Test 3", [["Име", "Поле"], ["1", "2"]]);

Console.WriteLine(uri2.ToString());

