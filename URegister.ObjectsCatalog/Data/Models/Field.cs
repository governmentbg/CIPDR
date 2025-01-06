using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace URegister.ObjectsCatalog.Data.Models
{
    /// <summary>
    /// Полета
    /// </summary>
    [Comment("Полета")]
    [Index(nameof(Name), nameof(Version))]
    [Index(nameof(Name), nameof(IsCurrent))]
    public class Field
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        [Comment("Идентификатор")]
        public Guid Id { get; set; }

        /// <summary>
        /// Име на поле
        /// </summary>
        [MaxLength(50)]
        [Comment("Име на поле")]
        public string? Name { get; set; } = null!;

        /// <summary>
        /// Етикет на поле
        /// </summary>
        [MaxLength(100)]
        [Comment("Етикет на поле")]
        public string? Label { get; set; } = null!;

        /// <summary>
        /// Тип на поле
        /// </summary>
        [Required]
        [Comment("Тип на поле")]
        public int FieldTypeId { get; set; }

        /// <summary>
        /// Информация за полето. Настройки по подразбиране
        /// </summary>
        [Required]
        [Column(TypeName = "jsonb")]
        [Comment("Информация за полето. Настройки по подразбиране")]
        public string FieldData { get; set; } = null!;

        /// <summary>
        /// Версия на полето
        /// </summary>
        [Required]
        [Comment("Версия на полето")]
        public int Version { get; set; }

        /// <summary>
        /// Дали полето е последна версия
        /// </summary>
        [Required]
        [Comment("Дали полето е последна версия")]
        public bool IsCurrent { get; set; } = true;

        /// <summary>
        /// Тип на полето
        /// </summary>
        [ForeignKey(nameof(FieldTypeId))]
        public FieldType FieldType { get; set; } = null!;
    }
}
