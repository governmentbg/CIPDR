using Microsoft.AspNetCore.Identity;

namespace Uregister.Users.Data.Identity
{
    public class ApplicationUserToken : IdentityUserToken<Guid>
    {
        public virtual ApplicationUser User { get; set; } = null!;
    }
}