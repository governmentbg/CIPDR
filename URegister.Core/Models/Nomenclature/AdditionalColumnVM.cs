using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace URegister.Core.Models.Nomenclature
{
    public class AdditionalColumnVM
    {
        /// <summary>
        /// Име на колона
        /// </summary>
        [StringLength(50)]
        [Display(Name = "Име на колона")]
        [Required]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Горно ниво в друг номенклатурен тип
        /// </summary>
        [Display(Name = "Горно ниво номенклатура")]
        public string? HolderType { get; set; }


        /// <summary>
        /// Стойност 
        /// </summary>
        [StringLength(1024)]
        [Display(Name = "Стойност")]
        [Required]
        public string Value { get; set; } = null!;

        /// <summary>
        /// Стойност ЕН
        /// </summary>
        [StringLength(1024)]
        [Display(Name = "Стойност ЕН")]
        public string? ValueEn { get; set; }

        /// <summary>
        /// Индекс за тамплейт
        /// </summary>
        public int Index { get; set; }
    }
}
