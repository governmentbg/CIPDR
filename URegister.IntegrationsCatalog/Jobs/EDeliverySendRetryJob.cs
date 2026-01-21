using Quartz;
using URegister.Infrastructure.Contracts;
using URegister.IntegrationsCatalog.Contracts;

namespace URegister.IntegrationsCatalog.Jobs
{
    [DisallowConcurrentExecution]
    public class EDeliverySendRetryJob : IJob
    {
        private readonly ILogger logger;

        private readonly IEDeliveryService eDeliveryService;
        private readonly IAuditInfo auditInfo;

        public EDeliverySendRetryJob(ILogger<EDeliverySendRetryJob> _logger,
                         IEDeliveryService _eDeliveryService,
                         IAuditInfo _auditInfo)
        {
            logger = _logger;
            eDeliveryService = _eDeliveryService;
            auditInfo = _auditInfo;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            logger.LogInformation("EDeliverySendRetryJob job executed: Time: {0}", context.FireTimeUtc);
            auditInfo.SetAuditInfoForQuartz("EDelivery", "SendRetry");
            await eDeliveryService.RetryEdeliveryMessages();
        }

    }
}
