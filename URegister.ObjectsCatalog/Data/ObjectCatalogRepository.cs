using URegister.Infrastructure.Data.Common;

namespace URegister.ObjectsCatalog.Data
{
    /// <summary>
    /// Репозитори за каталога на обектите
    /// </summary>
    public class ObjectCatalogRepository : Repository, IObjectCatalogRepository
    {
        /// <summary>
        /// Създава ново репозитори за каталога на обектите
        /// </summary>
        /// <param name="context"></param>
        public ObjectCatalogRepository(ObjectCatalogDbContext context)
        {
            Context = context;
        }
    }
}
