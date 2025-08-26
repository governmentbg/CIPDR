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
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [MaxLength(16, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        public string Uic { get; set; } = null!;

        /// <summary>
        /// Наименование на административния орган
        /// </summary>
        [Display(Name = "Наименование на административния орган")]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [MaxLength(500, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [RegularExpression(RegexPatterns.CyrillicPersonNamePattern, ErrorMessage = MessageConstant.NotCyrillic)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Правно основание
        /// </summary>
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [MaxLength(1000, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [Display(Name = "Правно основание")]
        //[RegularExpression(RegexPatterns.CyrillicTextPattern, ErrorMessage = MessageConstant.NotCyrillic)]
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
