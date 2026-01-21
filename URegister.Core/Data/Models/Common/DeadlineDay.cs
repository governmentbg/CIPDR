using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using URegister.Infrastructure.Data.Common;

namespace URegister.Core.Data.Models.Common
{
    /// <summary>
    /// Срокове за изпълнение на услуга
    /// </summary>
    [Comment("Срокове за изпълнение на услуга")]
    public class DeadlineDay : EntityBaseWithLastModifiedInfo
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Comment("Идентификатор")]
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на услуга
        /// </summary>
        public int? ServiceId { get; set; }

        /// <summary>
        /// Вид срок за изпълнение на услуга
        /// </summary>
        [Comment("Вид срок за изпълнение на услуга")]
        [MaxLength(11)]
        public string DeadlineTypeId { get; set; } = null!;

        /// <summary>
        /// Работни/календарни дни
        /// </summary>
        [Comment("Работни/календарни дни")]
        [MaxLength(11)]
        public string DayTypeId { get; set; } = string.Empty!;


        /// <summary>
        /// Срок за изпълнение на услуга/дни
        /// </summary>
        [Comment("Срок за изпълнение на услуга/дни")]
        public int Days { get; set; }
        public int FormParentId { get; set; }

        /// <summary>
        /// услуга
        /// </summary>
        [ForeignKey(nameof(ServiceId))]
        public Service? Service { get; set; }
    }
}
