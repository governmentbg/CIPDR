using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Data.Common;

namespace URegister.Core.Data.Models.Common
{
    /// <summary>
    /// Инфорация за качен от потребител файл
    /// </summary>
    [Comment("Инфорация за качен от потребител файл")]
    public class FileMetadata : EntityBaseWithLastModifiedInfo
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        [Comment("Идентификатор")]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Идентификатор на файла в хранилището
        /// </summary>
        [Comment("Идентификатор на файла в хранилището")]
        public Guid FileId { get; set; } = Guid.Empty;

        /// <summary>
        /// Идентификатор на роля на файла
        /// </summary>
        [Comment("Идентификатор на  източник")]
        public int FileSourceTypeId { get; set; } = 0;

        /// <summary>
        /// Идентификатор на роля на файла
        /// </summary>
        [Comment("Идентификатор на източник")]
        public string? SourceId { get; set; }

        /// <summary>
        /// Име на файла
        /// </summary>
        [Comment("Име на файла")]
        [StringLength(255, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Подпис
        /// </summary>
        [Comment("Подпис")]
        [StringLength(50, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        public string? Signature { get; set; } = null;

        /// <summary>
        /// Алгоритъм за изчисляване на хеш сума
        /// </summary>
        [Comment("Алгоритъм за изчисляване на хеш сума")]
        [StringLength(100, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        public string? HashingAlgorithm { get; set; } = null;

        /// <summary>
        /// Хеш сума
        /// </summary>
        [Comment("Хеш сума")]
        [StringLength(64, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        public string? Hash { get; set; } = null;

        /// <summary>
        /// Идентификатор на заявена услуга, по-която е качен файла
        /// </summary>
        [Comment("Идентификатор на заявена услуга, по-която е качен файла")]
        public Guid? ProcessId { get; set; } = null;

        [ForeignKey(nameof(ProcessId))]
        public virtual Process.Process Process { get; set; }

        /// <summary>
        /// Идентификатор на е-форма, ако файлът е заявление подадено чрез е-форма
        /// </summary>
        [Comment("Идентификатор на е-форма, ако файлът е заявление подадено чрез е-форма")]
        public Guid? EFormId { get; set; } = null;

        /// <summary>
        /// Дата на попълване на е-формата, ако файлът е заявление подадено чрез е-форма
        /// </summary>
        [Comment("Дата на попълване на е-формата, ако файлът е заявление подадено чрез е-форма")]
        [Column(TypeName = AttributeConstants.Timestamptz)]
        public DateTime? EFormDateOfFill { get; set; } = null;

        /// <summary>
        /// описание
        /// </summary>
        [Column("Oписание")]
        public string? Description { get; set; }
    }
}
