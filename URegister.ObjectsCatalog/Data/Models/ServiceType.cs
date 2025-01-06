using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace URegister.ObjectsCatalog.Data.Models
{
    /// <summary>
    /// Тип на услуга
    /// </summary>
    [Comment("Тип на услуга")]
    public class ServiceType
    {
        /// <summary>
        /// Идентификатор на тип на услуга
        /// </summary>
        [Key]
        [Comment("Идентификатор на тип на услуга")]
        public int Id { get; set; }

        /// <summary>
        /// Име на тип на услуга
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Comment("Име на тип на услуга")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Стъпки към вид услуга
        /// </summary>
        public List<ServiceTypeStep> ServiceTypeSteps { get; set; } = new List<ServiceTypeStep>();
    }
}
