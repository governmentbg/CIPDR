using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Core.Models.Common;

namespace URegister.Core.Models.Process
{
    public class InstructionResponseItemVM
    {
        /// <summary>
        /// Идентификатор 
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Идентификатор на указание
        /// </summary>
        public Guid InstructionId { get; set; }

        /// <summary>
        /// Идентификатор на процес
        /// </summary>
        public Guid ProcessId { get; set; }

        /// <summary>
        /// Съдържание на съобщението
        /// </summary>
        [Display(Name = "Съобщение")]
        public string? Content { get; set; }

        /// <summary>
        /// Дата на изпълнение
        /// </summary>
        public DateTime ModifiedOn { get; set; }

        public bool CanEdit { get; set; }

        public List<FileVM> Files { get; set; } = new();
    }
}
