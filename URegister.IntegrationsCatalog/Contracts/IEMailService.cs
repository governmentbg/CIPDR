using URegister.IntegrationsCatalog.Data.Models;

namespace URegister.IntegrationsCatalog.Contracts
{
    public interface IEMailService
    {
        Task AddEmailOnError(EDeliveryMessage edeliveryMessage);
        Task AddEmailOnInputNumber(string administrationName, EDeliveryMessage edeliveryMessage);
        Task AddEmailOnInstructionResponse(EDeliveryMessage edeliveryMessage);
        Task SendEMails();
        Task SendEMailsForSrok(string registerCode, string tenantId, string processId, string subject, string messageText);
    }
}
