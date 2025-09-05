using URegister.Infrastructure.Model.RegisterForms;

namespace URegister.Core.Models.Process
{
    public class ProcessStepVM : FormViewModel
    {
        public Guid ProcessId { get; set; }

        public Guid? FromProcessId { get; set; }
        public int ServiceStepId { get; set; }

        public int ServiceId { get; set; }

        public string? IncomingNumber { get; set; }
        public DateTime? IncomingDate { get; set; }

        public Guid? FileId { get; set; }

        /// <summary>
        /// Поредност
        /// </summary>
        public int OrderNum { get; set; }

        public ProcessInfoVM ProcessInfo { get; set; } = new();
    }
}
