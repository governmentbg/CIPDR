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
using URegister.Core.Data.Models.Register;
using URegister.Infrastructure.Constants;

namespace URegister.Core.Data.Models.Process
{
    /// <summary>
    /// Процеси
    /// </summary>
    [Comment("Процеси")]
    public class Process
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        [Comment("Идентификатор")]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Входящ номер
        /// </summary>
        [Required]
        [MaxLength(20)]
        [Comment("Входящ номер")]
        public string IncomingNumber { get; set; } = null!;

        /// <summary>
        /// Номер на вписване 
        /// </summary>
        [Comment("Номер на вписване ")]
        public string? RegisterNumber { get; set; }

        /// <summary>
        /// Дата на входиране
        /// </summary>
        [Comment("Дата на входиране")]
        [Column(TypeName = AttributeConstants.Timestamptz)]
        public DateTime IncomingDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Идентификатор на услуга
        /// </summary>
        [Comment("Идентификатор на услуга")]
        public int ServiceId { get; set; }

        /// <summary>
        /// Идентификатор на стъпка вписване
        /// </summary>
        [Comment("Идентификатор на стъпка вписване")]
        public Guid? RegisteredStepId { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        public int StatusId { get; set; }

        /// <summary>
        /// Идентификатор на MasterPersonIndex
        /// </summary>
        [Comment("Идентификатор на MasterPersonIndex")]
        public Guid MpriId { get; set; }

        /// <summary>
        /// Идентификатор на администрация
        /// </summary>
        [Comment("Идентификатор на администрация")]
        public Guid TennantId { get; set; }

        /// <summary>
        /// Администрация
        /// </summary>
        [ForeignKey(nameof(TennantId))]
        public Administration Administration { get; set; } = null!;

        /// <summary>
        /// услуга
        /// </summary>
        [ForeignKey(nameof(ServiceId))]
        public Service Service { get; set; } = null!;


        public List<ProcessStep> ProcessSteps { get; set; } = new();
    }
}
