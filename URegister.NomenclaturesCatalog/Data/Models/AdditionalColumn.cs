using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace URegister.NomenclaturesCatalog.Data.Models
{
    /// <summary>
    /// Допълнителни данни за номенклатура
    /// </summary>
    [Table("additional_columns")]
    [Comment("Допълнителни данни за номенклатура")]
    [Index(nameof(CodeableConceptId), nameof(Name), IsUnique = true)]
    public class AdditionalColumn
    {

        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        [Comment("Идентификатор")]
        public long Id { get; set; }


        /// <summary>
        /// Идентификатор на номенклатура
        /// </summary>
        [Comment("Идентификатор на номенклатура")]
        public long CodeableConceptId { get; set; }

        /// <summary>
        /// Име на колона
        /// </summary>
        [StringLength(50)]
        [Comment("Име на колона")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Стойност 
        /// </summary>
        [StringLength(1024)]
        [Comment("Стойност")]
        public string Value { get; set; } = null!;

        /// <summary>
        /// Стойност ЕН
        /// </summary>
        [StringLength(1024)]
        [Comment("Стойност ЕН")]
        public string? ValueEn { get; set; }

        /// <summary>
        /// Номенклатура
        /// </summary>
        [ForeignKey(nameof(CodeableConceptId))]
        public CodeableConcept CodeableConcept { get; set; } = null!;
    }
}
