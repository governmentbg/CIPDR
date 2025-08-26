using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URegister.Core.Models.Previewer
{
    /// <summary>
    /// Модел за файлове
    /// </summary>
    public class FileViewModel
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
        /// Идентификатор на файл в обектното хранилище
        /// </summary>
        public string? FileId { get; set; }

        /// <summary>
        /// Файл в byte[]
        /// </summary>
        public byte[] FileByteArray { get; set; }

        /// <summary>
        /// Файл в Base64
        /// </summary>
        public string? FileContentBase64 { get; set; }

        /// <summary>
        /// Име на файла
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Кратко описание
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Вид на файла
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Допълнителна информация за приложеният файл
        /// </summary>
        public string? Remark { get; set; }

        /// <summary>
        /// Флаг дали файлът е подписан
        /// </summary>
        public bool IsSigned { get; set; } = false;

        /// <summary>
        /// Подпис на файл, ако файлът е подписан разкачено
        /// </summary>
        public string? Signature { get; set; } = null;

        /// <summary>
        /// Размер на файл
        /// </summary>
        public long Size { get; set; }
    }
}
