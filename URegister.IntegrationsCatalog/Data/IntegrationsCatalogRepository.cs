using URegister.Infrastructure.Contracts;
using URegister.Infrastructure.Data.Common;

namespace URegister.IntegrationsCatalog.Data
{
    /// <summary>
    /// Репозитори за работа с каталога на регистрите
    /// </summary>
    public class IntegrationsCatalogRepository : Repository, IIntegrationsCatalogRepository
    {
        /// <summary>
        /// Създава ново репозитори за работа с каталога на регистрите
        /// </summary>
        /// <param name="context"></param>
        /// <param name="auditInfo"></param>
        /// <param name="auditLogClient"></param>
        public IntegrationsCatalogRepository(IntegrationsCatalogDbContext context,
            IAuditInfo auditInfo,
            IAuditLogServiceClient auditLogClient)
        {
            Context = context;
            this.auditInfo = auditInfo;
            this.auditLogClient = auditLogClient;
        }
    }
}
