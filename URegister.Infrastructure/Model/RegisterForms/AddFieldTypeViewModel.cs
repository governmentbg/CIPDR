using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using URegister.Infrastructure.Constants;

namespace URegister.Infrastructure.Model.RegisterForms
{
    /// <summary>
    /// Модел за дизайнера на форми
    /// </summary>
    public class AddFieldTypeViewModel
    {
        /// <summary>
        /// Име на тип на поле
        /// </summary>
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [RegularExpression(RegexPatterns.CyrillicPersonNamePattern, ErrorMessage = MessageConstant.NotCyrillic)]
        [DisplayName("Име")]
        [StringLength(50, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        public string? Label { get; set; }

        /// <summary>
        /// Тип на поле в базата данни на английски език
        /// </summary>
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [RegularExpression(RegexPatterns.LatinTextWithNumbersPattern, ErrorMessage = MessageConstant.NotLatin)]
        [DisplayName("Тип (на английски език)")]
        [StringLength(50, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        public string? Type { get; set; }

        /// <summary>
        /// Идентификатор на тип поле
        /// </summary>
        public int Id { get; set; }

        [DisplayName("Само за следните регистри")]
        public List<string>? RegisterRestrictionCodes { get; set; }
    }
}
