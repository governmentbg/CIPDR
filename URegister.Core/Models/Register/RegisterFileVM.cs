using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URegister.Core.Models.Register
{
    public class RegisterFileVM
    {
        public Guid? MetaFileId { get; set; }

        /// <summary>
        /// Type of NomenclatureType
        /// </summary>
        public string? NomenclatureType { get; set; }

        /// <summary>
        /// Type of NomenclatureType
        /// </summary>
        public List<SelectListItem> CodeableConceptDdl { get; set; } = new();

        /// <summary>
        /// Код от CodeableConcept
        /// </summary>
        [Display(Name = "Тип файл")]
        public string? CodeableConceptCode { get; set; }

        /// <summary>
        /// описание
        /// </summary>
        [Display(Name = "Oписание")]
        public string? Description { get; set; }

        public string? FileName { get; set; }

        public int Index { get; set; }
    }
}
