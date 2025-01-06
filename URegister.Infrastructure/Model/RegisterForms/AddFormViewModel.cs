using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Infrastructure.Constants;
using URegister.Infrastructure.Constants;

namespace URegister.Infrastructure.Model.RegisterForms
{
    /// <summary>
    /// Модел за дизайнера на форми
    /// </summary>
    public class AddFormViewModel
    {
        /// <summary>
        /// Идентификатор на формата
        /// </summary>
        public int ParentId { get; set; }

        /// <summary>
        /// Име на форма
        /// </summary>
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [RegularExpression(RegexPatterns.CyrillicTextPattern, ErrorMessage = MessageConstant.NotCyrillic)]
        [DisplayName("Име")]
        [StringLength(150, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        public string? FormTitle { get; set; }

        /// <summary>
        /// Предназначение на форма
        /// </summary>
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [RegularExpression(RegexPatterns.CyrillicTextPattern, ErrorMessage = MessageConstant.NotCyrillic)]
        [DisplayName("Предназначение")]
        [StringLength(250, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        public string? Purpose { get; set; }
    }
}
