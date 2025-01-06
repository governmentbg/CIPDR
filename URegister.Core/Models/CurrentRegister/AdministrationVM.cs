using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using URegister.Infrastructure.Constants;

namespace URegister.Core.Models.CurrentRegister
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

        /// <summary>
        /// Правно основание
        /// </summary>
        [Required]
        [MaxLength(1000)]
        [Display(Name = "Правно основание")]
        public string LegalBasis { get; set; } = null!;


        /// <summary>
        /// Ръководител
        /// </summary>
        public PersonVM Manager { get; set; } = new PersonVM
        {
            Type = PersonTypeValue.Manager,
        };

    }
}
