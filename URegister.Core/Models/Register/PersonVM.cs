using System.ComponentModel.DataAnnotations;
using URegister.Infrastructure.Constants;

namespace URegister.Core.Models.Register
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
        public string Type
        {
            get => _type;
            set => _type = value?.Trim();
        }
        private string _type = null!;

        /// <summary>
        /// Име
        /// </summary>
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [Display(Name = "Име")]
        [MaxLength(100, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [RegularExpression(RegexPatterns.CyrillicPersonNamePattern, ErrorMessage = MessageConstant.NotCyrillic)]
        public string FirstName
        {
            get => _firstName;
            set => _firstName = value?.Trim();
        }
        private string _firstName = null!;

        /// <summary>
        /// Презиме
        /// </summary>
        [MaxLength(100, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [Display(Name = "Презиме")]
        [RegularExpression(RegexPatterns.CyrillicPersonNamePattern, ErrorMessage = MessageConstant.NotCyrillic)]
        public string? MiddleName
        {
            get => _middleName;
            set => _middleName = value?.Trim();
        }
        private string? _middleName;

        /// <summary>
        /// Фамилия
        /// </summary>
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [MaxLength(100, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [Display(Name = "Фамилия")]
        [RegularExpression(RegexPatterns.CyrillicPersonNamePattern, ErrorMessage = MessageConstant.NotCyrillic)]
        public string LastName
        {
            get => _lastName;
            set => _lastName = value?.Trim();
        }
        private string _lastName = null!;

        /// <summary>
        /// Длъжност
        /// </summary>
        [MaxLength(100, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [Display(Name = "Длъжност")]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        //[RegularExpression(RegexPatterns.CyrillicTextPattern, ErrorMessage = MessageConstant.NotCyrillic)]
        public string? Position
        {
            get => _position;
            set => _position = value?.Trim();
        }
        private string? _position;

        /// <summary>
        /// Телефон
        /// Задължителен при тип лице "1" - контактно лице
        /// </summary>
        [MaxLength(20, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [Display(Name = MessageConstant.PhoneLabel, Prompt = MessageConstant.PhonePlaceholder)]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [RegularExpression( RegexPatterns.PhoneNumber, ErrorMessage = MessageConstant.RegexFail)]
        public string? Phone
        {
            get => _phone;
            set => _phone = value?.Trim();
        }
        private string? _phone;

        /// <summary>
        /// Имейл
        /// Задължителен при тип лице "1" - контактно лице
        /// </summary>
        [MaxLength(100, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [Display(Name = "Имейл")]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [RegularExpression( RegexPatterns.Email, ErrorMessage = MessageConstant.InvalidEmail)]
        public string? Email
        {
            get => _email;
            set => _email = value?.Trim();
        }
        private string? _email;

        /// <summary>
        /// Индекс за тамплейт
        /// </summary>
        public int Index { get; set; }
    }
}
