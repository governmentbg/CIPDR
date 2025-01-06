using Microsoft.EntityFrameworkCore;
using URegister.Infrastructure.Data;
using URegister.ObjectsCatalog.Data.Models;

namespace URegister.ObjectsCatalog.Data
{
    /// <summary>
    /// Контекст на базата данни за каталог на обекти
    /// </summary>
    public class ObjectCatalogDbContext : WorkflowDbContext
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="options"></param>
        public ObjectCatalogDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ObjectCatalogDbContext).Assembly);
        }

        /// <summary>
        /// Полета
        /// </summary>
        public DbSet<Field> Fields { get; set; }

        /// <summary>
        /// Типове полета
        /// </summary>
        public DbSet<FieldType> FieldTypes { get; set; }

        /// <summary>
        /// Стъпки
        /// </summary>
        public DbSet<Step> Steps { get; set; }

        /// <summary>
        /// Типове на услуги
        /// </summary>
        public DbSet<ServiceType> ServiceTypes { get; set; }

        /// <summary>
        /// Стъпки към видове услуги
        /// </summary>
        public DbSet<ServiceTypeStep> ServiceTypeSteps { get; set; }
    }
}
