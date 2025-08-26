using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Infrastructure.Constants;

namespace URegister.Core.Models.Service
{
    public class FieldTemplateVM
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на поле
        /// </summary>
        [Display(Name = "Поле")]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        public int FieldTypeId { get; set; }

        /// <summary>
        /// Име
        /// </summary>
        [Display(Name = "Име")]
        [StringLength(255, MinimumLength = 2, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Име на поле
        /// </summary>        
        public string? FieldTypeName { get; set; }

        /// <summary>
        /// Тип на поле
        /// </summary>        
        public string? FieldType { get; set; }

        /// <summary>
        /// Празен резултат ако няма стойност за полето
        /// </summary>        
        [Display(Name = "Темплейта се визуализира замо при попълнени данни")]
        public bool BlankIfNoValue { get; set; }
    }
}
