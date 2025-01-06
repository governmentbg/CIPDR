using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using URegister.Core.Models.Common;

namespace URegister.Core.Models.Service
{
    public class ServiceListItemVM
    {
        /// <summary>
        /// Идентификатор на тип на услуга
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Име на тип на услуга
        /// </summary>
        public string? Title { get; set; }

        // <summary>
        /// Тип услуга
        /// </summary>
        public string? ServiceType { get; set; }

        // <summary>
        /// Идентификатор тип услуга
        /// </summary>
        public int ServiceTypeId { get; set; }
    }
}
