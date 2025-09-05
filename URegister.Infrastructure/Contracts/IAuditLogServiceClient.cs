using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Infrastructure.Models;

namespace URegister.Infrastructure.Contracts
{
    public interface IAuditLogServiceClient
    {
        Task SaveAuditLogGrpc(IAuditInfo auditInfo, List<AuditEntry>? auditEntries);
    }
}
