using URegister.AuditLog;
using URegister.Infrastructure.Contracts;
using URegister.Infrastructure.Data.Common;

namespace URegister.RegistersCatalog.Data
{
    /// <summary>
    /// Репозитори за работа с каталога на регистрите
    /// </summary>
    public class RegistersCatalogRepository : Repository, IRegistersCatalogRepository
    {
        /// <summary>
        /// Създава ново репозитори за работа с каталога на регистрите
        /// </summary>
        /// <param name="context"></param>
        public RegistersCatalogRepository(
            RegistersCatalogDbContext context, 
            IAuditLogServiceClient auditLogClient,
            IAuditInfo auditInfo)
        {
            Context = context;
            this.auditLogClient = auditLogClient;
            this.auditInfo = auditInfo;
        }
    }
}
