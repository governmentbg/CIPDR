using URegister.IntegrationsCatalog.Data.Models;

namespace URegister.IntegrationsCatalog.Contracts
{
    public interface IEMailService
    {
        Task AddEmailOnInputNumber(string administrationName, EDeliveryMessage edeliveryMessage);
        Task SendEMails();
    }
}
