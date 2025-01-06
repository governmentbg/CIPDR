using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using URegister.Core.Data.Models.Common;
using URegister.Core.Data.Models.Process;
using URegister.Core.Data.Models.Register;
using URegister.Infrastructure.Data;
using URegister.Infrastructure.Data.Common;

namespace URegister.Core.Data
{
    public class ApplicationDbContext : WorkflowDbContext, IDataProtectionKeyContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
       : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            //добавяне на филтър за soft deleted данни
            var entityTypesHasSoftDeletion = builder.Model.GetEntityTypes()
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

            base.OnModelCreating(builder);
        }

        public DbSet<Form> Forms { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceStep> ServiceSteps { get; set; }
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

        /// <summary>
        /// Регистър
        /// </summary>
        public DbSet<Register> Registers { get; set; }

        /// <summary>
        /// Администрации
        /// </summary>
        public DbSet<Administration> Administrations { get; set; }

        /// <summary>
        /// Записи на лица в регистър
        /// </summary>
        public DbSet<AdministrationPerson> AdministrationPersons { get; set; }

        /// <summary>
        /// Процеси
        /// </summary>
        public DbSet<Process> Processes { get; set; }

        /// <summary>
        /// Стъпки към процес
        /// </summary>
        public DbSet<ProcessStep> ProcessSteps { get; set; }

        /// <summary>
        /// Вписвания
        /// </summary>
        public DbSet<RegisterItem> RegisterItems { get; set; }
    }
}
