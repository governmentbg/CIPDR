using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Uregister.Users.Data.Identity
{
    public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
    {
        public void Configure(EntityTypeBuilder<ApplicationRole> builder)
        {
            builder.ToTable("identity_roles", t =>
            {
                t.HasComment("Роли");
            })
                .HasKey(role => role.Id);

            // Index for "normalized" role name to allow efficient lookups
            builder.HasIndex(r => r.NormalizedName).HasDatabaseName("role_name_index").IsUnique();

            // Each Role can have many entries in the UserRole join table
            builder.HasMany(e => e.UserRoles)
                .WithOne(e => e.Role)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();

            // Each Role can have many associated RoleClaims
            builder.HasMany(e => e.RoleClaims)
                .WithOne(e => e.Role)
                .HasForeignKey(rc => rc.RoleId)
                .IsRequired();

            builder.Property(p => p.Id)
                .HasColumnName("id")
                .HasComment("Идентификатор на запис");
            builder.Property(p => p.ConcurrencyStamp)
                .HasColumnName("concurrency_stamp")
                .IsConcurrencyToken()
                .HasComment("Защита от конкурентни заявки");
            builder.Property(p => p.Name)
                .HasColumnName("name")
                .HasMaxLength(256)
                .HasComment("Име на роля");
            builder.Property(p => p.NormalizedName)
                .HasColumnName("normalized_name")
                .HasMaxLength(256)
                .HasComment("Нормализирано име на роля");
        }
    }
}
