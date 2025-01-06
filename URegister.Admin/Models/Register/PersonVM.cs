using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace URegister.Admin.Models.Register
{
    /// <summary>
    /// Лица от администрацията
    /// </summary>
    public class PersonVM
    {
        // <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Тип лице
        /// </summary>
        [Required]
        [Display(Name = "Тип лице")]
        public string Type { get; set; } = null!;

        /// <summary>
        /// Име
        /// </summary>
        [Required]
        [Display(Name = "Име")]
        [MaxLength(100)]
        public string FirstName { get; set; } = null!;

        /// <summary>
        /// Презиме
        /// </summary>
        [MaxLength(100)]
        [Display(Name = "Презиме")]
        public string? MiddleName { get; set; }

        /// <summary>
        /// Фамилия
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; } = null!;

        /// <summary>
        /// Длъжност
        /// </summary>
        [MaxLength(100)]
        [Display(Name = "Длъжност")]
        public string? Position { get; set; }

        /// <summary>
        /// Телефон
        /// Задължителен при тип лице "1" - контактно лице
        /// </summary>
        [MaxLength(20)]
        [Display(Name = "Телефон")]
        public string? Phone { get; set; }

        /// <summary>
        /// Имейл
        /// Задължителен при тип лице "1" - контактно лице
        /// </summary>
        [MaxLength(100)]
        [Display(Name = "Имейл")]
        public string? Email { get; set; }
        /// <summary>
        /// Индекс за тамплейт
        /// </summary>
        public int Index { get; set; }
    }
}
