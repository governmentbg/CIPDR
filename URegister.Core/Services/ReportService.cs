using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Core.Contracts;
using URegister.Core.Data;
using URegister.Core.Data.Models.Common;
using URegister.Core.Data.Models.Process;
using URegister.Core.Models.Process;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Model.Report;

namespace URegister.Core.Services
{
    public class ReportService : BaseService, IReportService
    {
        
        private readonly ILogger _logger;
        public ReportService(IApplicationRepository repo, ILogger<ReportService> logger) : base(repo, logger)
        {
            _logger = logger;
        }

        public async Task<StatisticalReportViewModel> GenerateStatisticalReport(DateTime? dateFrom, DateTime? dateTo, string registerTypeEntry)
        {
            var report = new StatisticalReportViewModel 
            { 
              DateFrom = dateFrom,
              DateTo = dateTo      
            };

            DateTime? dateFromUtc = dateFrom.HasValue ? dateFrom.Value.ToUniversalTime() : null;
            DateTime? dateToUtc = dateTo.HasValue ? dateTo.Value.ToUniversalTime().AddDays(1) : null;

            if (registerTypeEntry == RegisterTypeEntry.Applicant) 
            {
                report.TotalSentProcesses = await Repo.AllReadonly<Process>()
                    .Where(p => p.StatusId == (int)ProcessStatus.Send
                        && (!dateFromUtc.HasValue || p.IncomingDate >= dateFromUtc.Value)
                        && (!dateToUtc.HasValue || p.IncomingDate <= dateToUtc.Value))
                    .CountAsync();

                report.SentCounterProcesses = await Repo.AllReadonly<Process>()
                    .Where(p => p.StatusId == (int)ProcessStatus.Send
                        && p.ReceivedChannelId == ChannelType.OnDesk
                        && (!dateFromUtc.HasValue || p.IncomingDate >= dateFromUtc.Value)
                        && (!dateToUtc.HasValue || p.IncomingDate <= dateToUtc.Value))
                    .CountAsync();

                report.SentOnlineProcesses = await Repo.AllReadonly<Process>()
                    .Where(p => p.StatusId == (int)ProcessStatus.Send
                        && p.ReceivedChannelId == ChannelType.EDelivery
                        && (!dateFromUtc.HasValue || p.IncomingDate >= dateFromUtc.Value)
                        && (!dateToUtc.HasValue || p.IncomingDate <= dateToUtc.Value))
                    .CountAsync();
            }

            report.TotalRegisteredProcesses = await Repo.AllReadonly<Process>()
                    .Where(p => p.StatusId == (int)ProcessStatus.Registered
                        && (!dateFromUtc.HasValue || p.IncomingDate >= dateFromUtc.Value)
                        && (!dateToUtc.HasValue || p.IncomingDate <= dateToUtc.Value))
                    .CountAsync();

            report.RegisteredProcessesInitialRegistrations = await Repo.AllReadonly<Process>()
                    .Where(p => p.StatusId == (int)ProcessStatus.Registered
                        && p.Service.ServiceTypeId == (int)ServiceTypes.Register
                        && (!dateFromUtc.HasValue || p.IncomingDate >= dateFromUtc.Value)
                        && (!dateToUtc.HasValue || p.IncomingDate <= dateToUtc.Value))
                    .CountAsync();

            report.RegisteredProcessesCircumstanceChanges = await Repo.AllReadonly<Process>()
                .Where(p => p.StatusId == (int)ProcessStatus.Registered
                    && p.Service.ServiceTypeId == (int)ServiceTypes.Change
                    && (!dateFromUtc.HasValue || p.IncomingDate >= dateFromUtc.Value)
                    && (!dateToUtc.HasValue || p.IncomingDate <= dateToUtc.Value))
                .CountAsync();

            report.RegisteredProcessesDeletions = await Repo.AllReadonly<Process>()
                .Where(p => p.StatusId == (int)ProcessStatus.Registered
                    && p.Service.ServiceTypeId == (int)ServiceTypes.Deletion
                    && (!dateFromUtc.HasValue || p.IncomingDate >= dateFromUtc.Value)
                    && (!dateToUtc.HasValue || p.IncomingDate <= dateToUtc.Value))
                .CountAsync();

            report.TechnicalErrorsFixed = await Repo.AllReadonly<Process>()
                .Where(p => (p.StatusId == (int)ProcessStatus.Registered || p.StatusId == (int)ProcessStatus.Refused)
                    && p.Service.ServiceTypeId == (int)ServiceTypes.AskForCorrectionError
                    && (!dateFromUtc.HasValue || p.IncomingDate >= dateFromUtc.Value)
                    && (!dateToUtc.HasValue || p.IncomingDate <= dateToUtc.Value))
                .CountAsync();

            report.Rejections = await Repo.AllReadonly<Process>()
                .Where(p => p.StatusId == (int)ProcessStatus.Refused
                    && (!dateFromUtc.HasValue || p.IncomingDate >= dateFromUtc.Value)
                    && (!dateToUtc.HasValue || p.IncomingDate <= dateToUtc.Value))
                .CountAsync();

            report.Instructions = await Repo.AllReadonly<Instruction>()
                .Where(i => (!dateFromUtc.HasValue || i.CreatedOn >= dateFromUtc.Value)
                    && (!dateToUtc.HasValue || i.CreatedOn <= dateToUtc.Value))
                .CountAsync();

            //report.IssuedCertificates = await Repo.AllReadonly<FileMetadata>()
            //    .Where(f => f.FileSourceTypeId == (int)FileSourceType.Certificate)
            //    .CountAsync();

            report.IssuedCertificates = await Repo.AllReadonly<FileMetadata>()
                .Where(f => f.FileSourceTypeId == (int)FileSourceType.Certificate
                    && (!dateFromUtc.HasValue || f.ModifiedOn >= dateFromUtc.Value)
                    && (!dateToUtc.HasValue || f.ModifiedOn <= dateToUtc.Value))
                .CountAsync();

            report.RegisterTypeEntry = registerTypeEntry;

            return report;
        }
    }
}