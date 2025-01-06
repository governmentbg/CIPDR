using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RulesEngine.Models;

namespace URegister.Infrastructure.Data.Configuration
{
    /// <summary>
    /// Конфигурира Workflow модела
    /// </summary>
    public class WorkflowConfiguration : IEntityTypeConfiguration<Workflow>
    {
        public void Configure(EntityTypeBuilder<Workflow> builder)
        {
            builder.HasKey(k => k.WorkflowName);
            builder.Ignore(b => b.WorkflowsToInject);
        }
    }
}
