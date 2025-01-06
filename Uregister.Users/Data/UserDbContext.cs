using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Uregister.Users.Data.Identity;
using URegister.Infrastructure.Data.Configuration;

namespace Uregister.Users.Data
{
    public class UserDbContext : DbContext, IDataProtectionKeyContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            #region Identity configuration

            builder.ApplyConfiguration(new ApplicationUserConfiguration());
            builder.ApplyConfiguration(new ApplicationRoleConfiguration());
            builder.ApplyConfiguration(new ApplicationUserRoleConfiguration());
            builder.ApplyConfiguration(new ApplicationUserClaimConfiguration());
            builder.ApplyConfiguration(new ApplicationUserLoginConfiguration());
            builder.ApplyConfiguration(new ApplicationRoleClaimConfiguration());
            builder.ApplyConfiguration(new ApplicationUserTokenConfiguration());

            #endregion

            builder.ApplyConfiguration(new DataProtectionKeysConfiguration());
        }

        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
    }
}
