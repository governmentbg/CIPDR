using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using URegister.Infrastructure.Constants;

namespace URegister.Core.Models.Nomenclature
{
    public class AdditionalColumnVM
    {
        /// <summary>
        /// Име на колона
        /// </summary>
        [StringLength(50, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [Display(Name = "Име на колона")]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Горно ниво в друг номенклатурен тип
        /// </summary>
        [Display(Name = "Горно ниво номенклатура")]
        public string? HolderType { get; set; }


        /// <summary>
        /// Стойност 
        /// </summary>
        [StringLength(1024, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [Display(Name = "Стойност")]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        public string Value { get; set; } = null!;

        /// <summary>
        /// Стойност EN
        /// </summary>
        [StringLength(1024, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [Display(Name = "Стойност EN")]
        public string? ValueEn { get; set; }

        /// <summary>
        /// Индекс за тамплейт
        /// </summary>
        public int Index { get; set; }
    }
}
