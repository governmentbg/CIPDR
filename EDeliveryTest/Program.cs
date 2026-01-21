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
//var messages = await edClient.GetMessageList(100, 1000);
//Console.WriteLine(messages);
var id = await edClient.GetProfileId("831641791", "1");

//var tokenService = provider.GetRequiredService<ITokenService>();
//var targetGroupsClient = provider.GetRequiredService<ITargetGroupsClient>();
//var miscinfo = await tokenService.GetMiscinfo();
//var targetGroups = await targetGroupsClient.ListAsync(miscinfo);

//var id = await edClient.GetProfileId("201701209", "1");
// var id = await edClient.GetProfileId("123123123","2");
//var fileName = "11 Тони.pdf";
string? fileName = null;
//var fileData = File.ReadAllBytes(@"d:\tmp\" + fileName);
var fileData = new byte[0];
var mesg = @"След успешно постъпил електронен документ и генериране на входящ номер от системата, същият да се насочва с писмо към профила на заявителя в ССЕВ със следното съдържание в поле Съдържание" +
           @"След успешно постъпил електронен документ и генериране на входящ номер от системата, същият да се насочва с писмо към профила на заявителя в ССЕВ със следното съдържание в поле Съдържание" +
           @"След успешно постъпил електронен документ и генериране на входящ номер от системата, същият да се насочва с писмо към профила на заявителя в ССЕВ със следното съдържание в поле Съдържание";
//Console.WriteLine(mesg.Length);
await edClient.SendMessage(
    id ?? 0, 1,
    "Съобщение",
    mesg, null, fileName, fileData);

//var outmessages = await edClient.GetOutMessageList(null, null);

//var messages = await edClient.GetMessageList(null, null);
//foreach (var message in outmessages)
//{
//    var open = await edClient.ViewMessage(message.MessageId);
//    //Console.WriteLine($"{open.MessageId} {open.}");
//  //  (var blobId, var fileName, var fileData) = await edClient.DownLoadApplicationFile(open);
//  //  File.WriteAllBytes(@"d:\tmp\" + fileName, fileData);
//}
/* var client = provider.GetRequiredService<IMessagesClient>();
var blobsClient = provider.GetRequiredService<IBlobsClient>();
var oboBlobsClient = provider.GetRequiredService<IOboBlobsClient>();
var templatesClient = provider.GetRequiredService<ITemplatesClient>();
var profilesClient = provider.GetRequiredService<IProfilesClient>();
var tokenService = provider.GetRequiredService<ITokenService>();
var uploadService = provider.GetRequiredService<IFileService>();
var targetGroupsClient = provider.GetRequiredService<ITargetGroupsClient>();
var miscinfo = await tokenService.GetMiscinfo();
var targetGroups = await targetGroupsClient.ListAsync(miscinfo);
var profile = await profilesClient.SearchAsync(miscinfo, "7201306926", null, 1);

var templates = await templatesClient.ListAsync(miscinfo, 0, 2000);
var template = await templatesClient.DetailsAsync( miscinfo, 1);
var list = await client.GetInboxAsync(miscinfo, 0, 20);

var fileName = "edelivery_esb_api_0.6.pdf";
var fileData = await File.ReadAllBytesAsync(fileName);
var r = await uploadService.UploadFile(fileName, fileData);
Console.WriteLine(r);
//var msg = list.Result.First();
//var view = await client.OpenAsync(miscinfo, msg.MessageId);
var view = await client.OpenAsync(miscinfo, 995992);
var val = view.Fields.Values;
var fileInfo = view.Fields.Values.Last().ToString();
Console.WriteLine(fileInfo);
var arr = JsonSerializer.Deserialize<BlobDO[]>(fileInfo);

var blobs = await blobsClient.ListAsync(miscinfo, 0, 111);
//var oboblobs = await oboBlobsClient.ListOnBehalfAsync(miscinfo, 0, 111);
var blob = await blobsClient.DetailsAsync(miscinfo, 10067863);
//var oboBlob = await oboBlobsClient.DetailsOnBehalfAsync(miscinfo, 10067819);
var addr = arr.First().DownloadLink;
var file = await uploadService.DownLoadFile(addr);
File.WriteAllBytes(@"D:\1\d.test", file);
Console.WriteLine(fileInfo); */
