using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using URegister.Infrastructure.Constants;
using URegister.Core.Validation;

namespace URegister.Core.Models.Nomenclature
{
    /// <summary>
    /// Номенклатурни стойности
    /// </summary>
    public class CodeableConceptVM
    {
        /// <summary>
        /// Редакция/Добавяне 
        /// </summary>
        public bool IsInsert { get; set; }

        [StringLength(20, MinimumLength = 1, ErrorMessage = "Максималната дължина на полето {0} е {1}")]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [Display(Name = "Код")]
        public string Code { get; set; } = null!;

        /// <summary>
        /// Горно ниво в друг номенклатурна стойност 
        /// </summary>
        [Display(Name = "Горно ниво")]
        public string? HolderCode { get; set; }

        /// <summary>
        /// Стойност
        /// </summary>
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Максималната дължина на полето {0} е {1}")]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [Display(Name = "Стойност")]
        public string Value { get; set; } = null!;

        [StringLength(255, ErrorMessage = "Максималната дължина на полето {0} е {1}")]
        [Display(Name = "Стойност ЕН")]
        public string? ValueEn { get; set; }

        /// <summary>
        /// Валидна от дата
        /// </summary>
        [Column(TypeName = "date")]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [URStateDate(ErrorMessage = "Въведете валидна дата")]
        [Display(Name = "Валидна от дата")]
        public DateTime DateFrom { get; set; }

        public DateTime DateFromInit { get; set; }

        /// <summary>
        /// Валидна до дата
        /// </summary>
        [Column(TypeName = "date")]
        [Display(Name = "Валидна до дата")]
        [URStateDate(ErrorMessage = "Въведете валидна дата")]
        public DateTime? DateTo { get; set; }


        /// <summary>
        /// Тип на номенклатура
        /// </summary>
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [StringLength(6, MinimumLength = 5, ErrorMessage = "Дължината на полето {0} трябва да е {1}")]
        [Display(Name = "Тип на номенклатура")]
        public string Type { get; set; } = null!;

        /// <summary>
        /// Код не горно ниво при дървовидна номенклатура
        /// </summary>
        [Display(Name = "Код не горно ниво при дървовидна номенклатура")]
        public string? ParentCode { get; set; }

        [Display(Name = "Допълнителни колони")]
        public List<AdditionalColumnVM> AdditionalColumns { get; set; } = new();
    }
}
