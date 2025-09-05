using URegister.Infrastructure.Contracts;
using URegister.Infrastructure.Data.Common;

namespace URegister.NomenclaturesCatalog.Data
{
    /// <summary>
    /// Репозитори за работа с каталога на регистрите
    /// </summary>
    public class NomenclaturesCatalogRepository : Repository, INomenclaturesCatalogRepository
    {
        /// <summary>
        /// Създава ново репозитори за работа с каталога на регистрите
        /// </summary>
        /// <param name="context"></param>
        public NomenclaturesCatalogRepository(
            NomenclaturesCatalogDbContext context,
            IAuditInfo auditInfo,
            IAuditLogServiceClient auditLogClient
        )
        {
            Context = context;
            this.auditInfo = auditInfo;
            this.auditLogClient = auditLogClient;
        }
    }
}
