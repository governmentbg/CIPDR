using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace URegister.NomenclaturesCatalog.Data.Models
{
    /// <summary>
    /// Допустимост на номенклатура за регистъра
    /// </summary>
    [Table("codeable_concept_registers")]
    [Comment("Допустимост на номенклатура за регистъра")]
    [Index(nameof(Type), nameof(Code), nameof(RegisterId), IsUnique = true)]
    public class CodeableConceptRegister
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        [Comment("Идентификатор")]
        public long Id { get; set; }

        /// <summary>
        /// Тип на номенклатур
        /// </summary>
        [Comment("Тип на номенклатура")]
        public string Type { get; set; } = null!;

        /// <summary>
        /// Код на номенклатура
        /// </summary>
        [Comment("Код на номенклатура")]
        public string Code { get; set; } = null!;

        /// <summary>
        /// Идентификатор на администрация
        /// </summary>
        [Comment("Идентификатор на регистър")]
        public int RegisterId { get; set; }

        /// <summary>
        /// Допустима ли е за регистъра
        /// </summary>
        [Comment("Допустима ли е за регистъра")]
        public bool IsValid { get; set; }

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
    }
}
