using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Infrastructure.Model.AuditLog;

namespace URegister.Core.Contracts
{
    public interface IAuditLogService
    {
        /// <summary>
        /// Връща списък със записи в системния журнал
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<IActionResult> GetAuditLogRecordsList(IDataTablesRequest request, AuditLogFilterVM filter);

        /// <summary>
        /// Връща списък със стари и нови стойности на запис в системния журнал
        /// </summary>
        /// <param name="auditId"></param>
        /// <returns></returns>
        Task<IActionResult> GetAuditEntityValues(Guid auditId);
    }
}
