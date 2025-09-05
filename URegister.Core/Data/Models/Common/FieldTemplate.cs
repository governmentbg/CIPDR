using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Infrastructure.Constants;

namespace URegister.Core.Data.Models.Common
{
    public class FieldTemplate
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Comment("Идентификатор")]
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на поле
        /// </summary>        
        [Comment("Идентификатор на поле")]
        public Guid FieldId { get; set; }

        [StringLength(100)]
        [Required]
        [Comment("Код")]
        public string Code { get; set; } = null!;

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

        // <summary>
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

    }
}
