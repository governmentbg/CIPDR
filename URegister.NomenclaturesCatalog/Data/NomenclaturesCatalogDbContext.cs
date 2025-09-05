using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using URegister.Infrastructure.Data;
using URegister.Infrastructure.Data.Common;
using URegister.NomenclaturesCatalog.Data.Models;
using URegister.NomenclaturesCatalog.Infrastructure.Data.Models.Nomenclatures;

namespace URegister.NomenclaturesCatalog.Data
{
    /// <summary>
    /// Контекст на базата данни за съхранение на каталога с регистри
    /// </summary>
    public class NomenclaturesCatalogDbContext : WorkflowDbContext
    {
        /// <summary>
        /// Създава нов контекст на базата данни за съхранение на каталога с регистри
        /// </summary>
        /// <param name="options"></param>
        public NomenclaturesCatalogDbContext(DbContextOptions<NomenclaturesCatalogDbContext> options)
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
        /// Стойност на номенклатура
        /// </summary>
        public DbSet<CodeableConcept> CodeableConcepts { get; set; }

        /// <summary>
        /// Допълнителни данни
        /// </summary>
        public DbSet<Models.AdditionalColumn> AdditionalColumns { get; set; }

        /// <summary>
        /// Типове номенклатури
        /// </summary>
        public DbSet<NomenclatureType> NomenclatureTypes { get; set; }

        /// <summary>
        /// Допустимост на номенклатура за регистъра
        /// </summary>
        public DbSet<CodeableConceptRegister> CodeableConceptAdministrations { get; set; }


        /// <summary>
        /// Допустим тип номенклатура за администрация
        /// </summary>
        public DbSet<NomenclatureTypeRegister> NomenclatureTypeAdministrations { get; set; }

        public DbSet<EkDoc> ekDocs { get; set; }
    }
}
