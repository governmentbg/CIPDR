using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using URegister.Infrastructure.Data.Common;

namespace URegister.RegistersCatalog.Data.Models
{
    /// <summary>
    /// Администрации
    /// </summary>
    [Comment("Администрации")]
    [Index(nameof(RegisterId), nameof(AdministrationId), IsUnique = true)]
    public class RegisterAdministration : EntityBaseWithLastModifiedInfo
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        [Comment("Идентификатор")]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Идентификатор на регистър
        /// </summary>
        [Required]
        [Comment("Идентификатор на регистър")]
        public int RegisterId { get; set; }

        /// <summary>
        /// Идентификатор на администрация
        /// </summary>
        [Comment("Идентификатор")]
        public Guid AdministrationId { get; set; } 


        /// <summary>
        /// Правно основание
        /// </summary>
        [Required]
        [MaxLength(1000)]
        [Comment("Правно основание")]
        public string LegalBasis { get; set; } = null!;

        /// <summary>
        /// Дата на създаване
        /// </summary>
        [Required]
        [Comment("Дата на създаване")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// OpenData DataSetId
        /// </summary>
        [MaxLength(100)]
        [Comment("OpenData DataSetId")]
        public string? OpenDataDataSetId { get; set; }

        /// <summary>
        /// OpenData ResourceMetaId
        /// </summary>
        [MaxLength(100)]
        [Comment("OpenData ResourceMetaId")]
        public string? ResourceMetaId { get; set; }

        /// <summary>
        /// Автоматично изпращане на данни към OpenData 1 ежедневно 2 седмично 3 месечно
        /// </summary>
        [Comment("Автоматично изпращане на данни към OpenData 1 ежедневно 2 седмично 3 месечно")]
        public int FrequencyId { get; set; }

        /// <summary>
        /// Регистър
        /// </summary>
        [ForeignKey(nameof(RegisterId))]
        public Register Register { get; set; } = null!;

        /// <summary>
        /// Администрация
        /// </summary>
        [ForeignKey(nameof(AdministrationId))]
        public Administration Administration { get; set; } = null!;
    }
}
