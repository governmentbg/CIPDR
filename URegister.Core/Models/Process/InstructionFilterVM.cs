using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URegister.Core.Models.Process
{
    /// <summary>
    /// Модел на филтър за указания
    /// </summary>
    public class InstructionFilterVM
    {
        /// <summary>
        /// Идентифицато на процес
        /// </summary>
        public Guid ProcessId { get; set; }

        public string ProcessLabel { get; set; } = null!;

        public bool CanAdd { get; set; }

    }
}
