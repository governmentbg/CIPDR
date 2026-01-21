using Quartz;
using URegister.Infrastructure.Contracts;
using URegister.Core.Contracts;
using URegister.Infrastructure.Constants;

namespace URegister.Jobs
{
    [DisallowConcurrentExecution]
    public class ProcessEMailSendJob : IJob
    {
        private readonly ILogger logger;

        private readonly IProcessEMailService processEMailService;
        private readonly IAuditInfo auditInfo;
        public ProcessEMailSendJob(ILogger<ProcessEMailSendJob> _logger,
                         IProcessEMailService processEMailService,
                         IAuditInfo _auditInfo)
        {
            logger = _logger;
            this.processEMailService = processEMailService;
            auditInfo = _auditInfo;
            auditInfo.TypeAuditTask = TypeAuditTask.None;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            logger.LogInformation("EMail send job executed: Time: {0}", context.FireTimeUtc);
            auditInfo.SetAuditInfoForQuartz("ProcessEDelivery", "Receive");
            await processEMailService.SendEMailsForSrok();
        }

    }
}
