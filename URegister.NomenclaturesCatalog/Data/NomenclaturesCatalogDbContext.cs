using Microsoft.EntityFrameworkCore;
using URegister.Infrastructure.Data;
using URegister.NomenclaturesCatalog.Data.Models;
using URegister.NomenclaturesCatalog.Infrastructure.Data.Models.Nomenclatures;

namespace URegister.RegistersCatalog.Data
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
            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Стойност на номенклатура
        /// </summary>
        public DbSet<CodeableConcept> CodeableConcepts { get; set; }

        /// <summary>
        /// Допълнителни данни
        /// </summary>
        public DbSet<AdditionalColumn> AdditionalColumns { get; set; }

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
