using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Core.Data.Models.Common;
using URegister.Infrastructure.Constants;
namespace URegister.Core.Data.Models.Process
{
    /// <summary>
    /// Стъпки към процес
    /// </summary>
    [Comment("Стъпки към процес")]
    public class ProcessStep
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
        /// Идентификатор на стъпка
        /// </summary>
        [Comment("Идентификатор на стъпка")]
        public int ServiceStepId { get; set; }

        /// <summary>
        /// Поредност на стъпка
        /// </summary>
        [Comment("Поредност на стъпка")]
        public int OrderNum { get; set; }

        /// <summary>
        /// Идентификатор на форма
        /// </summary>
        [Comment("Идентификатор на форма")]
        public int FormId { get; set; }

        /// <summary>
        /// Информация за стъпка
        /// </summary>
        [Required]
        [Column(TypeName = "jsonb")]
        [Comment("Информация за стъпка")]
        public string StepData { get; set; } = null!;

        /// <summary>
        /// Процес
        /// </summary>
        [ForeignKey(nameof(ProcessId))]
        public Process Process { get; set; } = null!;

        /// <summary>
        /// Форма
        /// </summary>
        [ForeignKey(nameof(FormId))]
        public Form Form { get; set; } = null!;

        /// <summary>
        /// Стъпка
        /// </summary>
        [ForeignKey(nameof(ServiceStepId))]
        public ServiceStep ServiceStep { get; set; } = null!;
    }
}
