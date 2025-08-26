

using URegister.Common;
using URegister.IntegrationsCatalog.Data.Models;

namespace URegister.IntegrationsCatalog.Contracts
{
    public interface IEDeliveryService
    {
        Task<List<IntegrationFileMessage>> GetIntegrationFilesUrl(IntegrationFileRequest request);
        Task ReceiveMessages();
        Task SendMessage(OutboxMessage request);
        Task SendMessagesInputNumber();

        /// <summary>
        /// Връща списък със записи в лог на електронни връчвания
        /// </summary>
        /// <param name = "request" ></param >
        /// <returns></returns>
        Task<(List<EDeliveryProtoMessage>, int)> GetEDeliveryLogRecordsList(DatatableRequest request);
    }
}
