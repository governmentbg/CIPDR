using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using URegister.Infrastructure.Data;
using URegister.Infrastructure.Data.Common;
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
            //добавяне на филтър за soft deleted данни
            var entityTypesHasSoftDeletion = modelBuilder.Model.GetEntityTypes()
                .Where(e => e.ClrType.IsAssignableTo(typeof(ISoftDeletable)));

            foreach (var entityType in entityTypesHasSoftDeletion)
            {
                var isDeletedProperty = entityType.FindProperty(nameof(ISoftDeletable.IsActive));
                var parameter = Expression.Parameter(entityType.ClrType, "p");

                if (isDeletedProperty?.PropertyInfo != null && parameter != null)
                {
                    var filter = Expression.Lambda(Expression.Property(parameter, isDeletedProperty.PropertyInfo), parameter);
                    entityType.SetQueryFilter(filter);
                }
            }

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

        /// <summary>
        /// Статуси
        /// </summary>
        public DbSet<RegisterStatus> RegisterStatus { get; set; }

        /// <summary>
        /// Файлове
        /// </summary>
        public DbSet<RegisterFileMetadata> RegisterFileMetadata { get; set; }

        /// <summary>
        /// Услуги заради РНУ
        /// </summary>
        public DbSet<Data.Models.RegisterService> RegisterServices { get; set; }

        /// <summary>
        /// Календар работни дни
        /// </summary>
        public DbSet<CalendarDay> CalendarDays { get; set; }
    }
}
