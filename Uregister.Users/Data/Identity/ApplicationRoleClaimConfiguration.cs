using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

    namespace Uregister.Users.Data.Identity
    {
    public class ApplicationRoleClaimConfiguration : IEntityTypeConfiguration<ApplicationRoleClaim>
    {
        public void Configure(EntityTypeBuilder<ApplicationRoleClaim> builder)
        {
            builder.ToTable("identity_role_claims", t => 
            {
                t.HasComment("Клейма към роля");
            })
              .HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .HasColumnName("id")
                .HasComment("Идентификатор на запис");
            builder.Property(p => p.RoleId)
                .HasColumnName("role_id")
                .HasComment("Идентификатор на роля");
            builder.Property(p => p.ClaimType)
                .HasColumnName("claim_type")
                .HasComment("Тип на клейм");
            builder.Property(p => p.ClaimValue)
                .HasColumnName("claim_value")
                .HasComment("Стойност на клейм");
        }
    }
}
