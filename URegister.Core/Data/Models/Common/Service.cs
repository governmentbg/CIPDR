using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Data.Common;

namespace URegister.Core.Data.Models.Common
{
    /// <summary>
    /// Услуга в регистъра
    /// </summary>
    [Comment("Услуга в регистъра")]
    public class Service: EntityBaseWithLastModifiedInfo
    {
        /// <summary>
        /// Системен идентификатор
        /// </summary>
        [Key]
        [Comment(AttributeConstants.Identifier)]
        public int Id { get; set; }

        /// <summary>
        /// Име на услугата
        /// </summary>
        [StringLength(150)]
        [Required]
        [Comment("Име на услугата")]
        public string Title { get; set; } = null!;

        /// <summary>
        /// Идентификатор на тип услуга
        /// </summary>
        [Comment("Идентификатор на тип услуга")]
        public int ServiceTypeId { get; set; }

        /// <summary>
        /// Идентификатор на тип форма
        /// </summary>        
        [Comment("Идентификатор на тип форма")]
        [Required]
        public int FormParentId { get; set; }

        /// <summary>
        /// Референтен номер на услуга (РНУ)
        /// </summary>
        [Comment("Референтен номер на услуга (РНУ)")]
        public string? EFormCode { get; set; }

        /// <summary>
        /// Стъпки към услуга
        /// </summary>
        public List<ServiceStep> ServiceSteps { get; set; } = new();
    }
}
