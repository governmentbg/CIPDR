using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Data.Common;

namespace URegister.Core.Data.Models.Common
{
    /// <summary>
    /// Стъпка от услуга в регистъра
    /// </summary>
    [Comment("Стъпка от услуга в регистъра")]
    public class StepRole : EntityBaseWithLastModifiedInfo
    {
        /// <summary>
        /// Системен идентификатор
        /// </summary>
        [Key]
        [Comment(AttributeConstants.Identifier)]
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на стъпка 
        /// </summary>
        [Comment("Идентификатор на стъпка")]
        public int ServiceStepId { get; set; }

        /// <summary>
        /// Идентификатор на роля
        /// </summary>
        [Comment("Идентификатор на роля")]
        public Guid RoleId { get; set; }

        /// <summary>
        /// Външен ключ към услуга
        /// </summary>
        [ForeignKey(nameof(ServiceStepId))]
        [Comment("Външен ключ към услуга")]
        public virtual ServiceStep ServiceStep { get; set; } = null!;
    }
}
