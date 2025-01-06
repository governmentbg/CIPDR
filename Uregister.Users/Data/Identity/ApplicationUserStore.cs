using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Uregister.Users.Data.Identity
{
    [Table("users")]
    public class ApplicationUserStore : UserStore<ApplicationUser, ApplicationRole, UserDbContext, Guid, ApplicationUserClaim, ApplicationUserRole, ApplicationUserLogin, ApplicationUserToken, ApplicationRoleClaim>
    {
        public ApplicationUserStore(UserDbContext context) : base(context)
        {

        }
    }
}
