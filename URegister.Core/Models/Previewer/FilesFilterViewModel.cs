using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URegister.Core.Models
{
    /// <summary>
    /// Модел за филтър за файлове
    /// </summary>
    public class FilesFilterViewModel
    {
        /// <summary>
        /// Тип на файла
        /// </summary>
        public int SourceType { get; set; }

        /// <summary>
        /// Идентификатор на обект, към който е закачен файла
        /// </summary>
        public Guid SourceId { get; set; }

        /// <summary>
        /// Линк за връщане
        /// </summary>
        public string BackUrl { get; set; }
    }
}
