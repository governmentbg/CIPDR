using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URegister.Core.Models.OpenData
{
    public class OpenDataRegisterVM
    {
        /// <summary>
        /// Категория в opendata
        /// </summary>
        [Display(Name = "Категория в opendata")]
        public int CategoryId { get; set; }
    }
}
