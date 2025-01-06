using Microsoft.EntityFrameworkCore;
using URegister.NumberGenerator.Data.Models;

namespace URegister.NumberGenerator.Data
{
    /// <summary>
    /// Контекст на базата данни за генератора на номера
    /// </summary>
    public class NumberGeneratorDbContext : DbContext
    {
        /// <summary>
        /// Създава нов контекст на базата данни за генератора на номера
        /// </summary>
        /// <param name="options"></param>
        public NumberGeneratorDbContext(DbContextOptions<NumberGeneratorDbContext> options) 
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        }

        /// <summary>
        /// Номератори на институциите използващи универсалния регистър
        /// </summary>
        public DbSet<Numerator> Numerators { get; set; }

        /// <summary>
        /// Архив на номератора
        /// </summary>
        public DbSet<NumberArchive> NumberArchives { get; set; }

        /// <summary>
        /// Архив на номератора за външни системи
        /// </summary>
        public DbSet<ExternalNumberArchive> ExternalNumberArchives { get; set; }
    }
}
