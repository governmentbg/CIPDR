using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Data.Common;

namespace URegister.Core.Models.Process
{
     public class ProcessDeliveryVM 
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();


        /// <summary>
        /// Идентификатор на източник
        /// </summary>
        public int SourceTypeId { get; set; } = 0;

        /// <summary>
        /// Идентификатор на роля на файла
        /// </summary>
        public string? SourceId { get; set; }

        /// <summary>
        /// Идентификатор на заявена услуга
        /// </summary>
        public Guid ProcessId { get; set; }

        /// <summary>
        /// Начин на връчване
        /// </summary>
        [Display(Name = "Начин на връчване")]
        public string? ChannelId { get; set; }

        /// <summary>
        /// описание
        /// </summary>
        [Column("Oписание")]
        public string? Description { get; set; }

        /// <summary>
        /// Дата на връчване
        /// </summary>
        [Display(Name = "Дата на връчване")]
        public DateTime? DeliveryDate { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        public int StatusId { get; set; }

        public string? Channel { get; set; }
        public string? Source { get; set; }
        public string? Status { get; set; }
    }
}