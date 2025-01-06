using Microsoft.EntityFrameworkCore;
using URegister.Infrastructure.Data;
using URegister.RegistersCatalog.Data.Models;

namespace URegister.RegistersCatalog.Data
{
    /// <summary>
    /// Контекст на базата данни за съхранение на каталога с регистри
    /// </summary>
    public class RegistersCatalogDbContext : WorkflowDbContext
    {
        /// <summary>
        /// Създава нов контекст на базата данни за съхранение на каталога с регистри
        /// </summary>
        /// <param name="options"></param>
        public RegistersCatalogDbContext(DbContextOptions<RegistersCatalogDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Настройва модела на базата данни
        /// Задължително е да се извика базовия метод
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Глобални партиди на лица
        /// </summary>
        public DbSet<MasterPersonRecordsIndex> MasterPersonRecords { get; set; }

        /// <summary>
        /// Регистри
        /// </summary>
        public DbSet<Register> Registers { get; set; }

        /// <summary>
        /// Администрации
        /// </summary>
        public DbSet<RegisterAdministration> RegisterAdministrations { get; set; }

        /// <summary>
        /// Записи на лица в регистър
        /// </summary>
        public DbSet<RegisterPersonRecord> RegisterPersonRecords { get; set; }
    }
}
