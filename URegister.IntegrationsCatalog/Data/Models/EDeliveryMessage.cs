using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Data.Common;

namespace URegister.IntegrationsCatalog.Data.Models
{
    public class EDeliveryMessage: EntityBaseWithLastModifiedInfo
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        [Comment("Идентификатор")]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Идентификатор на съобщение
        /// </summary>
        [Comment("Идентификатор на съобщение")]
        public int MessageId { get; set; }

        /// <summary>
        /// Вид съобщение
        /// </summary>
        [Comment("Вид съобщение")]
        public int MessageTypeId { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        [Comment("Стъпка")]
        public int StepId { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        [Comment("Статус")]
        public int StatusId { get; set; }

        /// <summary>
        /// Грешка
        /// </summary>
        [Comment("Грешка")]
        public string? ErrorMessage { get; set; }


        /// <summary>
        /// Идентификатор на заявена услуга
        /// </summary>
        [Comment("Идентификатор на заявена услуга, по-която е качен файла")]
        public Guid? ProcessId { get; set; }

        /// <summary>
        /// Вид връзка
        /// </summary>
        [Comment("Вид връзка")]
        public int SourceType { get; set; }

        /// <summary>
        /// Идентификатор на връзка
        /// </summary>
        [Comment("Идентификатор на връзка")]
        public Guid? SourceId { get; set; }

        /// <summary>
        /// Входящ номер на заявена услуга
        /// </summary>
        [Comment("Входящ номер на заявена услуга")]
        public string? IncomingNumber { get; set; }

        /// <summary>
        /// Входяща дата на заявена услуга
        /// </summary>
        [Comment("Входяща дата на заявена услуга")]
        public DateTime? IncomingDate { get; set; }

        /// <summary>
        /// Идентификатор на администрация
        /// </summary>
        [Comment("Идентификатор на администрация")]
        public Guid? TenantId { get; set; }

        /// <summary>
        /// ЕИК на администрация
        /// </summary>
        [Comment("Идентификатор на администрация")]

        public string? AdministrationUic { get; set; }

        /// <summary>
        /// Идентификатор на регистър
        /// </summary>
        [Comment("Идентификатор на регистър")]
        public int? RegisterId { get; set; }

        /// <summary>
        /// Идентификатор на услуга
        /// </summary>
        [Comment("Идентификатор на услуга")]
        public int? ServiceId { get; set; }

        /// <summary>
        /// Информация от съобщениетопри при open
        /// </summary>
        [Column(TypeName = AttributeConstants.Jsonb)]
        [Comment("Информация от съобщението при open")]
        public string? Message { get; set; }

        /// <summary>
        /// Информация от пдф
        /// </summary>
        [Column(TypeName = AttributeConstants.Jsonb)]
        [Comment("Информация от пдф")]
        public string? ApplicationJson { get; set; }

        /// <summary>
        /// Информация от пдф
        /// </summary>
        [Column(TypeName = AttributeConstants.Jsonb)]
        [Comment("Информация от пдф json_submission")]
        public string? ApplicationSubmission { get; set; }


        /// <summary>
        /// Референтен номер на услуга (РНУ)
        /// </summary>
        [Comment("Референтен номер на услуга (РНУ)")]
        public string? Rnu { get; set; }

        /// <summary>
        /// Идентификатор на изходящо съобщение
        /// </summary>
        [Comment("Идентификатор на изходящо съобщение")]
        public Guid? OutboxId { get; set; }

        public ICollection<EDeliveryFileMetadata> EDeliveryFiles { get; set; } = new List<EDeliveryFileMetadata>();
    }
}
