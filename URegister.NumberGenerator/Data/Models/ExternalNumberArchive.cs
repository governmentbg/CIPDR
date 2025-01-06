using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace URegister.NumberGenerator.Data.Models
{
    /// <summary>
    /// Архив на номератора за външни системи
    /// </summary>
    [Index(nameof(Number), IsUnique = true)]
    [Comment("Архив на номератора за външни системи")]
    public class ExternalNumberArchive
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        [Comment("Идентификатор")]
        public Guid Id { get; set; }

        /// <summary>
        /// Име на регистър
        /// </summary>
        [Required]
        [MaxLength(200)]
        [Comment("Име на регистър")]
        public string Register { get; set; } = null!;

        /// <summary>
        /// Номер
        /// </summary>
        [Required]
        [Comment("Номер")]
        public long Number { get; set; }

        /// <summary>
        /// Идентификатор на инициращият документ
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Comment("Идентификатор на инициращият документ")]
        public string InitialDocumentId { get; set; } = null!;
    }
}
