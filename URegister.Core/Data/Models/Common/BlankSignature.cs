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
    /// Поредност на подписване на бланка
    /// </summary>
    [Comment("Поредност на подписване на бланка")]
    public class BlankSignature: EntityBaseWithLastModifiedInfo
    {
        /// <summary>
        /// Системен идентификатор
        /// </summary>
        [Key]
        [Comment(AttributeConstants.Identifier)]
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на бланка
        /// </summary>
        [Comment("Идентификатор на бланка")]
        public int BlankTemplateId { get; set; }

        /// <summary>
        /// Поредност
        /// </summary>
        [Comment("Поредност")]
        public int OrderNum { get; set; }

        /// <summary>
        /// Подписва се от обработващия служител
        /// </summary>
        [Comment("Подписва се от обработващия служител")]
        public bool SignByOperator { get; set; }

        /// <summary>
        /// Идентификатор на роля
        /// </summary>
        [Comment("Идентификатор на роля")]
        public Guid? RoleId { get; set; }

        /// <summary>
        /// Външен ключ към бланка
        /// </summary>
        [ForeignKey(nameof(BlankTemplateId))]
        [Comment("Външен ключ към услуга")]
        public virtual BlanksTemplate BlanksTemplate { get; set; } = null!;
    }
}
