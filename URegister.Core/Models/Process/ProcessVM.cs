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
    public class ProcessVM
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Идентификатор на първоначален процесс
        /// </summary>
        public Guid? FromProcessId { get; set; }
        ///// <summary>
        ///// Входящ номер
        ///// </summary>
        //[Required]
        //[MaxLength(20)]
        //[Display(Name ="Входящ номер")]
        public string? IncomingNumber { get; set; } = null;

        ///// <summary>
        ///// Номер на вписване 
        ///// </summary>
        //[Display(Name = "Номер на вписване ")]
        //public string? RegisterNumber { get; set; }

        ///// <summary>
        ///// Дата на входиране
        ///// </summary>
        //[Display(Name = "Дата на входиране")]
        public DateTime? IncomingDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Стар номер на вписване 
        /// </summary>
        [Display(Name = "Стар номер на вписване")]

        public string? OldIncomingNumber { get; set; }

        [Display(Name = "Стара дата на входиране")]
        public DateTime? OldIncomingDate { get; set; }

        /// <summary>
        /// Идентификатор на услуга
        /// </summary>
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [Display(Name = "Услуга")]
        public int? ServiceId { get; set; }

    }
}
