using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Data.Common;

namespace URegister.Core.Data.Models.Common
{
    /// <summary>
    /// Темплейти на публични полета
    /// </summary>
    public class PublicFieldTemplate : EntityBaseWithLastModifiedInfo
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        [Comment("Идентификатор")]
        public int Id { get; set; }

        /// <summary>
        /// Поредност
        /// </summary>
        [Comment("Поредност")]
        public int OrderNum { get; set; }


        /// <summary>
        /// Наименование на публично поле
        /// </summary>
        [Comment("Име напле в Json")]
        [Required]
        public string FieldName { get; set; } = null!;

        /// <summary>
        /// Наименование на публично поле
        /// </summary>
        [Comment("Наименование на публично поле")]
        [Required]
        public string Label { get; set; } = null!;


        /// <summary>
        /// Съдържание на бланка
        /// </summary>
        [Comment("Съдържание на бланка")]

        public string? Content { get; set; }

        /// <summary>
        /// Дата на създаване
        /// </summary>
        [Required]
        [Column(TypeName = AttributeConstants.Timestamptz)]
        [Comment("Дата на създаване")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Създадена от
        /// </summary>
        [Comment("Създадена от")]
        public string? CreatedBy { get; set; }
    }
}
