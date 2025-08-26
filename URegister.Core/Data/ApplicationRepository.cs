using URegister.Infrastructure.Contracts;
using URegister.Infrastructure.Data;
using URegister.Infrastructure.Data.Common;

namespace URegister.Core.Data
{
    public class ApplicationRepository : Repository, IApplicationRepository
    {
        public ApplicationRepository(ApplicationDbContext context, IAuditInfo auditInfo)
        {
            this.Context = context;
            this.auditInfo = auditInfo;
        }
    }
}
