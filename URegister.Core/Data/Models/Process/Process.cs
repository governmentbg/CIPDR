using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using URegister.Core.Data.Models.Common;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Data.Common;

namespace URegister.Core.Data.Models.Process
{
    /// <summary>
    /// Процеси
    /// </summary>
    [Comment("Заявени услуги")]
    public class Process: EntityBaseWithLastModifiedInfo
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        [Comment("Идентификатор")]
        public Guid Id { get; set; } = Guid.NewGuid();


        /// <summary>
        /// Идентификатор на първоначален процес
        /// </summary>
        [Comment("Идентификатор на първоначален процес")]
        public Guid? FromProcessId { get; set; }

        /// <summary>
        /// Входящ номер
        /// </summary>
        [Required]
        [MaxLength(20)]
        [Comment("Входящ номер")]
        public string IncomingNumber { get; set; } = null!;

        /// <summary>
        /// Стар входящ номер
        /// </summary>
        [MaxLength(50)]
        [Comment("Стар входящ номер")]
        public string? OldIncomingNumber { get; set; }

        /// <summary>
        /// Номер на вписване 
        /// </summary>
        [MaxLength(20)]
        [Comment("Номер на вписване ")]
        public string? RegisterNumber { get; set; }


        /// <summary>
        /// Дата на входиране
        /// </summary>
        [Comment("Дата на входиране")]
        [Column(TypeName = AttributeConstants.Timestamptz)]
        public DateTime IncomingDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Стара дата на входиране
        /// </summary>
        [Comment("Стара дата на входиране")]
        [Column(TypeName = AttributeConstants.Timestamptz)]
        public DateTime? OldIncomingDate { get; set; }

        /// <summary>
        /// Дата на вписване
        /// </summary>
        [Comment("Дата на вписване")]
        [Column(TypeName = AttributeConstants.Timestamptz)]
        public DateTime? RegisterDate { get; set; }

        /// <summary>
        /// Дата на пурвоначално вписване
        /// </summary>
        [Comment("Дата на първоначално вписване")]
        [Column(TypeName = AttributeConstants.Timestamptz)]
        public DateTime? RegisterInitDate { get; set; }

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
        [Comment("Статус")]
        public int StatusId { get; set; }

        /// <summary>
        /// Идентификатор на партида в MasterPersonIndex
        /// </summary>
        [Comment("Идентификатор на партида в MasterPersonIndex")]
        public Guid MpriId { get; set; }

        /// <summary>
        /// Идентификатор на заявител в MasterPersonIndex
        /// </summary>
        [Comment("Идентификатор на заявител в MasterPersonIndex")]
        public Guid MpriApplicantId { get; set; }

        /// <summary>
        /// Идентификатор на администрация
        /// </summary>
        [Comment("Идентификатор на администрация")]
        public Guid TenantId { get; set; }

        /// <summary>
        /// Идентификатор на стъпка
        /// </summary>
        [Comment("Идентификатор на стъпка")]
        public int? LastServiceStepId { get; set; }

        /// <summary>
        /// Идентификатор на форма
        /// </summary>
        [Comment("Идентификатор на форма")]
        public int FormId { get; set; }

        /// <summary>
        /// Поредност на вписването
        /// </summary>
        [Comment("Поредност на вписването")]
        public long OrderNumber { get; set; }

        /// <summary>
        /// Записани полета
        /// </summary>
        [Comment("Причина за прекратяване")]
        [StringLength(1000)]
        public string? ReasonForRejection { get; set; } = null;

        /// <summary>
        /// Номер на отказ
        /// </summary>
        [MaxLength(20)]
        [Comment("Номер на отказ")]
        public string? RejectionNumber { get; set; } = null!;

        /// <summary>
        /// Номер на удостоверение при вписване
        /// </summary>
        [MaxLength(20)]
        [Comment("Номер на удостоверение при вписване")]
        public string? RegisterCertificateNumber { get; set; } = null!;

        ///<summary> 
        ///Потребител, на който е присвоена услугата
        ///</summary>
        [Comment("Потребител, на който е присвоена услугата")]
        public Guid? AssignedToUser { get; set; }

        /// <summary>
        /// Начини на предоставяне на резултата"
        /// </summary>
        [MaxLength(11)]
        [Comment("Начини на предоставяне на резултата")]
        public string? PreferredResultDeliveryMethod { get; set; } 

        /// <summary>
        /// Номер на заявена услуга при импорт от е-форма
        /// </summary>
        [Comment("Номер на заявена услуга при импорт от е-форма")]
        public Guid? EFormRegisteredServiceNumber { get; set; } = null;

        /// <summary>
        /// Начин на получаване на заявлението
        /// </summary>
        [Comment("Начин на получаване на заявлението")]
        public string? ReceivedChannelId { get; set; }

        /// <summary>
        /// Вид срок за изпълнение на услуга
        /// </summary>
        [Comment("Вид срок за изпълнение на услуга")]
        public int DeadlineId { get; set; }

        /// <summary>
        /// Срок за изпълнение на услуга/дни
        /// </summary>
        [Comment("Срок за изпълнение на услуга/дни")]
        public int DeadlineDay { get; set; }

        /// <summary>
        /// Срок за изпълнение на услуга
        /// </summary>
        [Comment("Срок за изпълнение на услуга")]
        public DateTime? DeadlineDate { get; set; }

        /// <summary>
        /// Нотификация за настъпващ срок за изпълнение на услуга
        /// </summary>
        [Comment("Нотификация за настъпващ срок за изпълнение на услуга")]
        public bool IsSendEMailDeadlineDate { get; set; }

        #region ForeignKey

        /// <summary>
        /// услуга
        /// </summary>
        [ForeignKey(nameof(ServiceId))]
        public Service Service { get; set; } = null!;

        /// <summary>
        /// Форма
        /// </summary>
        [ForeignKey(nameof(FormId))]
        public Form Form { get; set; } = null!;

        /// <summary>
        /// Стъпка
        /// </summary>
        [ForeignKey(nameof(LastServiceStepId))]
        public ServiceStep LastServiceStep { get; set; } = null!;


        /// <summary>
        /// Стъпки на заявената услуга
        /// </summary>
        [Comment("Стъпки на заявената услуга")]
        public List<ProcessStep> ProcessSteps { get; set; } = new();


        /// <summary>
        /// Указания
        /// </summary>
        [Comment("Указания")]
        public List<Instruction> Instructions { get; set; } = new();


        /// <summary>
        /// Първоначален процес
        /// </summary>
        [Comment("Първоначален процес")]
        [ForeignKey(nameof(FromProcessId))]
        public Process? FromProcess { get; set; }

        /// <summary>
        /// Промени и заличавания
        /// </summary>
        [Comment("Промени и заличавания")]
        public List<Process> ChangeProcesses { get; set; } = new();

        /// <summary>
        /// Записани полета
        /// </summary>
        [Comment("Записани полета")]
        public List<RegisterItem> RegisterItems { get; set; } = new();

        /// <summary>
        /// Връчвания
        /// </summary>
        [Comment("Връчвания към заявената услуга")]
        public List<ProcessDelivery> ProcessDeliveries { get; set; } = new();

        /// <summary>
        /// Прикачени файлове
        /// </summary>
        [Comment("Връчвания към заявената услуга")]
        public List<FileMetadata> FileMetadataList { get; set; } = new();
        #endregion
    }
}
