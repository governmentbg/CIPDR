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
    public class ProcessListItemVM
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Id { get; set; } 

        /// <summary>
        /// Входящ номер
        /// </summary>
        public string? IncomingNumber { get; set; } 

        /// <summary>
        /// Номер на вписване 
        /// </summary>
        public string? RegisterNumber { get; set; }

        /// <summary>
        /// Дата на входиране
        /// </summary>
        public DateTime IncomingDate { get; set; }

        /// <summary>
        /// Услуга
        /// </summary>
        public string? ServiceName { get; set; }

        /// <summary>
        /// Стъпка
        /// </summary>
        public string? StepName { get; set; }

        /// <summary>
        /// Идентификатор стъпка 
        /// </summary>
        public int StepId { get; set; }

    }
}
