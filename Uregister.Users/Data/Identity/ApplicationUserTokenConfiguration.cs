using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Uregister.Users.Data.Identity
{
    public class ApplicationUserTokenConfiguration : IEntityTypeConfiguration<ApplicationUserToken>
    {
        public void Configure(EntityTypeBuilder<ApplicationUserToken> builder)
        {
            builder.ToTable("identity_user_tokens", t =>
            {
                t.HasComment("Потребителски токъни");
            })
                .HasKey(p => new { p.UserId, p.LoginProvider, p.Name });

            builder.Property(p => p.UserId)
                .HasColumnName("user_id")
                .HasComment("Идентификатор на потребител");
            builder.Property(p => p.LoginProvider)
                .HasColumnName("login_provider")
                .HasMaxLength(128)
                .HasComment("Доставчик на удостоверителни услуги");
            builder.Property(p => p.Name)
                .HasColumnName("name")
                .HasMaxLength(128)
                .HasComment("Име на токън");
            builder.Property(p => p.Value)
                .HasColumnName("value")
                .HasComment("Стойност на токън");
        }
    }
}
