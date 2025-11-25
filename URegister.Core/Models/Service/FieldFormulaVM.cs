using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using URegister.Infrastructure.Constants;

namespace URegister.Core.Models.Service
{
    public class FieldFormulaVM
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Поле за резултат (скрито или само за четене поле от тип 'число' или 'българска валутна')
        /// </summary>
        [Display(Name = "Поле за резултат (скрито или само за четене поле от тип 'число' или 'българска валутна')")]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [MaxLength(256, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        public string TargetField { get; set; } = null!;

        //public string FieldName { get; set; } = null!;

        ///// <summary>
        ///// Наименование на публично поле
        ///// </summary>
        //[Display(Name = "Наименование при визуализиране")]
        //[Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        //[MaxLength(256, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        //public string Label { get; set; } = null!;

        ///// <summary>
        ///// Съдържание на бланка
        ///// </summary>
        //[MaxLength(1000, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        //public string? Content { get; set; }

        /// <summary>
        /// Формула
        /// </summary>
        [Required(ErrorMessage = MessageConstant.FieldIsRequiredNoParam)]
        [DisplayName("Формула")]
        public string? FormulaText { get; set; }

        public int FormParentId { get; set; }
        public int Priority { get; set; }
    }
}
