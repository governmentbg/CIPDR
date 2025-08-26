using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using URegister.Infrastructure.Data.Common;

namespace URegister.ObjectsCatalog.Data.Models
{
    /// <summary>
    /// Типове полета
    /// </summary>
    [Comment("Типове полета")]
    [Index(nameof(Name), IsUnique = true)]
    public class FieldType : SoftDeletable
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        [Comment("Идентификатор")]
        public int Id { get; set; }

        /// <summary>
        /// Име на поле
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Comment("Име на поле")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Етикет на поле
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Comment("Етикет на поле")]
        public string Label { get; set; } = null!;

        /// <summary>
        /// Дали полето е сложно
        /// </summary>
        [Required]
        [Comment("Дали полето е сложно")]
        public bool IsComplexField { get; set; }

        /// <summary>
        /// Шаблон за визуализация
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Comment("Шаблон за визуализация")]
        public string Template { get; set; } = null!;

        /// <summary>
        /// Списък от конфигурации за типа
        /// </summary>
        public List<Field> Fields { get; set; } = new List<Field>();
    }
}
