using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RulesEngine.Models;
using System.Text.Json;

namespace URegister.Infrastructure.Data.Configuration
{
    /// <summary>
    /// Конфигурира Rule модела
    /// </summary>
    public class RuleConfiguration : IEntityTypeConfiguration<Rule>
    {
        public void Configure(EntityTypeBuilder<Rule> builder)
        {
            builder.HasOne<Rule>().WithMany(r => r.Rules).HasForeignKey("RuleNameFK");

            var serializationOptions = new JsonSerializerOptions(JsonSerializerDefaults.General);

            builder.HasKey(k => k.RuleName);

            var valueComparer = new ValueComparer<Dictionary<string, object>>(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c);

            builder.Property(b => b.Properties)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, serializationOptions),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, serializationOptions))
                    .Metadata
                    .SetValueComparer(valueComparer);

            builder.Property(p => p.Actions)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, serializationOptions),
                    v => JsonSerializer.Deserialize<RuleActions>(v, serializationOptions));

            builder.Ignore(b => b.WorkflowsToInject);
        }
    }
}
