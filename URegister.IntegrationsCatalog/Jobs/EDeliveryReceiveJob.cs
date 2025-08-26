using Quartz;
using URegister.IntegrationsCatalog.Contracts;

namespace URegister.IntegrationsCatalog.Jobs
{
    [DisallowConcurrentExecution]
    public class EDeliveryReceiveJob : IJob
    {
        private readonly ILogger logger;

        private readonly IEDeliveryService eDeliveryService;

        public EDeliveryReceiveJob(ILogger<EDeliveryReceiveJob> _logger,
                         IEDeliveryService _eDeliveryService)
        {
            logger = _logger;
            eDeliveryService = _eDeliveryService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            logger.LogInformation("EDelivery job executed: Time: {0}", context.FireTimeUtc);

            await eDeliveryService.ReceiveMessages();
        }

    }
}
