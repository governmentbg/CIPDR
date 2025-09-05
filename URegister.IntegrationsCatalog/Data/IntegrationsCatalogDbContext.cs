using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using URegister.Infrastructure.Data;
using URegister.Infrastructure.Data.Common;
using URegister.IntegrationsCatalog.Data.Models;

namespace URegister.IntegrationsCatalog.Data
{
    /// <summary>
    /// Контекст на базата данни за съхранение на каталога с регистри
    /// </summary>
    public class IntegrationsCatalogDbContext : WorkflowDbContext
    {
        /// <summary>
        /// Създава нов контекст на базата данни за съхранение на каталога с регистри
        /// </summary>
        /// <param name="options"></param>
        public IntegrationsCatalogDbContext(DbContextOptions<IntegrationsCatalogDbContext> options)
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
        public DbSet<EDeliveryMessage> EDeliveryMessages { get; set; }
        public DbSet<EDeliveryFileMetadata> EDeliveryFileMetadata { get; set; }
        public DbSet<EMailMessage> EMailMessages { get; set; }
    }
}
