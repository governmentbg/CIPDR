using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Uregister.Users.Data.Identity
{
    public class ApplicationUserClaimConfiguration : IEntityTypeConfiguration<ApplicationUserClaim>
    {
        public void Configure(EntityTypeBuilder<ApplicationUserClaim> builder)
        {
            builder.ToTable("identity_user_claims", t =>
            {
                t.HasComment("Клейма на потребител");
            })
                .HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .HasColumnName("id")
                .HasComment("Идентификатор на запис");
            builder.Property(p => p.UserId)
                .HasColumnName("user_id")
                .HasComment("Идентификатор на потребител");
            builder.Property(p => p.ClaimType)
                .HasColumnName("claim_type")
                .HasComment("Тип на клейм");
            builder.Property(p => p.ClaimValue)
                .HasColumnName("claim_value")
                .HasComment("Стойност на клейм");
        }
    }
}
