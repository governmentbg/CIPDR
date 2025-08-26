using Quartz;
using URegister.IntegrationsCatalog.Contracts;

namespace URegister.IntegrationsCatalog.Jobs
{
    [DisallowConcurrentExecution]
    public class EMailSendJob : IJob
    {
        private readonly ILogger logger;

        private readonly IEMailService eMailService;

        public EMailSendJob(ILogger<EMailSendJob> _logger,
                         IEMailService eMailService)
        {
            logger = _logger;
            this.eMailService = eMailService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            logger.LogInformation("EMail send job executed: Time: {0}", context.FireTimeUtc);

            await eMailService.SendEMails();
        }

    }
}
