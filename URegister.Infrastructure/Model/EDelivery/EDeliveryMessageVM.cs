using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URegister.Infrastructure.Model.EDelivery
{
    public class EDeliveryMessageVM
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Вид съобщение
        /// </summary>
        [Comment("Вид съобщение")]
        public int MessageTypeId { get; set; }

        /// <summary>
        /// Идентификатор на заявена услуга
        /// </summary>
        public Guid? ProcessId { get; set; }

        /// <summary>
        /// Вид връзка
        /// </summary>
        public int SourceType { get; set; }

        /// <summary>
        /// Идентификатор на връзка
        /// </summary>
        public Guid? SourceId { get; set; }

        public int RegisterId { get; set; }

        public string? Content { get; set; }

        /// <summary>
        /// Файлове в съобщение
        /// </summary>
        public List<EDeliveryFileVM> EDeliveryFiles { get; set; } = new();
    }
}
