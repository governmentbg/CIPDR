using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URegister.Admin.Models.Register
{
    public class PersonFilterVM
    {
        /// <summary>
        /// Идентификатор на регистър
        /// </summary>
        public int RegisterId { get; set; }

        /// <summary>
        /// Идентификатор на администрация
        /// </summary>
        public Guid AdministrationId { get; set; }
    }
}
