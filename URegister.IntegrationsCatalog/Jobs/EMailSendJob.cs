using Quartz;
using URegister.IntegrationsCatalog.Contracts;

namespace URegister.IntegrationsCatalog.Jobs
{
    [DisallowConcurrentExecution]
    public class EDeliverySendJob : IJob
    {
        private readonly ILogger logger;

        private readonly IEDeliveryService eDeliveryService;

        public EDeliverySendJob(ILogger<EDeliverySendJob> _logger,
                         IEDeliveryService _eDeliveryService)
        {
            logger = _logger;
            eDeliveryService = _eDeliveryService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            logger.LogInformation("EDelivery send job executed: Time: {0}", context.FireTimeUtc);

            await eDeliveryService.SendMessagesInputNumber();
        }

    }
}
