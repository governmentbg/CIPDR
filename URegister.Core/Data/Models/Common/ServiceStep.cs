using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using URegister.Infrastructure.Constants;

namespace URegister.Core.Data.Models.Common
{
    /// <summary>
    /// Стъпка от услуга в регистъра
    /// </summary>
    [Comment("Стъпка от услуга в регистъра")]
    public class ServiceStep
    {
        /// <summary>
        /// Системен идентификатор
        /// </summary>
        [Key]
        [Comment(AttributeConstants.Identifier)]
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на тип форма
        /// </summary>        
        [Comment("Идентификатор на тип форма")]
        [Required]
        public int FormParentId { get; set; }

        /// <summary>
        /// Име на стъпката
        /// </summary>
        [StringLength(150)]
        [Comment("Име на стъпката")]
       // [Required]
        public string? Title { get; set; }

        /// <summary>
        /// Идентификатор на услугата от регистъра
        /// </summary>        
        [Comment("Идентификатор на услугата от регистъра")]
        [Required]
        public int ServiceId { get; set; }

        /// <summary>
        /// Поредност
        /// </summary>
        [Comment("Поредност")]
        public int OrderNum { get; set; }

        /// <summary>
        /// Идентификатор на стъпка 
        /// </summary>
        [Comment("Идентификатор на стъпка")]
        public int StepId { get; set; }

        /// <summary>
        /// Външен ключ към услуга
        /// </summary>
        [ForeignKey(nameof(ServiceId))]
        [Comment("Външен ключ към услуга")]
        public virtual Service Service { get; set; } = null!;
    }
}
