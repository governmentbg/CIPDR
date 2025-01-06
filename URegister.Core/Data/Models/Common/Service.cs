using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using URegister.Infrastructure.Constants;

namespace URegister.Core.Data.Models.Common
{
    /// <summary>
    /// Услуга в регистъра
    /// </summary>
    [Comment("Услуга в регистъра")]
    public class Service
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

        // <summary>
        /// Идентификатор на тип услуга
        /// </summary>
        [Comment("Идентификатор на тип услуга")]
        public int ServiceTypeId { get; set; }

        /// <summary>
        /// Стъпки към услуга
        /// </summary>
        public List<ServiceStep> ServiceSteps { get; set; } = new();
    }
}
