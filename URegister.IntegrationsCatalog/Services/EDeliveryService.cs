using EDelivery.Integration;
using EDelivery.Integration.Contracts;
using EDelivery.Service;
using Google.Protobuf.WellKnownTypes;
using iText.Kernel.Pdf;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using System.IO;
using System.Security.AccessControl;
using System.ServiceModel.Channels;
using System.Text.Json;
using System.Text.Json.Nodes;
using ThirdParty.BouncyCastle.Utilities.IO.Pem;
using URegister.Common;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Contracts;
using URegister.Infrastructure.Extensions;
using URegister.Infrastructure.Model.EDelivery;
using URegister.Infrastructure.Services;
using URegister.IntegrationsCatalog.Contracts;
using URegister.IntegrationsCatalog.Data;
using URegister.IntegrationsCatalog.Data.Models;
using URegister.IntegrationsCatalog.Models;
using URegister.RegistersCatalog;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Configuration;

namespace URegister.IntegrationsCatalog.Services
{

    public class EDeliveryService(
        IEDeliveryClientService edeliveryClientService,
        IObjectStoreService objectStoreService,
        RegistersCatalogGrpc.RegistersCatalogGrpcClient registerGrpcClient,
        IHttpClientFactory httpFactory,
        IHttpRequester httpRequester,
        IIntegrationsCatalogRepository repo,
        IEMailService emailService,
        IConfiguration configuration
        ) : IEDeliveryService
    {
        private async Task SetEDeliveryMessageError(EDeliveryMessage edeliveryMessage, string errMsg)
        {
            edeliveryMessage.StatusId = (int)EDeliveryStatus.Error;
            edeliveryMessage.ErrorMessage = errMsg;
            await emailService.AddEmailOnError(edeliveryMessage);
            await repo.SaveChangesAsync();
        }
        private async Task SaveInstructionResponse(EDeliveryMessage edeliveryMessage, EDeliveryMessage edeliveryMessageFrom, MessageOpenDO openMessage)
        {
            var httpClient = httpFactory.CreateClient("apiGatewayClient");
            var endpoint = $"Import/import-edelivery-file";
            edeliveryMessage.ProcessId = edeliveryMessageFrom.ProcessId;
            edeliveryMessage.SourceId = edeliveryMessageFrom.SourceId;
            edeliveryMessage.SourceType = edeliveryMessageFrom.SourceType;
            edeliveryMessage.RegisterId = edeliveryMessageFrom.RegisterId;
            edeliveryMessage.MessageTypeId = (int)EDeliveryMessageType.InstructionResponse;
            edeliveryMessage.IncomingDate = edeliveryMessageFrom.IncomingDate;
            edeliveryMessage.IncomingNumber = edeliveryMessageFrom.IncomingNumber;
            edeliveryMessage.TenantId = edeliveryMessageFrom.TenantId;
            var messageContent = await edeliveryClientService.GetMessageText(openMessage);

            var messageVM = await EdeliveryMessageToVM(edeliveryMessage, messageContent);
            HttpResponseMessage response = await httpRequester.PostAsync("apiGatewayClient", endpoint, messageVM);

            var responseMessage = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                edeliveryMessage.StatusId = (int)EDeliveryStatus.Ready;
                edeliveryMessage.MessageTypeId = (int)EDeliveryMessageType.InstructionResponse;
            }
            else
            {
                edeliveryMessage.StatusId = (int)EDeliveryStatus.Error;
                edeliveryMessage.ErrorMessage = responseMessage;
            }
            await emailService.AddEmailOnInstructionResponse(edeliveryMessage);
            await repo.SaveChangesAsync();
        }

