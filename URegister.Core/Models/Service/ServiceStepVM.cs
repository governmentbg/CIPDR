using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace URegister.Core.Models.Service
{
    public class ServiceStepVM
    {
        /// <summary>
        /// Идентификатор на стъпка
        /// </summary>
        [Display(Name = "Идентификатор на стъпка")]
        public int Id { get; set; }

        // <summary>
        /// Поредност
        /// </summary>
        [Display(Name = "Поредност")]
        [Required]
        public int OrderNum { get; set; }

        /// <summary>
        /// Стъпка 
        /// </summary>
        [Display(Name = "Стъпка")]
        [Required]
        public int StepId { get; set; }

        /// <summary>
        /// Идентификатор на тип форма
        /// </summary>        
        [Display(Name ="Идентификатор на тип форма")]
        [Required]
        public int FormParentId { get; set; }

        public int Index { get; set; }

    }
}
