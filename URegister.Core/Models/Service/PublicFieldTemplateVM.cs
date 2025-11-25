using System.ComponentModel.DataAnnotations;
using URegister.Infrastructure.Constants;

namespace URegister.Core.Models.Service
{
    public class PublicFieldTemplateVM
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Поредност
        /// </summary>
        public int OrderNum { get; set; }

        [Display(Name = "Поле")]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [MaxLength(256, ErrorMessage = MessageConstant.StringMaxLengthValidation)]

        public string FieldName { get; set; } = null!;

        /// <summary>
        /// Наименование на публично поле
        /// </summary>
        [Display(Name = "Наименование при визуализиране")]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [MaxLength(256, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        public string Label { get; set; } = null!;

        /// <summary>
        /// Съдържание на бланка
        /// </summary>
        [MaxLength(1000, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        public string? Content { get; set; }

        /// <summary>
        /// Съдържание на бланка в текст
        /// </summary>
        public string? ContentText { get; set; }

    }
}
