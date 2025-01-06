using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace URegister.NumberGenerator.Data.Models
{
    /// <summary>
    /// Архив на номератора
    /// </summary>
    [Index(nameof(Number), IsUnique = true)]
    [Comment("Архив на номератора")]
    public class NumberArchive
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        [Comment("Идентификатор")]
        public Guid Id { get; set; }

        /// <summary>
        /// Код на регистър
        /// </summary>
        [Required]
        [MaxLength(10)]
        [Comment("Код на регистър")]
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
        [Comment("Идентификатор на инициращият документ")]
        public Guid InitialDocumentId { get; set; }
    }
}
