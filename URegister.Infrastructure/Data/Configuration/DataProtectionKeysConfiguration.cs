using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace URegister.Infrastructure.Data.Configuration
{
    public class DataProtectionKeysConfiguration : IEntityTypeConfiguration<DataProtectionKey>
    {
        public void Configure(EntityTypeBuilder<DataProtectionKey> builder)
        {
            builder.ToTable("data_protection_keys", t =>
            {
                t.HasComment("Ключове за защита на данни");
            });

            builder.Property(p => p.Id)
                .HasColumnName("id")
                .HasComment("Идентификатор на запис");
            builder.Property(p => p.FriendlyName)
                .HasColumnName("friendly_name")
                .HasComment("Име на ключа");
            builder.Property(p => p.Xml)
                .HasColumnName("xml")
                .HasComment("Ключ в XML формат");

        }
    }
}
