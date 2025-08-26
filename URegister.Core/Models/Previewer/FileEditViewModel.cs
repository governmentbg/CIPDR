using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URegister.Core.Models.Previewer
{
    /// <summary>
    /// Модел за добавяне на файл
    /// </summary>
    public class FileEditViewModel
    {
        /// <summary>
        /// Системен идентификатор
        /// </summary>
        public Guid? Id { get; set; }

        /// <summary>
        /// Тип на файла
        /// </summary>
        public int SourceType { get; set; }

        /// <summary>
        /// Идентификатор на обект, към който е закачен файла
        /// </summary>
        public Guid SourceId { get; set; }

        /// <summary>
        /// Кратко описание
        /// </summary>
        [Display(Name = "Пояснение")]
        public string? FileTitle { get; set; }

        /// <summary>
        /// Допълнителна информация за приложеният файл
        /// </summary>
        [Display(Name = "Допълнителна информация за приложеният файл")]
        public string? Remark { get; set; }

        /// <summary>
        /// Тип файл
        /// </summary>
        [Display(Name = "Тип файл")]
        public long? FileTypeId { get; set; }

        /// <summary>
        /// Файл
        /// </summary>
        public FileUploadViewModel FileUpload { get; set; } = new();

        /// <summary>
        /// Път за връщане
        /// </summary>
        public string BackUrl { get; set; }
    }
}
