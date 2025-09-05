using DataTables.AspNet.Core;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using URegister.AuditLog;
using URegister.Common;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Extensions;
using URegister.Infrastructure.Model.AuditLog;
using URegister.Users;
using static FastExpressionCompiler.ExpressionCompiler;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static URegister.Users.AppUserManager;

namespace URegister.Admin.Controllers
{
    [Display(Name = "Системен журнал")]
    public class AuditLogController(
        AuditLogGrpc.AuditLogGrpcClient auditLogClient,
        AppUserManagerClient appUserManagerClient
        ) : BaseController
    {       
        [Authorize(Roles = $"{UserRoles.Manager},{UserRoles.Editor},{UserRoles.Registrator},{UserRoles.GlobalAdmin}")]
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
            var protoRequest = request!.GetDataTablesRequestProto();
            var complexRequest = new DatatableRequestWithAuditLogFilter
            {
                Request = protoRequest,
                Filter = new AuditLogFilter
                {
                    DateFrom = filter.DateFrom.HasValue ? Timestamp.FromDateTime(filter.DateFrom.Value.ToUniversalTime()) : null,
                    DateTo = filter.DateTo.HasValue ? Timestamp.FromDateTime(filter.DateTo.Value.ToUniversalTime()) : null,
                    Method = filter.ActionType ?? string.Empty,
                    IpAddress = filter.IpAddress ?? string.Empty,
                    UserName = filter.UserName ?? string.Empty
                }
            };
            var response = await auditLogClient.GetAuditLogRecordsListAsync(complexRequest);
         
            return request.GetResponseServerPaging(response.AuditList, response.CountAll);
        }

        /// <summary>
        /// Връща списък със стари и нови стойности на запис в системния журнал
        /// </summary>
        /// <param name="auditId"></param>
        /// <returns></returns>
        [HttpGet]
        [Display(Name = "Извличане на стари и нови стойности")]
        public async Task<IActionResult> GetAuditEntityValues(string auditId)
        {
            var auditIdAsStringValue = new StringValue { Value = auditId };
            var response = await auditLogClient.GetAuditEntityValuesAsync(auditIdAsStringValue);
            if ((int)response.Status.Code == (int)ResultCodes.Ok)
            {
                return Ok(response.AuditEntities);
            }
            return BadRequest(new { Error = response.Status.Message });
        }
    }
}
