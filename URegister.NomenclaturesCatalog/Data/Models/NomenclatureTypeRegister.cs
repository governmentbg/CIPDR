using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace URegister.NomenclaturesCatalog.Data.Models
{
    /// <summary>
    /// Допустим тип номенклатура за администрация
    /// </summary>
    [Table("nomenclature_type_registers")]
    [Comment("Допустим тип номенклатура")]
    [Index(nameof(NomenclatureTypeId), nameof(RegisterId), IsUnique = true)]
    public class NomenclatureTypeRegister
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        [Comment("Идентификатор")]
        public long Id { get; set; }

        /// <summary>
        /// Идентификатор
        /// </summary>
        [Comment("Идентификатор на тип")]
        public int NomenclatureTypeId { get; set; }

        /// <summary>
        /// Идентификатор на регистър
        /// </summary>
        [Comment("Идентификатор на регистър")]
        public int RegisterId { get; set; }

        /// <summary>
        /// Допустима ли е за регистъра
        /// </summary>
        [Comment("Допустима ли е за регистъра")]
        public bool IsValid { get; set; }

        /// <summary>
        /// Допустими ли са всички стойности от CodeableConcept
        /// </summary>
        [Comment("Допустими ли са всички стойности от CodeableConcept")]
        public bool IsValidAllItems { get; set; }

        /// <summary>
        /// Създаден от
        /// </summary>
        [StringLength(255)]
        [Comment("Създаден от")]
        public string? CreatedBy { get; set; } = null!;

        /// <summary>
        /// Дата и час на записа
        /// </summary>
        [Column(TypeName = "timestamptz")]
        [Comment("Дата и час на записа")]
        public DateTime? CreatedOn { get; set; } = null!;

        /// <summary>
        /// Номенклатура
        /// </summary>
        [ForeignKey(nameof(NomenclatureTypeId))]
        public NomenclatureType NomenclatureType { get; set; } = null!;
    }
}
