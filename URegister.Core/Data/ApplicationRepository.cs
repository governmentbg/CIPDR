using URegister.Infrastructure.Data;
using URegister.Infrastructure.Data.Common;

namespace URegister.Core.Data
{
    public class ApplicationRepository : Repository, IApplicationRepository
    {
        public ApplicationRepository(ApplicationDbContext context)
        {
            this.Context = context;
        }
    }
}