        private async Task<List<ApplicationFileDataVM>> GetAppFileDataList(EDeliveryMessage edeliveryMessage, MessageOpenDO openMessage)
        {
            var blobs = await edeliveryClientService.GetMessageBlobs(openMessage);
            var appFileDataList = new List<ApplicationFileDataVM>();
            if (blobs != null)
            {
                foreach (var blob in blobs)
                {
                    EDeliveryFileMetadata fileMetaData;
                    if (edeliveryMessage.EDeliveryFiles.Any(x => x.BlobId == blob.BlobId))
                    {
                        fileMetaData = edeliveryMessage.EDeliveryFiles
                                                 .Where(x => x.BlobId == blob.BlobId)
                                                 .First();
                    }
                    else
                    {
                        fileMetaData = new EDeliveryFileMetadata
                        {
                            EDeliveryMessageId = edeliveryMessage.Id,
                            BlobId = blob.BlobId,
                            FileSourceTypeId = (int)EDeliveryFileType.AttachedFile,
                            FileName = blob.FileName,
                        };
                        edeliveryMessage.EDeliveryFiles.Add(fileMetaData);
                        await repo.AddAsync(fileMetaData);
                    }
                    var fileData = await edeliveryClientService.DownLoadFile(blob.DownloadLink);
                    fileMetaData.FileId = Guid.Parse(await objectStoreService.SaveObject(fileMetaData.FileName, fileData, "application/pdf", null));
                    var appFileData = ResolvePdfJson(fileData);
                    if (!string.IsNullOrEmpty(appFileData?.ServiceCode) &&
                        fileMetaData.FileName.StartsWith(appFileData!.ServiceCode!) &&
                        fileMetaData.FileName.EndsWith("-ZVLN.pdf"))
                    {
                        fileMetaData.Rnu = appFileData.Rnu;
                        fileMetaData.FileSourceTypeId = (int)EDeliveryFileType.Application;
                        appFileDataList.Add(appFileData);
                    }
                }
            }
            return appFileDataList;
        }
        public async Task ReceiveMessages()
        {
            var responseService = await registerGrpcClient.GetServiceListAsync(new Google.Protobuf.WellKnownTypes.Empty());
            var services = responseService.Data;
            var responseAdministrations = await registerGrpcClient.GetAdministrationUicListAsync(new Google.Protobuf.WellKnownTypes.Empty());
            var administrations = responseAdministrations.Data;


            var messages = await edeliveryClientService.GetMessageList(null, null);
            foreach (var message in messages)
            {
                var edeliveryMessage = await repo.All<EDeliveryMessage>()
                                                 .Include(x => x.EDeliveryFiles)
                                                 .Where(x => x.MessageId == message.MessageId)
                                                 .FirstOrDefaultAsync();
                if (edeliveryMessage?.StatusId == (int)EDeliveryStatus.Ready || edeliveryMessage?.StatusId == (int)EDeliveryStatus.Error)
                {
                    continue;
                }

                if (edeliveryMessage == null)
                {
                    edeliveryMessage = new EDeliveryMessage
                    {
                        MessageId = message.MessageId,
                        MessageTypeId = (int)EDeliveryMessageType.Other,
                        StepId = (int)EDeliveryStep.List,
                        StatusId = (int)EDeliveryStatus.InWork,
                        Rnu = message.Rnu,
                    };
                    await repo.AddAsync(edeliveryMessage);
                    await repo.SaveChangesAsync();
                }

                MessageOpenDO openMessage;

                openMessage = await edeliveryClientService.OpenMessage(message.MessageId);
                edeliveryMessage.StepId = (int)EDeliveryStep.Open;
                edeliveryMessage.Message = JsonSerializer.Serialize(openMessage);
                edeliveryMessage.Rnu = openMessage.Rnu;
                edeliveryMessage.ProfileId = openMessage.Sender.ProfileId;
                await repo.SaveChangesAsync();


                var appFileDataList = await GetAppFileDataList(edeliveryMessage, openMessage);

                EDeliveryMessage ? edeliveryMessageFrom = null;
                if (!string.IsNullOrEmpty(edeliveryMessage.Rnu))
                {
                    edeliveryMessageFrom = await repo.AllReadonly<EDeliveryMessage>()
                                                        .Where(x => x.Rnu == edeliveryMessage.Rnu)
                                                        .Where(x => EDeliveryMessageTypeConsts.EDeliveryMessageTypeOut.Contains(x.MessageTypeId))
                                                        .FirstOrDefaultAsync();
                }
                if (edeliveryMessageFrom?.MessageTypeId == (int)EDeliveryMessageType.OutInstruction)
                {
                    await SaveInstructionResponse(edeliveryMessage, edeliveryMessageFrom, openMessage);
                    continue;
                }


                if (appFileDataList.Count == 1)
                {
                    var appFileData = appFileDataList.First();

                    edeliveryMessage.StepId = (int)EDeliveryStep.File;
                    edeliveryMessage.ApplicationJson = appFileData.ApplicationJson;
                    edeliveryMessage.ApplicationSubmission = appFileData.ApplicationSubmission;
                    edeliveryMessage.AdministrationUic = appFileData.AdministrationUic;
                    edeliveryMessage.RegisterNumber = appFileData.RegisterNumber;
                    edeliveryMessage.MessageTypeId = (int)EDeliveryMessageType.Application;
                    var serviceTypeId = (int)ServiceTypes.Register;
                    if (!string.IsNullOrEmpty(appFileData.RegisterNumber))
                    {
                        serviceTypeId = (int)ServiceTypes.Change;
                        if (appFileData.RegisterType == "3")
                        {
                            serviceTypeId = (int)ServiceTypes.Deletion;
                        }
                    }
                    var service = services
                                   .Where(x => x.EformCode == appFileData.ServiceCode &&
                                               x.ServiceTypeId == serviceTypeId)
                                   .FirstOrDefault();
                    if (service == null && services.Count(x => x.EformCode == appFileData.ServiceCode) == 1)
                    {
                         service = services
                                       .Where(x => x.EformCode == appFileData.ServiceCode )
                                       .FirstOrDefault();
                        serviceTypeId = service.ServiceTypeId;
                   }
                    if (service == null && services.Count == 1)
                    {
                        service = services.First();
                    }
                    var administrationId = administrations.Where(x => x.Uic == appFileData.AdministrationUic).Select(x => x.AdministrationId).FirstOrDefault();
                    var administrationName = string.Empty;
                    if (administrationId == null && service != null)
                    {
                        var administrationsRegister = administrations.Where(x => x.RegisterIds.Any(r => r == service.RegisterId)).ToList();
                        if (administrationsRegister.Count() == 1)
                        {
                            var item = administrationsRegister.First();
                            administrationId = item.AdministrationId;
                            administrationName = item.Name;
                            edeliveryMessage.AdministrationUic = item.Uic;
                        }
                    }
                    var serviceRegisterCodes = services.Where(x => x.EformCode == appFileData.ServiceCode).Select(x => x.RegisterCode).Distinct().ToList();
                    if (serviceRegisterCodes.Count > 1)
                    {
                        await SetEDeliveryMessageError(edeliveryMessage,
                                                 $"Намирам услуга {appFileData.ServiceCode} в регистри" + string.Join(",", serviceRegisterCodes));
                        continue;
                    }

                    if (service == null || string.IsNullOrEmpty(administrationId))
                    {
                        var serviceType = serviceTypeId == (int)ServiceTypes.Register ? "първоначално вписване" : 
                                          (serviceTypeId == (int)ServiceTypes.Change ? "промяна на обстоятелства" : "заличаване"); 
                        await SetEDeliveryMessageError(edeliveryMessage,
                                                       (service == null ? $"Не намирам услуга {appFileData.ServiceCode} {serviceType}" : string.Empty) +
                                                       (string.IsNullOrEmpty(administrationId) ? $"Не намирам администрация {appFileData.AdministrationUic} " : string.Empty));
                        continue;
                    }
                    edeliveryMessage.RegisterId = service.RegisterId;
                    edeliveryMessage.ServiceId = service.ServiceId;
                    edeliveryMessage.TenantId = Guid.Parse(administrationId);
                    await repo.SaveChangesAsync();
                    if (service != null)
                    {
                        var endpoint = $"Import/import-json";
                        var model = new ImportJsonVM
                        {
                            AdministrationUic = edeliveryMessage.AdministrationUic,
                            JsonFromFile = appFileData.ApplicationJson,
                            RegisterCode = service.RegisterCode,
                            RegisterNumber = appFileData.RegisterNumber,
                            ServiceId = service.ServiceId,
                        };
                        model.EDeliveryFiles = await EDeliveryFilesToVM(edeliveryMessage.EDeliveryFiles);

                        HttpResponseMessage response = await httpRequester.PostAsync("apiGatewayClient", endpoint, model);

                        var responseMessage = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            var importResult = responseMessage.FromJson<ImportResultVM>();
                            if (importResult != null)
                            {
                                edeliveryMessage.StatusId = (int)EDeliveryStatus.Ready;
                                edeliveryMessage.ProcessId = importResult.ProcessId;
                                edeliveryMessage.IncomingDate = importResult.IncomingDate?.SetToUtc();
                                edeliveryMessage.IncomingNumber = importResult.IncomingNumber;
                                await emailService.AddEmailOnInputNumber(administrationName, edeliveryMessage);
                            }
                        }
                        else
                        {
                            await SetEDeliveryMessageError(edeliveryMessage,
                                 responseMessage);
                            continue;
                        }
                    }
                }

                if (appFileDataList.Count != 1)
                {
                    edeliveryMessage.StepId = (int)EDeliveryStep.File;
                    await SetEDeliveryMessageError(edeliveryMessage,
                        $"В полученото съобщение има {appFileDataList.Count} заявления");
                    await emailService.AddEmailOnError(edeliveryMessage);
                }
                await repo.SaveChangesAsync();
            }
        }

        private async Task<EDeliveryMessageVM> EdeliveryMessageToVM(EDeliveryMessage edeliveryMessage, string? messageContent)
        {
            return new EDeliveryMessageVM
            {
                Id = edeliveryMessage.Id,
                ProcessId = edeliveryMessage.ProcessId,
                SourceId = edeliveryMessage.SourceId,
                SourceType = edeliveryMessage.SourceType,
                RegisterId = edeliveryMessage.RegisterId ?? 0,
                MessageTypeId = edeliveryMessage.MessageTypeId,
                Content = messageContent,
                EDeliveryFiles = await EDeliveryFilesToVM(edeliveryMessage.EDeliveryFiles)
            };
        }
        private async Task<List<EDeliveryFileVM>> EDeliveryFilesToVM(ICollection<EDeliveryFileMetadata> eDeliveryFiles)
        {
            var result = new List<EDeliveryFileVM>();
            foreach (var eDeliveryFile in eDeliveryFiles)
            {
                var eDeliveryFileVM = new EDeliveryFileVM
                {
                    Id = eDeliveryFile.Id,
                    FileName = eDeliveryFile.FileName,
                    FileSourceTypeId = eDeliveryFile.FileSourceTypeId,
                };
                eDeliveryFileVM.FileUrl = await objectStoreService.GetPresignedUrl((eDeliveryFile.FileId ?? Guid.Empty).ToString());
                result.Add(eDeliveryFileVM);
            }
            ;
            return result;
        }

        public async Task SendMessagesInputNumber()
        {
            var responseAdministrations = await registerGrpcClient.GetAdministrationUicListAsync(new Google.Protobuf.WellKnownTypes.Empty());
            var edeliveryMessages = await repo.All<EDeliveryMessage>()
                                             .Where(x => x.MessageTypeId == (int)EDeliveryMessageType.Application &&
                                                         x.StatusId == (int)EDeliveryStatus.Ready &&
                                                         x.OutboxId == null)
                                             .ToListAsync();
            var registers = await registerGrpcClient.GetRegisterFullListAsync(new RegisterListRequest { DataTableRequest = new DatatableRequest { Length = -1 } });
            foreach (var edeliveryMessage in edeliveryMessages)
            {
                var administration = responseAdministrations.Data.Where(x => x.Uic == edeliveryMessage.AdministrationUic).First();
                var register = registers.Data.Where(x => x.Id == edeliveryMessage.RegisterId).First();
                var openMessage = JsonSerializer.Deserialize<MessageOpenDO>(edeliveryMessage.Message!)!;
                var outMessage = new EDeliveryMessage
                {
                    IncomingNumber = edeliveryMessage.IncomingNumber,
                    IncomingDate = edeliveryMessage.IncomingDate,
                    Rnu = edeliveryMessage.Rnu,
                    MessageTypeId = (int)EDeliveryMessageType.OutApplication,
                    AdministrationUic = edeliveryMessage.AdministrationUic,
                    TenantId = edeliveryMessage.TenantId,
                    RegisterId = edeliveryMessage.RegisterId,
                    ProcessId = edeliveryMessage.ProcessId,
                    StatusId = (int)EDeliveryStatus.Ready,
                    ProfileId = openMessage.Sender.ProfileId,
                    TemplateId = 1,
                };
                outMessage.MessageText = $"Вх. № {outMessage.IncomingNumber} / {outMessage.IncomingDate.ConvertUtcToBGTime().Value.ToString(FormattingConstant.DateFormat)}";
                var administrationName = !string.IsNullOrEmpty(administration.NameEDelivery) ? administration.NameEDelivery : administration.Name;
                var registerName = !string.IsNullOrEmpty(register.NameEDelivery) ? register.NameEDelivery : register.Name;
                outMessage.SubjectText = $"{administrationName} ({registerName})";
                await repo.AddAsync(outMessage);
                edeliveryMessage.OutboxId = outMessage.Id;
                await repo.SaveChangesAsync();
                await SendOutMessage(outMessage);
            }
        }

        public async Task SendMessagesError()
        {
            var edeliveryMessages = await repo.All<EDeliveryMessage>()
                                             .Where(x => x.MessageTypeId == (int)EDeliveryMessageType.Application &&
                                                         x.StatusId == (int)EDeliveryStatus.Error &&
                                                         x.OutboxId == null)
                                             .ToListAsync();
            foreach (var edeliveryMessage in edeliveryMessages)
            {
                var openMessage = JsonSerializer.Deserialize<MessageOpenDO>(edeliveryMessage.Message!)!;
                var outMessage = new EDeliveryMessage
                {
                    IncomingNumber = edeliveryMessage.IncomingNumber,
                    IncomingDate = edeliveryMessage.IncomingDate,
                    Rnu = edeliveryMessage.Rnu,
                    MessageTypeId = (int)EDeliveryMessageType.OutApplication,
                    AdministrationUic = edeliveryMessage.AdministrationUic,
                    TenantId = edeliveryMessage.TenantId,
                    RegisterId = edeliveryMessage.RegisterId,
                    ProcessId = edeliveryMessage.ProcessId,
                    StatusId = (int)EDeliveryStatus.Ready,
                    ProfileId = openMessage.Sender.ProfileId,
                    TemplateId = 1,
                };
                outMessage.MessageText = $"Неуспешно изпращане на заявлението към регистъра, моля свържете се с администратор на ИСЦИПР.";
                outMessage.SubjectText = $"Неуспешно изпращане на заявление";
                await repo.AddAsync(outMessage);
                edeliveryMessage.OutboxId = outMessage.Id;
                await repo.SaveChangesAsync();
                await SendOutMessage(outMessage);
            }
        }


        public async Task SendMessage(OutboxMessage request)
        {
            var outMessage = new EDeliveryMessage
            {
                Rnu = request.Rnu,
                MessageTypeId = request.MessageTypeId,
                TenantId = request.TenantId.ToGuid(),
                RegisterId = request.RegisterId,
                ProcessId = request.ProcessId.ToGuid(),
                SourceType = request.SourceType,
                SourceId = request.SourceId.ToGuid(),
                StatusId = (int)EDeliveryStatus.Ready,
                MessageText = request.Message,
                SubjectText = request.Subject,
                TemplateId = request.TemplateId
            };
            outMessage.ProfileId = await edeliveryClientService.GetProfileId(request.Uic, request.UicType);
            if (request.OutboxFiles.Any())
            {
                var outboxFile = request.OutboxFiles.First();
                outMessage.FileName = outboxFile.FileName;
                outMessage.FileUrl = outboxFile.FileUrl;
            }
            await repo.AddAsync(outMessage);
            await SendOutMessage(outMessage);
        }

        public async Task SendOutMessage(EDeliveryMessage outMessage)
        {
            if (configuration.GetValue<bool>("EDelivery:StopOutMessages"))
            {
                await repo.SaveChangesAsync();
                return;
            }
            var fileData = new byte[] { };
            if (!string.IsNullOrEmpty(outMessage.FileName))
            {
                fileData = await httpRequester.GetFileAsync("objectStoreClient", outMessage.FileUrl ?? string.Empty);
            }
            if (outMessage.ProfileId != null)
            {
                try
                {
                    (outMessage.MessageId, outMessage.Message, outMessage.Rnu) = await edeliveryClientService.SendMessage(
                      outMessage.ProfileId ?? 0,
                      outMessage.TemplateId,
                      outMessage.SubjectText ?? string.Empty,
                      outMessage.MessageText ?? string.Empty,
                      outMessage.Rnu,
                      outMessage.FileName ?? string.Empty,
                      fileData);
                }
                catch (Exception ex)
                {
                    outMessage.ErrorMessage = ex.Message;
                    outMessage.StatusId = (int)EDeliveryStatus.Error;
                    outMessage.ErrorCountSend += 1;
                }

            }
            else
            {
                outMessage.ErrorMessage = "Няма профил за получателя";
                outMessage.StatusId = (int)EDeliveryStatus.Error;
                outMessage.ErrorCountSend += 1;
            }
            await repo.SaveChangesAsync();
        }

        public async Task RetryEdeliveryMessages()
        {
            var maxErrorCount = 5;
            var outMessages = await repo.All<EDeliveryMessage>()
                                        .Where(x => x.MessageId == 0 &&
                                                    x.ErrorCountSend <= maxErrorCount &&
                                                    x.StatusId == (int)EDeliveryStatus.Error &&
                                                    x.ProfileId > 0)
                                        .ToListAsync();
            foreach (var outMessage in outMessages)
            {
                await SendOutMessage(outMessage);
            }
        }


        private ApplicationFileDataVM? ResolvePdfJson(byte[] fileData)
        {
            var result = new ApplicationFileDataVM();
            try
            {
                using var fileStream = new MemoryStream(fileData);
                PdfDocument pdfDoc = new(new PdfReader(fileStream));
                int objNumber = pdfDoc.GetNumberOfPdfObjects();
                for (int i = 1; i <= objNumber; i++)
                {
                    PdfObject obj = pdfDoc.GetPdfObject(i);

                    if (obj != null && obj.IsDictionary())
                    {
                        PdfDictionary dict = (PdfDictionary)obj;

                        //с изчистени параметри до необходимите и видими полета при попълване на заявление. Този вариант е по-подходящ за работа при интеграции система-система.
                        var key = new PdfName("application.json_json");

                        if (dict.ContainsKey(key))
                        {
                            result.ApplicationJson = dict.GetAsString(key).ToUnicodeString();
                        }

                        //съдържа пълната информация за уеб формата на дадена услуга (оставена от разработчика с цел при нужда от проверки или корекции)
                        key = new PdfName("application.json_submission");

                        if (dict.ContainsKey(key))
                        {
                            result.ApplicationSubmission = dict.GetAsString(key).ToUnicodeString();
                        }
                    }
                }
                if (string.IsNullOrEmpty(result.ApplicationJson))
                    return null;
                using var jsonDocument = JsonDocument.Parse(result.ApplicationJson);
                try
                {
                    result.AdministrationUic = GatherUicFromEForm(jsonDocument);
                }
                catch
                {

                }
                result.ServiceCode = GatherServiceCodeFromEForm(jsonDocument);
                result.Rnu = GatherRnuFromEForm(jsonDocument);
                result.RegisterNumber = GatherRegisterNumber(jsonDocument);
                result.RegisterType = GatherRegisterType(jsonDocument);
                return result;
            }
            catch
            {
                return null;
            }
        }

        private string GatherUicFromEForm(JsonDocument jsonDocument)
        {
            var administrationIdentifier = jsonDocument.RootElement.GetProperty("ServiceRequest")
                    .GetProperty("specificContent")
                    .GetProperty("specificContent")
                    .GetProperty("registerOwner")
                    .GetProperty("value");

            return administrationIdentifier.GetString();
        }
        private string GatherServiceCodeFromEForm(JsonDocument jsonDocument)
        {
            var serviceIdentifier = jsonDocument.RootElement.GetProperty("ServiceRequest")
                    .GetProperty("publicService")
                    .GetProperty("identifier")
                    .GetProperty("identifier");

            return serviceIdentifier.GetInt64().ToString();
        }

        private string? GatherRnuFromEForm(JsonDocument jsonDocument)
        {
            var serviceIdentifier = jsonDocument.RootElement.GetProperty("ServiceRequest")
                    .GetProperty("requestURI")
                    .GetProperty("identifier");

            return serviceIdentifier.GetString();
        }
        private string? GatherRegisterNumber(JsonDocument jsonDocument)
        {
            try
            {
                var registerNumber = jsonDocument.RootElement.GetProperty("ServiceRequest")
                                                    .GetProperty("specificContent")
                                                    .GetProperty("specificContent")
                                                    .GetProperty("__additionalSpecificContent")
                                                    .GetProperty("tags")
                                                    .GetProperty("_registerNumber");

                return registerNumber.GetInt64().ToString();
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
        private string? GatherRegisterType(JsonDocument jsonDocument)
        {
            try
            {
                var registerNumber = jsonDocument.RootElement.GetProperty("ServiceRequest")
                                                    .GetProperty("specificContent")
                                                    .GetProperty("specificContent")
                                                    .GetProperty("__additionalSpecificContent")
                                                    .GetProperty("tags")
                                                    .GetProperty("_registerType");

                return registerNumber.GetString();
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public async Task<List<IntegrationFileMessage>> GetIntegrationFilesUrl(IntegrationFileRequest request)
        {
            var ids = request.Ids.Select(x => x.ToGuid()).ToList();
            var edeliveryFiles = await repo.AllReadonly<EDeliveryFileMetadata>()
                                           .Where(x => ids.Contains(x.Id))
                                           .ToListAsync();
            var result = edeliveryFiles.Select(x => new IntegrationFileMessage
            {
                Id = x.Id.ToString(),
                Url = x.FileId.ToString(),
            })
            .ToList();
            foreach (var file in result)
            {
                file.Url = await objectStoreService.GetPresignedUrl(file.Url);
            }
            return result;
        }

        /// <summary>
        /// Връща списък със записи в лог на електронни връчвания
        /// </summary>
        /// <param name = "request" ></param >
        /// <returns></returns>
        public async Task<(List<EDeliveryProtoMessage>, int)> GetEDeliveryLogRecordsList(DatatableRequest request)
        {
            var query = repo.AllReadonly<EDeliveryMessage>()
                            .IgnoreQueryFilters()
                            .Where(x => x.StatusId == (int)EDeliveryStatus.Error)
                            .TagWith(nameof(GetEDeliveryLogRecordsList));

            var countAll = 0;
            (query, countAll) = await request.GetFilteredData(query);

            var registersResponse = await registerGrpcClient.GetRegisterListAsync(new Empty());
            if (registersResponse.Status != null && registersResponse.Status.Code != ResultCodes.Ok) // Adjust based on ResultStatus
            {
                throw new InvalidOperationException($"Грешка по време на извличане на регистри: {registersResponse.Status.Message}");
            }
            var registerMap = registersResponse.Data.ToDictionary(r => r.Id, r => r.Label);

            var data = (await query.ToListAsync())
                                  .Select(x => new EDeliveryProtoMessage
                                  {
                                      Id = x.Id.ToString(),
                                      //RegisterId = x.RegisterId ?? 0,
                                      RegisterId = x.RegisterId.GetValueOrDefault(0),
                                      ErrorMessage = x.ErrorMessage,
                                      MessageId = x.MessageId,
                                      ModifiedOn = x.ModifiedOn != DateTime.MinValue
                                      ? x.ModifiedOn.ToUniversalTime().ToTimestamp()
                                      : null,
                                      PublicServiceName = x.ApplicationJson != null
                                      ? JsonSerializer.Deserialize<JsonNode>(x.ApplicationJson)?["ServiceRequest"]?["publicService"]?["name"]?.ToString()
                                      : null,
                                      PublicServiceIdentifier = x.ApplicationJson != null
                                      ? JsonSerializer.Deserialize<JsonNode>(x.ApplicationJson)?["ServiceRequest"]?["publicService"]?["identifier"]?["identifier"]?.ToString()
                                      : null,
                                      RegisterName = x.RegisterId != null && registerMap.TryGetValue(x.RegisterId.Value, out var label) ? label : null
                                  }).ToList();

            return (data, countAll);
        }
    }
}
