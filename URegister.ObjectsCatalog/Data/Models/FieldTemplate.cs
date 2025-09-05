using Google.Protobuf.WellKnownTypes;
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

namespace URegister.ObjectsCatalog.Data.Models
{
    /// <summary>
    /// Темплейти на бланки
    /// </summary>
    public class FieldTemplate : SoftDeletable
    {

        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        [Comment("Идентификатор")]
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на поле
        /// </summary>
        [Comment("Идентификатор на поле")]
        public int FieldTypeId { get; set; }

        /// <summary>
        /// Поле
        /// </summary>
        [ForeignKey(nameof(FieldTypeId))]
        public FieldType FieldType { get; set; } = null!;

        /// <summary>
        /// Име
        /// </summary>
        [Comment("Име")]
        [StringLength(255)]
        [Required]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Съдържание на бланка
        /// </summary>
        [Comment("Съдържание на бланка")]

        public string? Content { get; set; }

        /// <summary>
        /// Съдържание на бланка
        /// </summary>
        [Comment("Съдържание на бланка текст")]

        public string? ContentText { get; set; }

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


        /// <summary>
        /// Дата на създаване
        /// </summary>
        [Column(TypeName = AttributeConstants.Timestamptz)]
        [Comment("Дата на създаване")]
        public DateTime? ModifiedOn { get; set; }

        /// <summary>
        /// Създадена от
        /// </summary>
        [Comment("Създадена от")]
        public string? ModifiedBy { get; set; }


        /// <summary>
        /// Празен резултат ако няма стойност за полето
        /// </summary>        
        [Comment("Празен резултат ако няма стойност за полето")]
        public bool BlankIfNoValue { get; set; }
    }
}
