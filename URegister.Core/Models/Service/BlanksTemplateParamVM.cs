using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URegister.Core.Models.Service
{
    public class BlanksTemplateParamVM
    {
        /// <summary>
        /// Име на параметър
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Етикет на параметър
        /// </summary>
        public string? Label { get; set; }

        /// <summary>
        /// Име на параметър
        /// </summary>
        public string? Type { get; set; }

        public bool Repeatable {get; set; }
        public List<BlanksTemplateParamVM>? Templates { get; set; }
    }
}
