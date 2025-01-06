using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using URegister.Core.Models.Common;

namespace URegister.Admin.Models.Service
{
    public class ServiceTypeVM
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
        [Display(Name = "Име на тип на услуга")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Услуги
        /// </summary>
        [Display(Name = "Услуги")]
        public List<ChecklistItemViewModel> Steps { get; set; } = new();

        public bool IsInsert => Id <= 0;
    }
}
