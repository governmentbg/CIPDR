using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Data.Common;

namespace URegister.RegistersCatalog.Data.Models
{
    /// <summary>
    /// Статуси на регистър
    /// </summary>
    [Comment("Статуси на регистър")]
    public class RegisterStatus : EntityBaseWithLastModifiedInfo
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
        /// Идентификатор на статус
        /// </summary>
        [Comment("Идентификатор на статус")]
        public int StatusId { get; set; }

        /// <summary>
        /// Забележка
        /// </summary>
        [MaxLength(1000)]
        [Comment("Забележка")]
        public string? Remark { get; set; }

        /// <summary>
        /// Регистър
        /// </summary>
        [ForeignKey(nameof(RegisterId))]
        public Register Register { get; set; } = null!;

    }
}
