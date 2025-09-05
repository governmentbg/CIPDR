using System.Net.Mail;

namespace URegister.IntegrationsCatalog.Contracts
{
    public interface IEmailSender
    {
        Task SendEmailAsync(MailAddress emailTo, string subject, string body);
    }
}
