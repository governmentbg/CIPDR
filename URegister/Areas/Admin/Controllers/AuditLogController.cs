using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using URegister.Core.Contracts;
using URegister.Core.Models.Process;
using URegister.Core.Services;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Model.AuditLog;

namespace URegister.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{UserRoles.Manager},{UserRoles.Editor},{UserRoles.Registrator}")]
    [Display(Name = "Системен журнал")]
    public class AuditLogController(
        IAuditLogService auditLogService
        ) : BaseController
    {   
        [Display(Name = "Зареждане на страница Системен журнал")]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Списък на записи в системен журнал
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Извличане на списък със записи в системния журнал")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetAuditLogRecordsList(IDataTablesRequest request, AuditLogFilterVM filter)
        {
            return await auditLogService.GetAuditLogRecordsList(request, filter);
        }

        /// <summary>
        /// Връща списък със стари и нови стойности на запис в системния журнал
        /// </summary>
        /// <param name="auditId"></param>
        /// <returns></returns>
        [HttpGet]
        [Display(Name = "Извличане на стари и нови стойности")]
        public async Task<IActionResult> GetAuditEntityValues(Guid auditId)
        {
            return await auditLogService.GetAuditEntityValues(auditId);
        }
    }
}
