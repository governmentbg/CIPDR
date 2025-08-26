using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Core.Data.Models.Common;
using URegister.Core.Data.Models.Process;

namespace URegister.Core.Contracts
{
    public interface IProcessTemplateService
    {
        Task<string> GetProcessCertificateHtml(Process process, Process processCertificate, int serviceIdCertificate, List<RegisterItem> registerItemsCertificate, List<RegisterItem> registerItems);
        Task<string> GetProcessCertificateOnRegisterHtml(Process process, List<RegisterItem> registerItems, BlanksTemplate blanksTemplate);
        Task<JsonResult?> GetRegistrationProcessList(Guid administrationId, int skip, int take, string searchKey, string searchPattern, DateTime? toDate, DateTime? fromDate);
    }
}
