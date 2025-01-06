using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using URegister.Infrastructure.Constants;

namespace URegister.Infrastructure.Data.Common
{
    public interface IEntityBaseWithLastModifiedInfo
    {
        /// <summary>
        /// Идентификатор на потребилет променил последно записа
        /// </summary>
        [Comment("Идентификатор на потребилет променил последно записа")]
        [StringLength(36)]
        public string ModifiedByUserId { get; set; }

        /// <summary>
        /// Дата на последна промяна
        /// </summary>
        [Comment("Дата на последна промяна")]
        [Column(TypeName = AttributeConstants.Timestamptz)]
        public DateTime ModifiedOn { get; set; }
    }
}
