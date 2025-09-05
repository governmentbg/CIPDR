using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace URegister.Core.Models.Nomenclature
{
    /// <summary>
    /// Модел на филтър за номенклатури
    /// </summary>
    public class NomenclatureTypeFilterVM
    {
        /// <summary>
        /// Тип
        /// </summary>
        [Display(Name = "Тип")]
        public string? Type
        {
            get => _type;
            set => _type = value?.Trim();
        }
        private string? _type;

        /// <summary>
        /// Име
        /// </summary>
        [Display(Name = "Име")]
        public string? Name
        {
            get => _name;
            set => _name = value?.Trim();
        }
        private string? _name;
    }

}
