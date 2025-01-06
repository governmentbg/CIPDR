using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Uregister.Users.Data.Identity
{
    public class ApplicationUserLoginConfiguration : IEntityTypeConfiguration<ApplicationUserLogin>
    {
        public void Configure(EntityTypeBuilder<ApplicationUserLogin> builder)
        {
            builder.ToTable("identity_user_logins", t =>
            {
                t.HasComment("Външни доставчици на удостоверителни услуги");
            })
                .HasKey(p => new { p.ProviderKey, p.LoginProvider });

            builder.Property(p => p.ProviderKey)
                .HasColumnName("provider_key")
                .HasMaxLength(128)
                .HasComment("Ключ на доставчик");
            builder.Property(p => p.UserId)
                .HasColumnName("user_id")
                .HasComment("Идентификатор на потребител");
            builder.Property(p => p.LoginProvider)
                .HasColumnName("login_provider")
                .HasMaxLength(128)
                .HasComment("Доставчик на удостоверителни услуги");
            builder.Property(p => p.ProviderDisplayName)
                .HasColumnName("provider_display_name")
                .HasComment("Етикет на доставчик на удостоверителни услуги");
        }
    }
}
