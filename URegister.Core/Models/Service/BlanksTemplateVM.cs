using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Infrastructure.Constants;

namespace URegister.Core.Models.Service
{
    public class BlanksTemplateVM
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на услуга
        /// </summary>
        [Display(Name = "Услуга")]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        public int? ServiceId { get; set; }

        /// <summary>
        /// Идентификатор на форма
        /// </summary>        
        [Display(Name = "Тип форма")]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        public int FormParentId { get; set; }

        [StringLength(100, MinimumLength = 2, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [Display(Name = "Код")]
        public string Code { get; set; } = null!;

        /// <summary>
        /// Име
        /// </summary>
        [Display(Name = "Име")]
        [StringLength(255, MinimumLength = 2, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Име на услуга
        /// </summary>        
        public string? ServiceName { get; set; }

        /// <summary>
        /// Име на форма
        /// </summary>        
        public string? FormName { get; set; }

        /// <summary>
        /// Тип бланка
        /// </summary>        
        [Display(Name = "Тип бланка")]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        public int SourceType { get; set; }

        /// <summary>
        /// Име на Тип бланка
        /// </summary>        
        public string? SourceTypeName { get; set; }


        /// <summary>
        /// Генериране на регистров номер за бланката
        /// </summary>
        [Display(Name = "Генериране на регистров номер за бланката")]
        public bool HasRegisterNumber { get; set; }

        /// <summary>
        /// Бланката се подпечатва
        /// </summary>
        [Display(Name = "Бланката се подпечатва")]
        public bool HasStamp { get; set; }




        public List<BlankSignatureVM> BlankSignatures { get; set; } = new();
    }
}
