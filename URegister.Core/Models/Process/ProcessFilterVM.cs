using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Model.RegisterForms;

namespace URegister.Core.Models.Process
{
    public class ProcessFilterVM
    {
        /// <summary>
        /// Входящ номер
        /// </summary>
        [MaxLength(20)]
        [Display(Name ="Входящ номер")]
        public string? IncomingNumber { get; set; } 

        /// <summary>
        /// Номер на вписване 
        /// </summary>
        [Display(Name = "Номер на вписване ")]
        public string? RegisterNumber { get; set; }

        /// <summary>
        /// Дата на входиране
        /// </summary>
        [Display(Name = "Дата на входиране")]
        public DateTime? IncomingDate { get; set; }

        /// <summary>
        /// Идентификатор на услуга
        /// </summary>
        [Display(Name = "Услуга")]
        public int ServiceId { get; set; }

        /// <summary>
        /// Идентификатор на стъпка
        /// </summary>
        [Display(Name = "Стъпка")]
        public int StepId { get; set; }

        /// <summary>
        /// Идентификатор на лице
        /// </summary>
        [MaxLength(20)]
        [Display(Name = "Идентификатор на лице")]
        public string? Pid { get; set; } 

        /// <summary>
        /// Тип на идентификатора
        /// </summary>
        [MaxLength(2)]
        public string? PidType { get; set; }
    }
}
