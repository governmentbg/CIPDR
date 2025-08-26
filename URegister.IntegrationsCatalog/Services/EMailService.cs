using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.ServiceModel.Channels;
using URegister.Infrastructure.Constants;
using URegister.IntegrationsCatalog.Contracts;
using URegister.IntegrationsCatalog.Data;
using URegister.IntegrationsCatalog.Data.Models;
using URegister.RegistersCatalog;
using URegister.Users;


namespace URegister.IntegrationsCatalog.Services
{
    public class EMailService(
        IIntegrationsCatalogRepository repo,
        IEmailSender sender,
        RegistersCatalogGrpc.RegistersCatalogGrpcClient registerGrpcClient,
        AppUserManager.AppUserManagerClient appUserManagerClient,  
        IConfiguration configuration
    ) : IEMailService
    {
        public async Task AddEmailOnInputNumber(string administrationName, EDeliveryMessage edeliveryMessage)
        {
            var registerResponse = await registerGrpcClient.GetRegisterAsync(new GetRegisterRequest
            {
                RegisterId = edeliveryMessage.RegisterId ?? 0,
            });
            var register = registerResponse.Data;
            var usersResponse = await appUserManagerClient.UserReceiveEmailsAsync(new UserReceiveEmailsRequest
            {
                RegisterCode = register.Code,
                AdministrationId = edeliveryMessage.TenantId?.ToString(),
            });
            foreach (var user in usersResponse.UserData)
            {
                var emailMessage = new EMailMessage
                {
                    EMail = user.Email,
                    ErrorMessage = "Получено заявление",
                    Message = $"{administrationName}<br> ({register.Name}) <br> Вх. № {edeliveryMessage.IncomingNumber} / {edeliveryMessage.IncomingDate}",
                    SourceId = edeliveryMessage.Id,
                    SourceType = (int)EMailSourceType.ReceivedEForm,
                    StatusId = (int)EMailStatus.New,
                };
                await repo.AddAsync(emailMessage);
            }
        }
        public async Task SendEMails()
        {
            var messages = await repo.All<EMailMessage>()
                                     .Where(x => x.StatusId == (int)EMailStatus.New)
                                     .ToListAsync();
            var errLimit = configuration.GetValue<int>("Email:MaxFailAttempts");
            foreach (var message in messages)
            {
                try
                {
                    MailAddress emailTo = new MailAddress(message.EMail, message.PersonName);
                    await sender.SendEmailAsync(emailTo, message.Subject, message.Message);
                    message.StatusId = (int)EMailStatus.Send;
                }
                catch (Exception ex)
                {
                    message.ErrorCount++;
                    message.ErrorMessage = ex.Message;
                    
                    if (message.ErrorCount >= errLimit)
                    {
                        message.StatusId = (int)EMailStatus.Error;
                    }
                }
            }
            await repo.SaveChangesAsync();
        }
    }
}
