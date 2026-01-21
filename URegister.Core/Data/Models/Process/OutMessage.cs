using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URegister.Core.Data.Models.Process
{
    public class OutMessage
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        [Comment("Идентификатор")]
        public Guid Id { get; set; } = Guid.NewGuid();


        /// <summary>
        /// Вид съобщение
        /// </summary>
        [Comment("Вид съобщение")]
        public int MessageTypeId { get; set; }

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
        /// Идентификатор на администрация
        /// </summary>
        [Comment("Идентификатор на администрация")]
        public Guid? TenantId { get; set; }

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
        /// Текст на съобщението при изпращане
        /// </summary>
        [Comment("Текст на съобщението при изпращане")]
        public string? MessageText { get; set; }

        /// <summary>
        /// Subject на съобщението при изпращане
        /// </summary>
        [Comment("Subject на съобщението при изпращане")]
        public string? SubjectText { get; set; }

        /// <summary>
        /// Брой повторения при грешно изпращане
        /// </summary>
        [Comment("Брой повторения при грешно изпращане")]
        public int ErrorCountSend { get; set; }

        /// <summary>
        /// Идентификатор на получател
        /// </summary>
        [Comment("Идентификатор на получател")]
        public string? Pid { get; set; }

        /// <summary>
        /// Тип идентификатор на получател
        /// </summary>
        [Comment("Тип идентификатор на получател")]
        public string? PidType { get; set; }

        /// <summary>
        /// Начини на предоставяне на резултата"
        /// </summary>
        [MaxLength(11)]
        [Comment("Начини на предоставяне на резултата")]
        public string? DeliveryMethod { get; set; }
    }
}
