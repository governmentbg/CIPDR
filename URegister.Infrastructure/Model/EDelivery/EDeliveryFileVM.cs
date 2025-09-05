using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URegister.Infrastructure.Model.EDelivery
{
    public class EDeliveryFileVM
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Име на файл
        /// </summary>
        public string? FileName { get; set; }

        public string? FileUrl { get; set; }

        public int FileSourceTypeId { get; set; }
    }
}
