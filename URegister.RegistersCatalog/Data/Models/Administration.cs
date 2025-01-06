using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using URegister.Infrastructure.Data.Common;

namespace URegister.RegistersCatalog.Data.Models
{
    /// <summary>
    /// Администрации
    /// </summary>
    [Comment("Администрации")]
    [Index(nameof(Uic), IsUnique = true)]
    public class Administration : SoftDeletable
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        [Comment("Идентификатор")]
        public Guid Id { get; set; } = Guid.NewGuid();

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
        /// Дата на създаване
        /// </summary>
        [Required]
        [Comment("Дата на създаване")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Лица от администрацията
        /// </summary>
        public List<AdministrationPerson> People { get; set; } = new List<AdministrationPerson>();
    }
}
