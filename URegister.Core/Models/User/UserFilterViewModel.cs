using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using URegister.Infrastructure.Constants;

namespace URegister.Core.Models.User
{
    public class UserFilterViewModel
    {
        [DisplayName("Администрация")]
        public string? AdministrationId { get; set; }

        [DisplayName("Име")]
        [StringLength(50, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        //[RegularExpression(RegexPatterns.CyrillicPersonNamePattern, ErrorMessage = MessageConstant.NotCyrillic)]
        public string FirstName { get; set; }

        [DisplayName("Презиме")]
        [StringLength(50, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        //[RegularExpression(RegexPatterns.CyrillicPersonNamePattern, ErrorMessage = MessageConstant.NotCyrillic)]
        public string MiddleName { get; set; }

        [DisplayName("Фамилия")]
        [StringLength(50, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        //[RegularExpression(RegexPatterns.CyrillicPersonNamePattern, ErrorMessage = MessageConstant.NotCyrillic)]
        public string LastName { get; set; }

        [DisplayName("Роля")]
        public string RoleId { get; set; }

        [DisplayName(MessageConstant.EmailLabel)]
        [StringLength(100, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        public string Email { get; set; }

        [DisplayName("Само активни")]
        public bool ActiveOnly { get; set; }
    }
}
