using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Data.Common;

namespace URegister.Core.Data.Models.Process
{
    /// <summary>
    /// Отговори на Указания
    /// </summary>
    [Comment("Отговори на Указания")]
    public class InstructionResponse : EntityBaseWithLastModifiedInfo
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
        public Guid InstructionId { get; set; }

        /// <summary>
        /// Процес
        /// </summary>
        [ForeignKey(nameof(InstructionId))]
        public Instruction Instruction { get; set; } = null!;

        /// <summary>
        /// Съдържание
        /// </summary>
        [Comment("Съдържание")]
        public string? Content { get; set; }

        /// <summary>
        /// Прието от
        /// </summary>
        [Comment("Прието от")]
        public Guid? ReceivedBy { get; set; }

        // <summary>
        /// Дата на приемане
        /// </summary>
        [Comment("Дата на приемане")]
        [Column(TypeName = AttributeConstants.Timestamptz)]
        public DateTime? ReceivedOn { get; set; } 
    }
}
