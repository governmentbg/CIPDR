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
    public class BlanksTemplateContentVM
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
        public int FormParentId { get; set; }

        /// <summary>
        /// Идентификатор на услуга
        /// </summary>
        public int? ServiceId { get; set; }

        /// <summary>
        /// Идентификатор на услуга
        /// </summary>
        public int SourceType { get; set; }

        /// <summary>
        /// Съдържание на бланка
        /// </summary>
        public string? Content { get; set; }

    }
}
