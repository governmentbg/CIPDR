using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Data.Common;

namespace URegister.Core.Models.Register
{
    /// <summary>
    /// Статуси на регистър
    /// </summary>
     public class RegisterStatusVM 
     {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Идентификатор на регистър
        /// </summary>
        public int RegisterId { get; set; }

        /// <summary>
        /// Идентификатор на статус
        /// </summary>
        [Display(Name = "Идентификатор на статус")]
        public int StatusId { get; set; }

        /// <summary>
        /// Забележка
        /// </summary>
        [MaxLength(1000)]
        [Display(Name = "Забележка")]
        public string? Remark { get; set; }

        public RegisterFileListVM RegisterFiles { get; set; } = new()
        {
            FilesLabel = "Прикачени файлове"
        };

    }
}
