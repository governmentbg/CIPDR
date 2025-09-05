using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using URegister.Infrastructure.Constants;

namespace URegister.Models
{

    public class LoginVM
    {
        [Display(Name = "Потребителско име")]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        public string Username { get; set; }

        [Display(Name = "Парола")]
        public string Password { get; set; }

        [Display(Name = "Запомни ме")]
        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }

        public bool LoginWithPassword { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }
    }
}
