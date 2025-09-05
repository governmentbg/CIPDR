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
        private string _code = null!;
        private string _value = null!;
        private string? _valueEn;
        private string? _holderCode;

        /// <summary>
        /// Ид за Редакция на невлезли в сила записи
        /// </summary>
        public long EditId { get; set; }

        /// <summary>
        /// Редакция/Добавяне 
        /// </summary>
        public bool IsInsert { get; set; }

        [StringLength(20, MinimumLength = 1, ErrorMessage = "Максималната дължина на полето {0} е {1}")]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [Display(Name = "Код")]
        public string Code
        {
            get => _code;
            set => _code = value?.Trim();
        }

        /// <summary>
        /// Горно ниво в друг номенклатурна стойност 
        /// </summary>
        [Display(Name = "Горно ниво")]
        public string? HolderCode
        {
            get => _holderCode;
            set => _holderCode = value?.Trim();
        }

        /// <summary>
        /// Стойност
        /// </summary>
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Максималната дължина на полето {0} е {1}")]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [Display(Name = "Стойност")]
        public string Value
        {
            get => _value;
            set => _value = value?.Trim();
        }

        [StringLength(255, ErrorMessage = "Максималната дължина на полето {0} е {1}")]
        [RegularExpression(RegexPatterns.LatinTextWithNumbersPattern, ErrorMessage = MessageConstant.NotLatin)]
        [Display(Name = "Стойност EN")]
        public string? ValueEn
        {
            get => _valueEn;
            set => _valueEn = value?.Trim();
        }

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
