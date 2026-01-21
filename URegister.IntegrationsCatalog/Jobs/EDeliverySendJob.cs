using Quartz;
using URegister.Infrastructure.Contracts;
using URegister.IntegrationsCatalog.Contracts;

namespace URegister.IntegrationsCatalog.Jobs
{
    [DisallowConcurrentExecution]
    public class EDeliverySendJob : IJob
    {
        private readonly ILogger logger;

        private readonly IEDeliveryService eDeliveryService;
        private readonly IAuditInfo auditInfo;
        public EDeliverySendJob(ILogger<EDeliverySendJob> _logger,
                         IEDeliveryService _eDeliveryService,
                         IAuditInfo _auditInfo)
        {
            logger = _logger;
            eDeliveryService = _eDeliveryService;
            auditInfo = _auditInfo;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            logger.LogInformation("EDelivery send job executed: Time: {0}", context.FireTimeUtc);
            auditInfo.SetAuditInfoForQuartz("EMail", "Send");
            await eDeliveryService.SendMessagesInputNumber();
        }

    }
}
