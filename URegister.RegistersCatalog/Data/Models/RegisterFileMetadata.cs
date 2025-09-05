using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Data.Common;

namespace URegister.RegistersCatalog.Data.Models
{
    /// <summary>
    /// Съдържа конфигурацияна на полетата във форма
    /// </summary>
    [Comment("Инфорация за качен от потребител файл")]
    public class RegisterFileMetadata : EntityBaseWithLastModifiedInfo
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
        [Comment("Идентификатор на роля на файла")]
        public int FileSourceTypeId { get; set; }

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
        public string? Signature { get; set; }

        /// <summary>
        /// Алгоритъм за изчисляване на хеш сума
        /// </summary>
        [Comment("Алгоритъм за изчисляване на хеш сума")]
        [StringLength(100, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        public string? HashingAlgorithm { get; set; } 

        /// <summary>
        /// Хеш сума
        /// </summary>
        [Comment("Хеш сума")]
        [StringLength(64, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        public string? Hash { get; set; } = null;

        /// <summary>
        /// Идентификатор на сорс
        /// </summary>
        [Comment("Идентификатор на сорс")]
        public string? SourceId { get; set; }

        public int? RegisterId { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        [Comment("Описание")]
        public string? Description { get; set; } = null;

        /// <summary>
        /// Type of NomenclatureType
        /// </summary>
        [Comment("Type of NomenclatureType")]
        public string? NomenclatureType { get; set; }

        /// <summary>
        /// Код от CodeableConcept
        /// </summary>
        [Comment("Тип файл")]
        public string? CodeableConceptCode { get; set; }
    }
}
