using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Core.Models.Common;
using URegister.Infrastructure.Constants;

namespace URegister.Core.Models.Process
{
    /// <summary>
    /// Допълнителни данни за заявена услуга
    /// </summary>
    public class ProcessInfoVM
    {
        /// <summary>
        /// Начин на получаване на заявлението
        /// </summary>
        [Display(Name = "Начин на получаване на заявлението")]
        public string? ReceivedChannelId { get; set; } = ChannelType.OnDesk;

        /// <summary>
        /// Начин на получаване на резултат от услуга
        /// </summary>
        [Display(Name = "Начин на получаване на резултат от услуга")]
        public string? PreferredResultDeliveryMethod { get; set; }

        /// <summary>
        /// Вид срок за изпълнение на услуга
        /// </summary>
        [Display(Name = "Вид срок за изпълнение на услуга")]
        public int DeadlineId { get; set; }

        /// <summary>
        /// Срок за изпълнение на услуга/дни
        /// </summary>
        [Display(Name = "Срок за изпълнение на услуга/дни")]
        public int DeadlineDay { get; set; }

        /// <summary>
        /// Срок за изпълнение на услуга
        /// </summary>
        [Display(Name = "Срок за изпълнение на услуга")]
        public DateTime? DeadlineDate { get; set; }

        /// <summary>
        /// Стар номер на вписване 
        /// </summary>
        [Display(Name = "Стар номер на вписване")]
        public string? OldIncomingNumber { get; set; }

        /// <summary>
        /// Стара дата на входиране
        /// </summary>
        [Display(Name = "Стара дата на входиране")]
        public DateTime? OldIncomingDate { get; set; }
        
        public int ServiceStepId { get; set; }

        [Display(Name = "Решение")]
        public int CoordinationStatusId { get; set; }

        [Display(Name = "Мотиви")]
        public string? CoordinationMotive { get; set; }

        public FileVM? EFormFile { get; set; }

    }
}
