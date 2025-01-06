using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RulesEngine.Models;

namespace URegister.Infrastructure.Data.Configuration
{
    /// <summary>
    /// Конфигурира ScopedParam модела
    /// </summary>
    public class ScopedParamConfiguration : IEntityTypeConfiguration<ScopedParam>
    {
        public void Configure(EntityTypeBuilder<ScopedParam> builder)
        {
            builder.HasKey(k => k.Name);
        }
    }
}
