using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace URegister.Core.Models.Nomenclature
{
    public class NomenclatureTypeRegisterFilterVM
    {
        /// <summary>
        /// Регистър
        /// </summary>
        [Display(Name = "Регистър")]
        public int RegisterId { get; set; }


        /// <summary>
        /// Тип
        /// </summary>
        [Display(Name = "Тип")]
        public string? Type { get; set; }

        /// <summary>
        /// Име
        /// </summary>
        [Display(Name = "Име")]
        public string? Name { get; set; }


        public bool IsValidAllType { get; set; }
    }
}
