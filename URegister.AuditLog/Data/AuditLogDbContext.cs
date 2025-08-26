using Microsoft.EntityFrameworkCore;


namespace URegister.AuditLog.Data
{
    /// <summary>
    /// Контекст на базата данни за генератора на номера
    /// </summary>
    public class AuditLogDbContext : DbContext
    {
        /// <summary>
        /// Създава нов контекст на базата данни за генератора на номера
        /// </summary>
        /// <param name="options"></param>
        public AuditLogDbContext(DbContextOptions<AuditLogDbContext> options) 
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        }

        /// <summary>
        /// Потребителски изгледи на справки
        /// </summary>
        public DbSet<Audit> Audits { get; set; }

        public List<AuditEntity> AuditEntities { get; set; }

    }
}
