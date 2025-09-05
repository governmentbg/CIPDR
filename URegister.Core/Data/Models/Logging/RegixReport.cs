using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using URegister.Infrastructure.Constants;

namespace URegister.Core.Data.Models.Logging
{
    /// <summary>
    /// Комуникация с Regix
    /// </summary>
    [Comment("Комуникация с Regix")]
    public class RegixReport
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        [Comment(AttributeConstants.Identifier)]
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на потребител
        /// </summary>
        [Required]
        [Comment("Идентификатор на потребител")]
        public Guid UserId { get; set; }

        /// <summary>
        /// Дата на събитието
        /// </summary>
        [Required]
        [Column(TypeName = AttributeConstants.Timestamptz)]
        [Comment("Дата на събитието")]
        public DateTime EventDate { get; set; }

        /// <summary>
        /// Guid от Regix
        /// </summary>
        [Comment("Guid от Regix")]
        public Guid? RegixGuid { get; set; }

        /// <summary>
        /// Съдържание на заявка
        /// </summary>
        [Comment("Съдържание на заявка")]
        [Required]
        public required string RequestData { get; set; }

        /// <summary>
        /// Съдържание на отговор
        /// </summary>
        [Comment("Съдържание на отговор")]
        [Required]
        [Column(TypeName = AttributeConstants.Jsonb)]
        public required string ResponseData { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        [Comment("Описание")]
        public string? Description { get; set; }

        /// <summary>
        /// Номенклатурна стойност на тип заявка
        /// </summary>
        [Required]
        [StringLength(3)]
        [Comment("Номенклатурна стойност на тип заявка")]
        public string RegixRequestType { get; set; }
    }
}
