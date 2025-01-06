using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace URegister.Core.Models.Register
{
    /// <summary>
    /// Администрация на регистър
    /// </summary>
    public class AdministrationVM
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ЕИК
        /// </summary>
        [Display(Name = "ЕИК/БУЛСТАТ")]
        [Required]
        [MaxLength(16)]
        public string Uic { get; set; } = null!;

        /// <summary>
        /// Име
        /// </summary>
        [Display(Name = "Име")]
        [Required]
        [MaxLength(500)]
        public string Name { get; set; } = null!;
    }
}
