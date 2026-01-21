using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URegister.Core.Models.Common
{
    public class FileForSignVM
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Идентификатор на процесс
        /// </summary>
        public Guid ProcessId { get; set; }

        /// <summary>
        /// Име на файл
        /// </summary>
        [Display(Name = "Име на файл")]
        public string? FileName { get; set; }

        /// <summary>
        /// Тип файл
        /// </summary>
        [Display(Name = "Вид")]
        public string? TypeName { get; set; }


        /// <summary>
        /// описание
        /// </summary>
        [Display(Name = "Oписание")]
        public string? Description { get; set; }

        public Guid? RoleId { get; set; }

    }
}
