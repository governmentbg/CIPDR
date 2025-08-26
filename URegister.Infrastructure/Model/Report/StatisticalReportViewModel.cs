using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URegister.Infrastructure.Model.Report
{
    public class StatisticalReportViewModel
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }
        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }
        public int TotalSentProcesses { get; set; }
        public int SentCounterProcesses { get; set; }
        public int SentOnlineProcesses { get; set; }
        public int TotalRegisteredProcesses { get; set; }
        public int RegisteredProcessesInitialRegistrations { get; set; }
        public int RegisteredProcessesCircumstanceChanges { get; set; }
        public int RegisteredProcessesDeletions { get; set; }
        public int TechnicalErrorsFixed { get; set; }
        public int Rejections { get; set; }
        public int Instructions { get; set; }
        public int IssuedCertificates { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int CreatedUsers { get; set; }
        public string? RegisterTypeEntry { get; set; }
    }
}
