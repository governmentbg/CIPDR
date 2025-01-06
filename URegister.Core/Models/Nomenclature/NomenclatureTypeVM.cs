using System.ComponentModel.DataAnnotations;
using URegister.Infrastructure.Constants;

namespace URegister.Core.Models.Nomenclature
{
    public class NomenclatureTypeVM
    {
        /// <summary>
        /// Редакция/Добавяне 
        /// </summary>
        public bool IsInsert { get; set; }

        /// <summary>
        /// Тип
        /// </summary>
        [Display(Name = "Тип")]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [StringLength(6, MinimumLength = 2, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        public string Type { get; set; } = null!;

        /// <summary>
        /// Горно ниво в друг номенклатурен тип
        /// </summary>
        [Display(Name = "Горно ниво")]
        public string? HolderType { get; set; }

        /// <summary>
        /// Име
        /// </summary>
        [Display(Name = "Име")]
        [StringLength(255)]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Публичен номенклатурен тип
        /// </summary>
        [Display(Name = "Публичен номенклатурен тип")]
        public bool IsPublic { get; set; }
    }
}
