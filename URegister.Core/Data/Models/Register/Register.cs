using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using URegister.Infrastructure.Data.Common;

namespace URegister.Core.Data.Models.Register
{
    /// <summary>
    /// Регистри
    /// </summary>
    [Comment("Регистри")]
    [Index(nameof(Code), IsUnique = true)]
    public class Register : SoftDeletable
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        [Comment("Идентификатор")]
        public int Id { get; set; }

        /// <summary>
        /// Код на регистър
        /// </summary>
        [Required]
        [MaxLength(10)]
        [Comment("Код на регистър")]
        public string Code { get; set; } = null!;

        /// <summary>
        /// Име на регистър
        /// </summary>
        [Required]
        [MaxLength(500)]
        [Comment("Име на регистър")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Описание
        /// </summary>
        [MaxLength(1000)]
        [Comment("Описание")]
        public string? Description { get; set; }

        /// <summary>
        /// Правно основание
        /// </summary>
        [Required]
        [MaxLength(1000)]
        [Comment("Правно основание")]
        public string LegalBasis { get; set; } = null!;

        /// <summary>
        /// Вид на регистъра
        /// </summary>
        [Required]
        [MaxLength(5)]
        [Comment("Вид на регистъра")]
        public string Type { get; set; } = null!;

        /// <summary>
        /// Ниво на осигуреност на средствата за електронна идентификация
        /// </summary>
        [Required]
        [MaxLength(5)]
        [Comment("Ниво на осигуреност на средствата за електронна идентификация")]
        public string IdentitySecurityLevel { get; set; } = null!;


        /// <summary>
        /// Начин на вписване
        /// </summary>
        [Required]
        [MaxLength(5)]
        [Comment("Начин на вписване")]
        public string TypeEntry { get; set; } = null!;

        /// <summary>
        /// Дата на създаване
        /// </summary>
        [Required]
        [Comment("Дата на създаване")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Администрации
        /// </summary>
        public List<Administration> Administrations { get; set; } = new List<Administration>();
    }
}
