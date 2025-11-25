using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using URegister.Infrastructure.Constants;
using URegister.Core.Validation;

namespace URegister.Core.Models.User
{
    public class UserViewModel
    {
        private DateTime _createdAt;
        private long _createdTimestamp;

        public string? Id { get; set; }

        [DisplayName("Потребителско име")]
        [StringLength(50, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        //[Required(ErrorMessage = MessageConstant.FieldIsRequired)] TODO: празно е при нов
        public string? Username { get; set; }

        [DisplayName("Име")]
        [StringLength(50, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [RegularExpression(RegexPatterns.CyrillicPersonNamePattern, ErrorMessage = MessageConstant.NotCyrillic)]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        public string FirstName { get; set; }

        [DisplayName("Презиме")]
        [StringLength(50, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [RegularExpression(RegexPatterns.CyrillicPersonNamePattern, ErrorMessage = MessageConstant.NotCyrillic)]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        public string MiddleName { get; set; }

        [DisplayName("Фамилия")]
        [StringLength(50, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [RegularExpression(RegexPatterns.CyrillicPersonNamePattern, ErrorMessage = MessageConstant.NotCyrillic)]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        public string LastName { get; set; }

        [DisplayName(MessageConstant.EmailLabel)]
        [StringLength(100, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [RegularExpression(RegexPatterns.Email, ErrorMessage = MessageConstant.InvalidEmail)]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        public string Email { get; set; }

        [Display(Name = MessageConstant.PhoneLabel, Prompt = MessageConstant.PhonePlaceholder)]
        [StringLength(100, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [RegularExpression(RegexPatterns.PhoneNumber, ErrorMessage = MessageConstant.RegexFail)]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        public string PhoneNumber { get; set; }

        [DisplayName("Администрация")]
        [Required(ErrorMessage = MessageConstant.FieldIsRequiredNoParam)]
        public string? AdministrationId { get; set; }

        [DisplayName("Име на администрация")]
        public string? AdministrationName { get; set; }

        [DisplayName("Длъжност")]
        [StringLength(100, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        //[RegularExpression(RegexPatterns.CyrillicTextPattern, ErrorMessage = MessageConstant.NotCyrillic)]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        public string Position { get; set; }

        public long CreatedTimestamp
        {
            get => _createdTimestamp;
            set
            {
                _createdTimestamp = value;
                _createdAt = DateTimeOffset.FromUnixTimeMilliseconds(_createdTimestamp).DateTime;
                CreatedAtStr = _createdAt.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        public DateTime CreatedAt
        {
            get => _createdAt;
            set
            {
                _createdAt = value;
                CreatedAtStr = _createdAt.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        [DisplayName("Дата на създаване")]
        public string? CreatedAtStr { get; private set; }

        [DisplayName("Активиран")]
        public bool Enabled { get; set; }
        
        [DisplayName("ЕГН")]
        [StringLength(10, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [RegularExpression(RegexPatterns.Digits, ErrorMessage = MessageConstant.InvalidValue)]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [UrEgn(ErrorMessage = "Въведете валиден ЕГН")]
        public string Pid { get; set; }

        public bool IsGlobalAdmin { get; set; }
        [DisplayName("Получаване имейл при подадена е-форма")]
        public bool ReceiveEFormNotification { get; set; }

        [DisplayName("Получаване имейл при грешка подадена е-форма")]
        public bool ReceiveEFormOnErrorNotification { get; set; }

    }
}
