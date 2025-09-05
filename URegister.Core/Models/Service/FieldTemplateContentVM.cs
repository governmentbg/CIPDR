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
    public class FieldTemplateContentVM
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Име
        /// </summary>
        public string Name { get; set; } = null!;
 
        /// <summary>
        /// Идентификатор на тип форма
        /// </summary>        
        public int FieldTypeId { get; set; }

        /// <summary>
        /// Съдържание на бланка
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// Съдържание на бланка текст
        /// </summary>
        public string? ContentText { get; set; }


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
        public bool BlankIfNoValue { get; set; }
    }
}
