using Microsoft.AspNetCore.Http;
using System.ComponentModel;

namespace URegister.Core.Models.Previewer
{
    public class FileUploadViewModel
    {
        /// <summary>
        /// Типовете поддържани файлове за качвне, разделени с ','. Пр.: "image/png, image/jpeg"
        /// </summary>
        public string AcceptableFileTypes { get; set; } = string.Empty;

        [DisplayName("Заповед за назначаване")]
        public IFormFile File { get; set; } = null;
    }
}
