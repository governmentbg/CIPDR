using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using URegister.Core.Contracts;
using URegister.Core.Data;
using URegister.Core.Data.Models.Common;
using URegister.Core.Data.Models.Process;
using URegister.Core.Models.Process;
using URegister.Infrastructure.Constants;

namespace URegister.Core.Services
{
    public class DashboardService : BaseService, IDashboardService
    {
        public DashboardService(IApplicationRepository repo,
            ILogger<BaseService> logger)
            : base(repo, logger)
        {
        }

        /// <summary>
        /// Връща данни за Dashboard
        /// </summary>     
        /// <returns></returns>
        public async Task<DashboardVM> GetDashboardData()
        {
            var model = new DashboardVM();
            model.Process.ProcessAllCount = await Repo.AllReadonly<Process>().CountAsync();
            model.Process.ProcessSentCount = await Repo.AllReadonly<Process>().Where(p => p.StatusId == (int)ProcessStatus.Send).CountAsync();
            model.Process.ProcessInProgressCount = await Repo.AllReadonly<Process>().Where(p => p.StatusId == (int)ProcessStatus.InWork).CountAsync();
            model.Process.ProcessRegisteredCount = await Repo.AllReadonly<Process>().Where(p => p.StatusId == (int)ProcessStatus.Registered).CountAsync();
            model.Process.ProcessRefusedCount = await Repo.AllReadonly<Process>().Where(p => p.StatusId == (int)ProcessStatus.Refused).CountAsync();
            model.Process.ProcessCertificateCount = await Repo.AllReadonly<Process>().Where(p => p.StatusId == (int)ProcessStatus.Certificate).CountAsync();

            model.BlanksTemplateCount = await Repo.AllReadonly<BlanksTemplate>().CountAsync();
            model.CustomViewCount = await Repo.AllReadonly<CustomView>().CountAsync();
            
            model.Form.FormAllCount = await Repo.AllReadonly<Form>().IgnoreQueryFilters().Where(f => f.IsActive || f.ApprovalStatus == (int)ApprovalStatus.Requested).CountAsync();
            model.Form.FormWaitingApprovalCount = await Repo.AllReadonly<Form>().IgnoreQueryFilters().Where(f => !f.IsActive && f.ApprovalStatus == (int)ApprovalStatus.Requested).CountAsync();
            model.Form.FormApprovedCount = await Repo.AllReadonly<Form>().IgnoreQueryFilters().Where(f => f.IsActive).CountAsync();          
            return model;
        }
    }
}
