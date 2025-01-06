using Microsoft.AspNetCore.Identity;

namespace Uregister.Users.Data.Identity
{
    public class ApplicationUserRole : IdentityUserRole<Guid>
    {
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ApplicationRole Role { get; set; } = null!;

        public string RegisterCode { get; set; } = null!;
    }
}
