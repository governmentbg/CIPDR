using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URegister.Core.Models.OpenData
{
    public class OpenDataAdministrationVM
    {
        public string? AdministrationName { get; set; }
        /// <summary>
        /// api-key за opendata
        /// </summary>
        [Display(Name = "api-key")]
        public string? ApiKey { get; set; }

        /// <summary>
        /// Идентификатор на организация в  opendata
        /// </summary>
        [Display(Name = "Идентификатор на организация")]
        public int OrganizationId { get; set; }

        /// <summary>
        /// Идентификатор на организация в  opendata
        /// </summary>
        [Display(Name = "Идентификатор на организация")]
        public Guid AdministrationId { get; set; }

        /// <summary>
        /// Идентификатор на регистър
        /// </summary>
        [Display(Name = "Идентификатор на регистър")]
        public int RegisterId { get; set; }

        

        /// <summary>
        /// Честота на изпращане на данни
        /// </summary>
        [Display(Name = "Честота на изпращане на данни")]
        public int FrequencyId { get; set; }

        /// <summary>
        /// Честота на изпращане на данни на ниво АДМИНИСТРАЦИЯ
        /// </summary>
        [Display(Name = "Честота на изпращане на данни на ниво АДМИНИСТРАЦИЯ")]
        public int FrequencyAdministrationId { get; set; }
    }
}
