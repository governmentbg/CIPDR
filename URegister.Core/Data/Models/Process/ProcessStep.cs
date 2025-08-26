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
using URegister.Infrastructure.Data.Common;
namespace URegister.Core.Data.Models.Process
{
    /// <summary>
    /// Стъпки към процес
    /// </summary>
    [Comment("Стъпки към процес")]
    public class ProcessStep : EntityBaseWithLastModifiedInfo
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
        /// Статус при съгласуване
        /// </summary>
        [Comment("Статус при съгласуване")]

        public int CoordinationStatusId { get; set; }
        /// <summary>
        /// Мотиви при съгласуване
        /// </summary>
        [Comment("Мотиви при съгласуване")]
        public string? CoordinationMotive { get; set; }

        /// <summary>
        /// Информация за стъпка
        /// </summary>
        [Required]
        [Column(TypeName = AttributeConstants.Jsonb)]
        [Comment("Информация за стъпка")]
        public string StepData { get; set; } = null!;

        /// <summary>
        /// Процес
        /// </summary>
        [ForeignKey(nameof(ProcessId))]
        public Process Process { get; set; } = null!;

        /// <summary>
        /// Стъпка
        /// </summary>
        [ForeignKey(nameof(ServiceStepId))]
        public ServiceStep ServiceStep { get; set; } = null!;
    }
}
