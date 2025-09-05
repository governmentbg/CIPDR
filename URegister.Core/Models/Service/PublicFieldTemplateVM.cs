using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        [Display(Name = "FieldName")]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [MaxLength(100, ErrorMessage = MessageConstant.StringMaxLengthValidation)]

        public string FieldName { get; set; } = null!;

        /// <summary>
        /// Наименование на публично поле
        /// </summary>
        [Display(Name = "Наименование")]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [MaxLength(100, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        public string Label { get; set; } = null!;

        /// <summary>
        /// Съдържание на бланка
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// Съдържание на бланка в текст
        /// </summary>
        public string? ContentText { get; set; }

    }
}
