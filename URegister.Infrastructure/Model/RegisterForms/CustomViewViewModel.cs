using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using URegister.Infrastructure.Constants;

namespace URegister.Infrastructure.Model.RegisterForms
{
    /// <summary>
    /// Модел за справка
    /// </summary>
    public class CustomViewViewModel
    {
        /// <summary>
        /// Идентификатор на справка
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Име на справка
        /// </summary>
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [RegularExpression(RegexPatterns.CyrillicTextPattern, ErrorMessage = MessageConstant.NotCyrillic)]
        [DisplayName("Име")]
        [StringLength(150, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        public string? CustomViewTitle { get; set; }

        /// <summary>
        /// Избрани колони
        /// </summary>
        [DisplayName("Колони в справката")]
        public List<string> SelectedColumns { get; set; }
    }
}
