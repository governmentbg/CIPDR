using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using URegister.IntegrationsCatalog.Contracts;

namespace URegister.IntegrationsCatalog.Services
{
    public class EmailSender(
        SmtpClient smtpClient,
        IConfiguration configuration
        ) : IEmailSender
    {
        
        public void Dispose()
        {
            smtpClient.Dispose();
        }

        /// <summary>
        /// Sends Email by given parameters
        /// </summary>
        /// <param name="emailFrom">Email from</param>
        /// <param name="emailTo">Email to</param>
        /// <param name="subject">Subject of the Email</param>
        /// <param name="body">Content of the Email</param>
        public async Task SendEmailAsync(MailAddress emailFrom, MailAddress emailTo, string subject, string body)
        {
            var emailBody = new StringBuilder(body);

            var paragraph1 = "<small><b>Това е автоматично генерирано съобщение. Моля, не отговаряйте!</b></small>";
            var newLine = "<br />";

            emailBody.Append(newLine);
            emailBody.Append(newLine);
            emailBody.Append(paragraph1);

            var message = new MailMessage(
                from: emailFrom,
                to: emailTo);
            message.Subject = subject;
            message.Body = emailBody.ToString();
            message.IsBodyHtml = true;

            await smtpClient.SendMailAsync(message);
        }

        /// <summary>
        /// Sends Email by given parameters
        /// </summary>
        /// <param name="emailTo">Email to</param>
        /// <param name="subject">Subject of the Email</param>
        /// <param name="body">Content of the Email</param>
        public async Task SendEmailAsync(MailAddress emailTo, string subject, string body)
        {
            if (string.IsNullOrEmpty(configuration.GetValue<string>("Email:FromEmail")))
            {
                throw new Exception("Не е конфигуриран адрес за изпращане на майл Email:FromEmail");
            }
            MailAddress emailFrom = new MailAddress(configuration.GetValue<string>("Email:FromEmail")!,
                                                    configuration.GetValue<string>("Email:FromName"));
            await SendEmailAsync(emailFrom, emailTo, subject, body);

        }
}
}
