using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Uregister.Users.Data.Identity
{
    public class ApplicationUserRoleConfiguration : IEntityTypeConfiguration<ApplicationUserRole>
    {
        public void Configure(EntityTypeBuilder<ApplicationUserRole> builder)
        {
            builder.ToTable("identity_user_roles", t =>
            {
                t.HasComment("Роли на потребител");
            })
                .HasKey(p => new { p.UserId, p.RoleId, p.RegisterCode });

            builder.Property(p => p.RoleId)
                .HasColumnName("role_id")
                .HasComment("Идентификатор на роля");
            builder.Property(p => p.UserId)
                .HasColumnName("user_id")
                .HasComment("Идентификатор на потребител");
            builder.Property(p => p.RegisterCode)
                .HasColumnName("register_code")
                .HasComment("Код на регистър");
        }
    }
}
