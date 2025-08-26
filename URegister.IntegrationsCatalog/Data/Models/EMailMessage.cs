using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using URegister.Infrastructure.Data.Common;

namespace URegister.IntegrationsCatalog.Data.Models
{
    /// <summary>
    /// e-mail съобщения
    /// </summary>
    public class EMailMessage : EntityBaseWithLastModifiedInfo
    {
        // <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        [Comment("Идентификатор")]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Статус
        /// </summary>
        [Comment("Статус")]
        public int StatusId { get; set; }

        /// <summary>
        /// Грешка
        /// </summary>
        [Comment("Грешка")]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Брой грешки
        /// </summary>
        [Comment("Брой грешки")]
        public int ErrorCount { get; set; }

        /// <summary>
        /// Вид връзка
        /// </summary>
        [Comment("Вид връзка")]
        public int SourceType { get; set; }

        /// <summary>
        /// Идентификатор на връзка
        /// </summary>
        [Comment("Идентификатор на връзка")]
        public Guid? SourceId { get; set; }

        /// <summary>
        /// Съобщение
        /// </summary>
        [Comment("Съобщение")]
        public string Message { get; set; } = null!;
        /// <summary>
        /// Subject in mail
        /// </summary>
        public string Subject { get; set; } = null!;

        /// <summary>
        /// е-маил адрес
        /// </summary>
        [Comment("е-маил адрес")]
        public string EMail { get; set; } = null!;
        /// <summary>
        /// Получател
        /// </summary>
        [Comment("Получател")]
        public string? PersonName { get; set; }
    }
}
