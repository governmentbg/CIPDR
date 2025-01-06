using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using URegister.Infrastructure.Data.Common;

namespace URegister.Core.Data.Models.Register
{
    /// <summary>
    /// Администрации
    /// </summary>
    [Comment("Администрации")]
    public class Administration : SoftDeletable
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        [Comment("Идентификатор")]
        public Guid Id { get; set; }

        /// <summary>
        /// Идентификатор на регистър
        /// </summary>
        [Required]
        [Comment("Идентификатор на регистър")]
        public int RegisterId { get; set; }

        /// <summary>
        /// ЕИК
        /// </summary>
        [Comment("ЕИК")]
        [Required]
        [MaxLength(16)]
        public string Uic { get; set; } = null!;

        /// <summary>
        /// Име
        /// </summary>
        [Comment("Име")]
        [Required]
        [MaxLength(500)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Правно основание
        /// </summary>
        [Required]
        [MaxLength(1000)]
        [Comment("Правно основание")]
        public string LegalBasis { get; set; } = null!;

        /// <summary>
        /// Дата на създаване
        /// </summary>
        [Required]
        [Comment("Дата на създаване")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Регистър
        /// </summary>
        [ForeignKey(nameof(RegisterId))]
        public Register? Register { get; set; }

        /// <summary>
        /// Лица от администрацията
        /// </summary>
        public List<AdministrationPerson> People { get; set; } = new List<AdministrationPerson>();
    }
}
