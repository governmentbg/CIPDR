using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using URegister.Infrastructure.Data.Common;

namespace URegister.RegistersCatalog.Data.Models
{
    /// <summary>
    /// Лица от администрацията
    /// </summary>
    [Comment("Лица от администрацията")]
    public class AdministrationPerson : SoftDeletable
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        [Comment("Идентификатор")]
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на администрация
        /// </summary>
        [Required]
        [Comment("Идентификатор на администрация")]
        public Guid AdministrationId { get; set; }

        /// <summary>
        /// Тип лице
        /// </summary>
        [Required]
        [Comment("Тип лице")]
        public string Type { get; set; } = null!;

        /// <summary>
        /// Име
        /// </summary>
        [Required]
        [Comment("Име")]
        [MaxLength(100)]
        public string FirstName { get; set; } = null!;

        /// <summary>
        /// Презиме
        /// </summary>
        [MaxLength(100)]
        [Comment("Презиме")]
        public string? MiddleName { get; set; }

        /// <summary>
        /// Фамилия
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Comment("Фамилия")]
        public string LastName { get; set; } = null!;

        /// <summary>
        /// Длъжност
        /// </summary>
        [MaxLength(100)]
        [Comment("Длъжност")]
        public string? Position { get; set; }

        /// <summary>
        /// Телефон
        /// Задължителен при тип лице "1" - контактно лице
        /// </summary>
        [MaxLength(20)]
        [Comment("Телефон")]
        public string? Phone { get; set; }

        /// <summary>
        /// Имейл
        /// Задължителен при тип лице "1" - контактно лице
        /// </summary>
        [MaxLength(100)]
        [Comment("Имейл")]
        public string? Email { get; set; }

        /// <summary>
        /// Дата на създаване
        /// </summary>
        [Required]
        [Comment("Дата на създаване")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Идентификатор на регистър
        /// </summary>
        [Required]
        [Comment("Идентификатор на регистър")]
        public int? RegisterId { get; set; }


        /// <summary>
        /// Администрация
        /// </summary>
        [ForeignKey(nameof(AdministrationId))]
        public Administration Administration { get; set; } = null!;

        /// <summary>
        /// Регистър
        /// </summary>
        [ForeignKey(nameof(RegisterId))]
        public Register Register { get; set; } = null!;
    }
}
