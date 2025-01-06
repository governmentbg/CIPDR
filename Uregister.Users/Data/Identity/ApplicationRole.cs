using Microsoft.AspNetCore.Identity;

namespace Uregister.Users.Data.Identity
{
    public class ApplicationRole : IdentityRole<Guid>
    {
        public string Label { get; set; } = null!;
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();
        public virtual ICollection<ApplicationRoleClaim> RoleClaims { get; set; } = new List<ApplicationRoleClaim>();
    }
}
