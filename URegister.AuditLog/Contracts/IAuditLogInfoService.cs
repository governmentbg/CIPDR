
using Microsoft.AspNetCore.Mvc;
using URegister.Common;
using URegister.Infrastructure.Model.AuditLog;

namespace URegister.AuditLog.Contracts
{
    public interface IAuditLogInfoService
    {
        Task AddAuditLogAndEntities(AuditEntitiesMessage request);

        Task<(List<AuditMessage>, int)> GetAuditLogRecordsList(DatatableRequestWithAuditLogFilter request);

        Task<List<AuditEntityMessage>> GetAuditEntityValues(string auditId);
    }
}
