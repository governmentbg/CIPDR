using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace URegister.Core.Models.Nomenclature
{
    public class NomenclatureTypeRegisterUpdateVM
    {
        /// <summary>
        /// Регистър
        /// </summary>
        public int RegisterId { get; set; }

        /// <summary>
        /// Тип
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Валиден
        /// </summary>
        public bool? IsValid { get; set; }

        /// <summary>
        /// всички стойности
        /// </summary>
        public bool? IsValidAll { get; set; }

        /// <summary>
        /// Тип от филтъра
        /// </summary>
        public string? FilterType { get; set; }

        /// <summary>
        /// Име от филтъра
        /// </summary>
        public string? FilterName { get; set; }

    }
}
