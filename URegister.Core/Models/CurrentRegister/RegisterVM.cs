using System.ComponentModel.DataAnnotations;
using URegister.Infrastructure.Constants;

namespace URegister.Core.Models.CurrentRegister
{
    public class RegisterVM
    {
        // <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Код на регистър
        /// </summary>
        [Required]
        [MaxLength(10)]
        [Display(Name = "Код на регистър")]
        public string Code { get; set; } = null!;

        /// <summary>
        /// Име на регистър
        /// </summary>
        [Required]
        [MaxLength(500)]
        [Display(Name = "Име на регистър")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Описание
        /// </summary>
        [MaxLength(1000)]
        [Display(Name = "Описание")]
        public string? Description { get; set; }

        /// <summary>
        /// Правно основание
        /// </summary>
        [Required]
        [MaxLength(1000)]
        [Display(Name = "Правно основание")]
        public string LegalBasis { get; set; } = null!;

        /// <summary>
        /// Вид на регистъра
        /// </summary>
        [Required]
        [MaxLength(5)]
        [Display(Name = "Вид на регистъра")]
        public string Type { get; set; } = null!;

        /// <summary>
        /// Ниво на осигуреност на средствата за електронна идентификация
        /// </summary>
        [Required]
        [MaxLength(5)]
        [Display(Name = "Ниво на осигуреност на средствата за електронна идентификация")]
        public string IdentitySecurityLevel { get; set; } = null!;


        /// <summary>
        /// Начин на вписване
        /// </summary>
        [Required]
        [MaxLength(5)]
        [Display(Name = "Начин на вписване")]
        public string TypeEntry { get; set; } = null!;
    }
}
