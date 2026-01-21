using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Core.Contracts;
using URegister.Core.Data;
using URegister.Core.Data.Models.Common;
using URegister.Core.Data.Models.Process;
using URegister.Infrastructure.Constants;
using URegister.IntegrationsCatalog;
using URegister.RegistersCatalog;

namespace URegister.Core.Services
{
    public class ProcessEMailService: BaseService, IProcessEMailService
    {
        private readonly IntegrationGrpc.IntegrationGrpcClient integrationGrpcClient;
        private readonly IRegisterService registerService;
        private readonly RegistersCatalogGrpc.RegistersCatalogGrpcClient registerGrpcClient;
        public ProcessEMailService(
            IApplicationRepository repo,
            ILogger<BaseService> logger,
            IntegrationGrpc.IntegrationGrpcClient integrationGrpcClient,
            IRegisterService registerService,
            RegistersCatalogGrpc.RegistersCatalogGrpcClient registerGrpcClient
        ) : base(repo, logger)
        {
            this.integrationGrpcClient = integrationGrpcClient;
            this.registerService = registerService;
            this.registerGrpcClient = registerGrpcClient;
        }
        public async Task SendEMailsForSrok()
        {
              var services = await Repo.AllReadonly<Service>()
                                  .Where(x => x.ServiceTypeId == (int)ServiceTypes.Register ||
                                              x.ServiceTypeId == (int)ServiceTypes.Change)
                                  .Select(x => x.Id)
                                  .ToListAsync();
            var register = await registerService.GetCurrentRegister();
            var deadLineDate = DateTime.UtcNow.AddDays(1);
            var processes = await Repo.All<Process>()
                                      .Where(x => !x.IsSendEMailDeadlineDate &&
                                                  x.DeadlineDate < deadLineDate &&
                                                  x.StatusId != (int)ProcessStatus.Registered &&
                                                  x.StatusId != (int)ProcessStatus.Refused &&
                                                  services.Contains(x.ServiceId)
                                            )
                                      .ToListAsync();
            foreach (var process in processes)
            {
                var administrationResponse = await registerGrpcClient.GetAdministrationAsync(new GetAdministrationRequest
                {
                    AdministrationId = process.TenantId.ToString(),
                });
                string administrationName = administrationResponse.Data.Name;
                var response = await integrationGrpcClient.SendEmailForSrokAsync(new EMailForSrokRequest
                {
                    RegisterCode = register.Code,
                    TenantId = process.TenantId.ToString(),
                    ProcessId = process.Id.ToString(),
                    Subject = "Уведомление за изтичащ срок за обработка на заявление в ИСЦИПР",
                    Message = $"Срокът за обработка на подадено заявление, в {register.Name}, {administrationName} регистрирано в системата с вх. № {process.IncomingNumber} изтича на {process.DeadlineDate:dd.MM.yyyy}"
                });
                process.IsSendEMailDeadlineDate = true;
            }
            await Repo.SaveChangesAsync();
        }
    }
}
