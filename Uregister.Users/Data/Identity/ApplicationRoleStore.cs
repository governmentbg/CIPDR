using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Uregister.Users.Data.Identity
{
    [Table("roles")]
    public class ApplicationRoleStore : RoleStore<ApplicationRole, UserDbContext, Guid, ApplicationUserRole, ApplicationRoleClaim>
    {
        public ApplicationRoleStore(UserDbContext context) : base(context)
        {
        }
    }
}
