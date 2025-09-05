using System.ComponentModel.DataAnnotations;
using URegister.Core.Validation;
using URegister.Infrastructure.Constants;

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
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [MaxLength(16, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [UrEik(ErrorMessage = "Въведете валиден ЕИК/БУЛСТАТ")]
        public string Uic
        {
            get => _uic;
            set => _uic = value?.Trim();
        }
        private string _uic = null!;

        /// <summary>
        /// Наименование на административния орган
        /// </summary>
        [Display(Name = "Наименование на административния орган")]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [MaxLength(500, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [RegularExpression(RegexPatterns.CyrillicTextPattern, ErrorMessage = MessageConstant.NotCyrillic)]
        public string Name
        {
            get => _name;
            set => _name = value?.Trim();
        }
        private string _name = null!;
    }
}