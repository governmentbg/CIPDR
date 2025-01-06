using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using URegister.Infrastructure.Constants;

namespace URegister.Infrastructure.Data.Common
{
    public class EntityBaseWithLastModifiedInfo : SoftDeletable, IEntityBaseWithLastModifiedInfo
    {
        /// <summary>
        /// Идентификатор на потребилет променил последно записа
        /// </summary>
        [Comment("Идентификатор на потребилет променил последно записа")]
        [Required]
        [StringLength(36)]
        public string ModifiedByUserId { get; set; }

        /// <summary>
        /// Дата на последна промяна
        /// </summary>
        [Comment("Дата на последна промяна")]
        [Required]
        [Column(TypeName = AttributeConstants.Timestamptz)]
        public DateTime ModifiedOn { get; set; }

        //[ForeignKey(nameof(ModifiedByUserId))]
        //public virtual ApplicationUser ModifiedByUser { get; set; }
    }
}
