using Microsoft.EntityFrameworkCore;
using RulesEngine.Models;
using URegister.Infrastructure.Data.Configuration;

namespace URegister.Infrastructure.Data
{
    /// <summary>
    /// Абстрактен клас за контекст на базата данни за изпълнение на работни потоци
    /// </summary>
    public abstract class WorkflowDbContext : DbContext
    {
        protected WorkflowDbContext(DbContextOptions options)
            : base(options)
        {
        }

        /// <summary>
        /// Работни потоци
        /// </summary>
        public DbSet<Workflow> Workflows { get; set; }

        /// <summary>
        /// Правила
        /// </summary>
        public DbSet<Rule> Rules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        { 
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new ScopedParamConfiguration());
            modelBuilder.ApplyConfiguration(new WorkflowConfiguration());
            modelBuilder.ApplyConfiguration(new RuleConfiguration());
        }
    }
}
