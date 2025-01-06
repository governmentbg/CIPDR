using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace URegister.ObjectsCatalog.Data.Models
{
    /// <summary>
    /// Типове полета
    /// </summary>
    [Comment("Типове полета")]
    [Index(nameof(Name), IsUnique = true)]
    public class FieldType
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
        /// Дали полето е комплексно
        /// </summary>
        [Required]
        [Comment("Дали полето е комплексно")]
        public bool IsComplexField { get; set; }

        /// <summary>
        /// Шаблон за визуализация
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Comment("Шаблон за визуализация")]
        public string Template { get; set; } = null!;
    }
}
