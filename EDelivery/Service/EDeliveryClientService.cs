using EDelivery.Contracts;
using EDelivery.Integration;
using EDelivery.Integration.Clients;
using EDelivery.Integration.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EDelivery.Service
{
    public class EDeliveryClientService(
        ITokenService tokenService,
        IFileService fileService,
        IMessagesClient messagesClient,
        IBlobsClient blobsClient,
        ITemplatesClient templatesClient,
        IProfilesClient profilesClient,
        ILogger<EDeliveryClientService> logger, 
        ITargetGroupsClient targetGroupsClient
        ): IEDeliveryClientService
    {
        public async Task<ICollection<InboxDO>> GetMessageList(int? offset, int? length)
        {
            var miscinfo = await tokenService.GetMiscinfo();
            var responce = await messagesClient.GetInboxAsync(miscinfo, offset, length);
            return responce.Result;
        }

        public async Task<MessageOpenDO> OpenMessage(int messageId)
        {
            var miscinfo = await tokenService.GetMiscinfo();
            return await messagesClient.OpenAsync(miscinfo, messageId);
        }
        public async Task<MessageViewDO> ViewMessage(int messageId)
        {
            var miscinfo = await tokenService.GetMiscinfo();
            return await messagesClient.ViewAsync(miscinfo, messageId);
        }

        public async Task<BlobDO[]?> GetMessageBlobs(MessageOpenDO message)
        {
            var miscinfo = await tokenService.GetMiscinfo();
            var template = await templatesClient.DetailsAsync(miscinfo, message.TemplateId);
            var fileContentId = template.Content.Where(x => x.Type == ComponentType.File).Select(x => x.Id).FirstOrDefault();
            if (!message.Fields.ContainsKey(fileContentId.ToString()))
                return null;
            var fileInfo = message.Fields[fileContentId.ToString()].ToString();
            if (fileInfo == null)
                return null;
            return JsonSerializer.Deserialize<BlobDO[]>(fileInfo);
        }

        public async Task<string?> GetMessageText(MessageOpenDO message)
        {
            var miscinfo = await tokenService.GetMiscinfo();
            var template = await templatesClient.DetailsAsync(miscinfo, message.TemplateId);
            var textContentId = template.Content.Where(x => x.Type == ComponentType.Textfield || x.Type == ComponentType.Textarea).Select(x => x.Id).FirstOrDefault();
            return message.Fields[textContentId.ToString()].ToString();
        }

        public async Task<byte[]> DownLoadFile(string url)
        {
            return await fileService.DownLoadFile(url);
        }
        
        public async Task<int?> GetProfileId(string uic, string uicType)
        {
            var miscinfo = await tokenService.GetMiscinfo();
            var targetGroups = await targetGroupsClient.ListAsync(miscinfo);
            foreach (var targetGroup in targetGroups)
            {
                try
                {
                    var model = await profilesClient.SearchAsync(miscinfo, uic, null, targetGroup.TargetGroupId);
                    if (model?.ProfileId != null)
                       return model?.ProfileId;
                }
                catch (Exception ex)
                {
                  //  logger.LogError(ex, ex.Message);
                }

            }
            return null;
        }

        public async Task<(int, string, string)> SendMessage(int profileId, int templateId, string subject, string message,  string? rnu, string fileName, byte[] fileData)
        {
            var miscinfo = await tokenService.GetMiscinfo();
            BlobDO? blob = null;
            if (fileData.Length > 0)
            {
                blob = await fileService.UploadFile(fileName, fileData);
            }
            var template = await templatesClient.DetailsAsync(miscinfo, templateId);
            var fileContentId = template.Content.Where(x => x.Type == ComponentType.File).Select(x => x.Id).FirstOrDefault();
            var textContentId = template.Content.Where(x => x.Type == ComponentType.Textarea).Select(x => x.Id).FirstOrDefault();
            var fields = new Dictionary<string, object>();
            if (blob != null)
            {
                fields.Add(fileContentId.ToString(), new int[] { blob.BlobId });
            } else
            {
                fields.Add(fileContentId.ToString(), new int[]{});
            }
            fields.Add(textContentId.ToString(), message);
            var messageDO = new MessageSendDO(fields, [profileId], rnu, subject, templateId);
            var result = await messagesClient.SendAsync(miscinfo, messageDO);
            var viewDO = await messagesClient.ViewAsync(miscinfo, result);
            var messageJson = JsonSerializer.Serialize(messageDO);
            return (result, messageJson, viewDO.Rnu);
        }

        public async Task<ICollection<OutboxDO>> GetOutMessageList(int? offset, int? length)
        {
            var miscinfo = await tokenService.GetMiscinfo();
            var responce = await messagesClient.GetOutboxAsync(miscinfo, offset, length);
            return responce.Result;
        }

    }
}

