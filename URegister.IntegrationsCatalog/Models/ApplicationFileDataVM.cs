using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using URegister.Infrastructure.Constants;

namespace URegister.IntegrationsCatalog.Models
{
    public class ApplicationFileDataVM
    {
        /// <summary>
        /// Информация от пдф
        /// </summary>
        public string? ApplicationJson { get; set; }

        /// <summary>
        /// Информация от пдф
        /// </summary>
        public string? ApplicationSubmission { get; set; }

        /// <summary>
        /// Референтен номер на услуга (РНУ)
        /// </summary>
        public string? Rnu { get; set; }

        /// <summary>
        /// ЕИК на администрация
        /// </summary>
        public string? AdministrationUic { get; set; }
  
        /// <summary>
        /// Код на услуга
        /// </summary>
        public string? ServiceCode { get; set; }
    }
}
