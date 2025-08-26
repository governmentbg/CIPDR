using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Infrastructure.Data.Common;

namespace URegister.Core.Data.Models.Process
{
    /// <summary>
    /// Указания
    /// </summary>
    [Comment("Указания")]
    public class Instruction : EntityBaseWithLastModifiedInfo
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        [Comment("Идентификатор")]
        public Guid Id { get; set; } = Guid.NewGuid();


        /// <summary>
        /// Идентификатор на процес
        /// </summary>
        [Comment("Идентификатор на процес")]
        public Guid ProcessId { get; set; }

        /// <summary>
        /// Процес
        /// </summary>
        [ForeignKey(nameof(ProcessId))]
        public Process Process { get; set; } = null!;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public DateTime? ClosedOn { get; set; }


        public DateTime DateTo { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Съдържание
        /// </summary>
        [Comment("Съдържание")]
        public string Content { get; set; } = null!;



        /// <summary>
        /// Записани полета
        /// </summary>
        [Comment("Записани полета")]
        public List<InstructionResponse> InstructionResponses { get; set; } = new();
    }
}
