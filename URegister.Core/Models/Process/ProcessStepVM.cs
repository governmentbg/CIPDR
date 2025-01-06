using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Infrastructure.Model.RegisterForms;

namespace URegister.Core.Models.Process
{
    public class ProcessStepVM : FormViewModel
    {
        public Guid ProcessId { get; set; }
         public int ServiceStepId { get; set; }

        /// <summary>
        /// Поредност
        /// </summary>
        public int OrderNum { get; set; }
    }
}
