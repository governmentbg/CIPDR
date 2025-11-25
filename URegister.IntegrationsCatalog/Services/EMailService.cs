using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.ServiceModel.Channels;
using URegister.Common;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Extensions;
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
                    Subject = "Получено заявление",
                    Message = $"В {administrationName}<br> ({register.Name}) <br>" +
                              $"е постъпило ново заявление за обработка <br>" +
                              $"Вх. № {edeliveryMessage.IncomingNumber} / {edeliveryMessage.IncomingDate.ConvertUtcToBGTime().Value.ToString(FormattingConstant.DateFormat)}" +
                              $"Моля, отворете модул „Заявени услуги“ -> \"Управление\" за преглед и последващи действия.",
                    SourceId = edeliveryMessage.Id,
                    SourceType = (int)EMailSourceType.ReceivedEForm,
                    StatusId = (int)EMailStatus.New,
                };
                await repo.AddAsync(emailMessage);
            }
        }

        public async Task AddEmailOnError(EDeliveryMessage edeliveryMessage)
        {
            var rolesResponse = await appUserManagerClient.GetRolesAsync(new Google.Protobuf.WellKnownTypes.Empty());
            var role = rolesResponse.Roles.Where(x => x.Name == "GlobalAdmin").First();
            var usersResponse = await appUserManagerClient.GetUserListAsync(new UserFilter
            {
              //  RoleId = role.RoleId,
                AdministrationId = Guid.Empty.ToString(),
                DatatableRequest = new DatatableRequest { Length = -1 },
                ReceiveEmailOnError = true
            });
            foreach (var user in usersResponse.Users)
            {
                var emailMessage = new EMailMessage
                {
                    EMail = user.Email,
                    Subject = "Проблем при обработка на заявление",
                    Message = $"Възникна грешка при импорт на подадено заявление № {edeliveryMessage.MessageId}, <br>" +
                              "чрез еФорми.Необходимо е потребител с роля „Глобален администратор МЕУ“ да извърши проверка на възникналата грешка в модул „Лог на електронните връчвания\".",
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
                                  //   .Where(x => x.EMail == "a.stoyanov@is-bg.net")
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
