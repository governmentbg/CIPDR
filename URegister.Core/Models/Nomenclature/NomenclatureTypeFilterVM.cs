using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace URegister.Core.Models.Nomenclature
{
    public class NomenclatureTypeFilterVM
    {
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
    }
}
