using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using URegister.Core.Models.Common;

namespace URegister.Core.Models.Service
{
    public class ServiceVM
    {
        /// <summary>
        /// Идентификатор на тип на услуга
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Име на тип на услуга
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Display(Name = "Име на услуга")]
        public string Name { get; set; } = null!;

        // <summary>
        /// Идентификатор на тип услуга
        /// </summary>
        [Required]
        [Display(Name = "Тип услуга")]
        public int ServiceTypeId { get; set; }

        /// <summary>
        /// Стъпки
        /// </summary>
        [Display(Name = "Стъпки")]
        public List<ServiceStepVM> Steps { get; set; } = new();

        public bool IsInsert => Id <= 0;
    }
}
