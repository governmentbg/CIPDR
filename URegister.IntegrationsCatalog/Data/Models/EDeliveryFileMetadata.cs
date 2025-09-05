using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Data.Common;

namespace URegister.IntegrationsCatalog.Data.Models
{
    /// <summary>
    /// Инфорация за качен от потребител файл
    /// </summary>
    [Comment("Инфорация за качен от потребител файл")]
    public class EDeliveryFileMetadata : EntityBaseWithLastModifiedInfo
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        [Comment("Идентификатор")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Comment("Идентификатор на съобщение")]
        public Guid EDeliveryMessageId { get; set; }

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
        /// Идентификатор на файла в хранилището
        /// </summary>
        [Comment("Идентификатор на файла в хранилището")]
        public Guid? FileId { get; set; }


        /// <summary>
        /// Идентификатор на файл от съобщение
        /// </summary>
        [Comment("Идентификатор на файл от  съобщение")]
        public int BlobId { get; set; }

        /// <summary>
        /// Референтен номер на услуга (РНУ)
        /// </summary>
        [Comment("Референтен номер на услуга (РНУ)")]
        public string? Rnu { get; set; }

        [ForeignKey(nameof(EDeliveryMessageId))]
        public EDeliveryMessage EDeliveryMessage { get; set; } = null!;
    }
}
