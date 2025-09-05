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
        public IntegrationsCatalogRepository(IntegrationsCatalogDbContext context)
        {
            Context = context;
        }
    }
}
