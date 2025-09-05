using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URegister.Core.Models.Common
{
    public class FileVM
    {
        public int Index { get; set; }
        /// <summary>
        /// Име на файл
        /// </summary>
        public string? FileName { get; set; }
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid? MetaFileId { get; set; }

        /// <summary>
        /// описание
        /// </summary>
        [Display(Name = "Oписание")]
        public string? Description { get; set; }

    }
}
