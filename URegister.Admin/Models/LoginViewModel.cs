using Microsoft.AspNetCore.Authentication;

namespace URegister.Admin.Models
{
    public class LoginViewModel
    {
        public string? ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();
    }
}
