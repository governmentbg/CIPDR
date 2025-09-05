using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using URegister.Infrastructure.Model.AuditLog;
using URegister.IntegrationsCatalog;
using URegister.Infrastructure.Extensions;

namespace URegister.Admin.Controllers
{
    [Display(Name = "Лог на електронни връчвания")]
    public class EDeliveryLogController : BaseController
    {
        private readonly IntegrationGrpc.IntegrationGrpcClient _integrationGrpcClient;

        public EDeliveryLogController(IntegrationGrpc.IntegrationGrpcClient integrationGrpcClient) 
        {
            _integrationGrpcClient = integrationGrpcClient;
        }

        [Display(Name = "Начална страница на Лог на електронни връчвания")]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Списък на записи в лог на електронни връчвания
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Извличане на списък със записи в лог на електронни връчвания")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetEDeliveryLogRecordsList(IDataTablesRequest request)
        {
            var protoRequest = request!.GetDataTablesRequestProto();
            var result = await _integrationGrpcClient.GetEDeliveryLogRecordsListAsync(protoRequest);

            return request.GetResponseServerPaging(result.EdeliveryMessages, result.CountAll);
        }
    }
}
