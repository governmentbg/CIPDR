using System.ComponentModel.DataAnnotations;
using URegister.Infrastructure.Constants;

namespace URegister.Core.Models.CurrentRegister
{
    /// <summary>
    /// Лица от администрацията
    /// </summary>
    public class PersonVM
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Тип лице
        /// </summary>
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [Display(Name = "Тип лице")]
        public string Type { get; set; } = null!;

        /// <summary>
        /// Тип лице
        /// </summary>
        public string? TypeName { get; set; }

        /// <summary>
        /// Име
        /// </summary>
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [Display(Name = "Име")]
        [MaxLength(100, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [RegularExpression(RegexPatterns.CyrillicPersonNamePattern, ErrorMessage = MessageConstant.NotCyrillic)]
        public string FirstName { get; set; } = null!;

        /// <summary>
        /// Презиме
        /// </summary>
        [MaxLength(100, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [Display(Name = "Презиме")]
        [RegularExpression(RegexPatterns.CyrillicPersonNamePattern, ErrorMessage = MessageConstant.NotCyrillic)]
        public string? MiddleName { get; set; }

        /// <summary>
        /// Фамилия
        /// </summary>
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [MaxLength(100, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [RegularExpression(RegexPatterns.CyrillicPersonNamePattern, ErrorMessage = MessageConstant.NotCyrillic)]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; } = null!;

        /// <summary>
        /// Длъжност
        /// </summary>
        [MaxLength(100, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [Display(Name = "Длъжност")]
        //[RegularExpression(RegexPatterns.CyrillicTextPattern, ErrorMessage = MessageConstant.NotCyrillic)]
        public string? Position { get; set; }

        /// <summary>
        /// Телефон
        /// Задължителен при тип лице "1" - контактно лице
        /// </summary>
        [MaxLength(20, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [RegularExpression(RegexPatterns.PhoneNumber, ErrorMessage = MessageConstant.RegexFail)]
        [Display(Name = MessageConstant.PhoneLabel, Prompt = MessageConstant.PhonePlaceholder)]
        public string? Phone { get; set; }

        /// <summary>
        /// Имейл
        /// Задължителен при тип лице "1" - контактно лице
        /// </summary>
        [MaxLength(100, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [RegularExpression(RegexPatterns.Email, ErrorMessage = MessageConstant.InvalidEmail)]
        [Display(Name = MessageConstant.EmailLabel)]
        public string? Email { get; set; }

        /// <summary>
        /// Индекс за темплейт
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Идентификатор на администрация
        /// </summary>
        public Guid AdministrationId { get; set; }
    }
}
