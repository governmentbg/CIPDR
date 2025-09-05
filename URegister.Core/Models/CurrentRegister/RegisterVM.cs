using System.ComponentModel.DataAnnotations;
using URegister.Infrastructure.Constants;

namespace URegister.Core.Models.CurrentRegister
{
    public class RegisterVM
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Код на регистър
        /// </summary>
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [MaxLength(10, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [Display(Name = "Код на регистър")]
        public string Code { get; set; } = null!;

        /// <summary>
        /// Име на регистър
        /// </summary>
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [MaxLength(500, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [Display(Name = "Име на регистър")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Описание
        /// </summary>
        [MaxLength(1000, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [Display(Name = "Описание")]
        public string? Description { get; set; }

        /// <summary>
        /// Правно основание
        /// </summary>
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [MaxLength(1000, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [Display(Name = "Правно основание")]
        //[RegularExpression(RegexPatterns.CyrillicTextPattern, ErrorMessage = MessageConstant.NotCyrillic)]
        public string LegalBasis { get; set; } = null!;

        /// <summary>
        /// Вид на регистъра
        /// </summary>
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [MaxLength(5, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [Display(Name = "Вид на регистъра")]
        public string Type { get; set; } = null!;

        /// <summary>
        /// Ниво на осигуреност на електронната идентификация при заявяване на ЕАУ
        /// </summary>
        // [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [MaxLength(5, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [Display(Name = "Ниво на осигуреност на електронната идентификация при заявяване на ЕАУ")]
        public string? IdentitySecurityLevel { get; set; }


        /// <summary>
        /// Начин на вписване
        /// </summary>
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [MaxLength(5, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [Display(Name = "Начин на вписване")]
        public string TypeEntry { get; set; } = null!;
    }
}
