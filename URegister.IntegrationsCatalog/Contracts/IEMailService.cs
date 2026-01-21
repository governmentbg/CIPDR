using URegister.IntegrationsCatalog.Data.Models;

namespace URegister.IntegrationsCatalog.Contracts
{
    public interface IEMailService
    {
        Task AddEmailOnError(EDeliveryMessage edeliveryMessage);
        Task AddEmailOnInputNumber(string administrationName, EDeliveryMessage edeliveryMessage);
        Task SendEMails();
    }
}
