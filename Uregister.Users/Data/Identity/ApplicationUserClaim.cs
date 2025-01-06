using Microsoft.AspNetCore.Identity;

namespace Uregister.Users.Data.Identity
{
    public class ApplicationUserClaim : IdentityUserClaim<Guid>
    {
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
