using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using URegister.Infrastructure.Constants;

namespace URegister.Infrastructure.Data.Common
{
    public interface IEntityBaseWithLastModifiedInfo
    {
        /// <summary>
        /// Идентификатор на потребителят променил последно записа
        /// </summary>
        [Comment("Идентификатор на потребителят променил последно записа")]
        [Required]
        public Guid ModifiedByUserId { get; set; }

        /// <summary>
        /// Дата на последна промяна
        /// </summary>
        [Comment("Дата на последна промяна")]
        [Column(TypeName = AttributeConstants.Timestamptz)]
        public DateTime ModifiedOn { get; set; }
    }
}
