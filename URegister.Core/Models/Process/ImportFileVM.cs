using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Infrastructure.Constants;

namespace URegister.Core.Models.Process
{
    public class ImportFileVM
    {
        /// <summary>
        /// Идентификатор на услуга
        /// </summary>
        [Display(Name = "Услуга")]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        public int? ServiceId { get; set; }

        public string? FileId { get; set; }
        public string? ServiceName { get; set; }
        public string? FormName { get; set; }
    }
}
