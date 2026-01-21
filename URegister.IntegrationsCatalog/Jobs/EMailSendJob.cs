using Quartz;
using URegister.Infrastructure.Contracts;
using URegister.IntegrationsCatalog.Contracts;

namespace URegister.IntegrationsCatalog.Jobs
{
    [DisallowConcurrentExecution]
    public class EMailSendJob : IJob
    {
        private readonly ILogger logger;

        private readonly IEMailService eMailService;
        private readonly IAuditInfo auditInfo;
        public EMailSendJob(ILogger<EMailSendJob> _logger,
                         IEMailService eMailService,
                         IAuditInfo _auditInfo)
        {
            logger = _logger;
            this.eMailService = eMailService;
            auditInfo = _auditInfo;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            logger.LogInformation("EMail send job executed: Time: {0}", context.FireTimeUtc);
            auditInfo.SetAuditInfoForQuartz("EDelivery", "Receive");
            await eMailService.SendEMails();
        }

    }
}
