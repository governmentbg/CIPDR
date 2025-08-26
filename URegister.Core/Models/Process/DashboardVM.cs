using Microsoft.AspNetCore.Authentication;
using URegister.Infrastructure.Constants;

namespace URegister.Core.Models.Process
{
    public class DashboardVM
    {
        public int BlanksTemplateCount { get; set; }
        public int CustomViewCount { get; set; }
        public string RegisterBaseURL { get; set; }
        public string CurrentAdministrationName { get; set; }
        public UserDashboardVm Users { get; set; } = new UserDashboardVm();
        public int UserAssignedProcessCount { get; set; }
        public ProcessesDashboardVm Process { get; set; } = new ProcessesDashboardVm();
        public FormDashboardVm Form { get; set; } = new FormDashboardVm();
    }

    public class UserDashboardVm
    {
        public int UsersCount { get; set; }
        public int EnableUsersCount { get; set; }
        public int DisableUsersCount { get; set; }
        public string AdminisrationName { get; set; }
    }

    public class ProcessesDashboardVm
    {
        public int ProcessAllCount { get; set; }
        public int ProcessSentCount { get; set; }
        public int ProcessInProgressCount { get; set; }
        public int ProcessRegisteredCount { get; set; }
        public int ProcessRefusedCount { get; set; }
        public int ProcessCertificateCount { get; set; }
    }

    public class FormDashboardVm
    {
        public int FormAllCount { get; set; }
        public int FormWaitingApprovalCount { get; set; }
        public int FormApprovedCount { get; set; }             
    }
}
