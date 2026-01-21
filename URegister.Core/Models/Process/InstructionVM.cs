using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace URegister.Core.Models.Process
{
    public class InstructionVM
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Id { get; set; }


        public Guid ProcessId { get; set; }

        /// <summary>
        /// Съдържание
        /// </summary>
        [Display(Name = "Указания")]
        public string Content { get; set; } = null!;

        /// <summary>
        /// Идентификатор на потребител дал указания
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Дадено от
        /// </summary>
        [Display(Name = "Дадено от")]
        public string? UserName { get; set; } 

        /// <summary>
        /// От дата
        /// </summary>
        [Display(Name = "От дата")]
        public DateTime InstructionDate { get; set; }

        public bool HasResponse { get; set; }

        public Guid? FileId  { get; set; }

        public bool CanAdd { get; set; }

        /// <summary>
        /// Начини на предоставяне на резултата"
        /// </summary>
        public string? ResultDeliveryMethod { get; set; } 
    }
}
