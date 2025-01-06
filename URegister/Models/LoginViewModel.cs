using Microsoft.AspNetCore.Authentication;
using System.ComponentModel.DataAnnotations;

namespace URegister.Models
{
    public class LoginViewModel
    {
        public string? ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();
    }
}
