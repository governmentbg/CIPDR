using EDelivery.Integration.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.Integration
{
    public interface IEDeliveryClientService
    {
        Task<byte[]> DownLoadFile(string url);
        Task<BlobDO[]?> GetMessageBlobs(MessageOpenDO message);
        Task<ICollection<InboxDO>> GetMessageList(int? offset, int? length);
        Task<MessageOpenDO> OpenMessage(int messageId);
        Task<int?> GetProfileId(string uic, string uicType);
        Task<(int, string, string)> SendMessage(int profileId, int templateId, string subject, string message, string? rnu, string fileName, byte[] fileData);
        Task<ICollection<OutboxDO>> GetOutMessageList(int? offset, int? length);
        Task<string?> GetMessageText(MessageOpenDO message);
        Task<MessageViewDO> ViewMessage(int messageId);
    }
}
