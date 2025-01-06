using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URegister.Core.Models.CurrentRegister
{
    public class PersonFilterVM
    {
        /// <summary>
        /// Идентификатор на администрация
        /// </summary>
        public Guid AdministrationId { get; set; }
    }
}
