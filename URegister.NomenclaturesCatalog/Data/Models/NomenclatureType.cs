using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace URegister.NomenclaturesCatalog.Data.Models
{
    /// <summary>
    /// Тип номенклатура
    /// </summary>
    [Table("nomenclature_types")]
    [Comment("Тип номенклатура")]
    [Index(nameof(Type), IsUnique = true)]
    public class NomenclatureType
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        [Comment("Идентификатор")]
        public int Id { get; set; }

        /// <summary>
        /// Тип
        /// </summary>
        [Comment("Тип")]
        [Required(ErrorMessage = "Полето {0} е задължително")]
        [StringLength(6, MinimumLength = 5, ErrorMessage = "Дължината на полето {0} трябва да е {1}")]
        public string Type { get; set; } = null!;

        /// <summary>
        /// Име
        /// </summary>
        [Comment("Име")]
        [StringLength(255)]
        [Required]
        public string Name { get; set; } = null!;


        /// <summary>
        /// Горно ниво в друг номенклатурен тип
        /// </summary>
        [Comment("Горно ниво в друг номенклатурен тип")]
        public string? HolderType { get; set; }

        /// <summary>
        /// Публичен номенклатурен тип
        /// </summary>
        [Comment("Публичен номенклатурен тип")]
        public bool IsPublic { get; set; }

        /// <summary>
        /// Администрации ползващи тази номенклатура
        /// </summary>
        public virtual ICollection<NomenclatureTypeRegister> Registers { get; set; } = null!;

    }
}
