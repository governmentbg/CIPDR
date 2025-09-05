using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URegister.Core.Models.Process
{
    public class InstructionResponseVM
    {
        public Guid GetProcessId()
        {
            return Items.FirstOrDefault()?.ProcessId ?? Guid.Empty;
        }
        public List<InstructionResponseItemVM> Items { get; set; } = new();
    }
}
