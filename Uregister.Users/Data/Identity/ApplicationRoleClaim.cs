using Microsoft.AspNetCore.Identity;

namespace Uregister.Users.Data.Identity
{
    public class ApplicationRoleClaim : IdentityRoleClaim<Guid>
    {
        public virtual ApplicationRole Role { get; set; } = null!;
    }
}
