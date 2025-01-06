using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using URegister.Infrastructure.Data.Common;

namespace URegister.RegistersCatalog.Data.Models
{
    /// <summary>
    /// Глобална партида на лице
    /// </summary>
    [Comment("Глобална партида на лице")]
    [Index(nameof(Pid), nameof(PidType), IsUnique = true)]
    public class MasterPersonRecordsIndex : SoftDeletable
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        [Comment("Идентификатор")]
        public Guid Id { get; set; }

        /// <summary>
        /// Идентификатор на основно лице
        /// </summary>
        [Comment("Идентификатор на основно лице")]
        public Guid? MasterPersonId { get; set; }

        /// <summary>
        /// Идентификатор на лице
        /// </summary>
        [MaxLength(20)]
        [Required]
        [Comment("Идентификатор на лице")]
        public string Pid { get; set; } = null!;

        /// <summary>
        /// Тип на идентификатора
        /// </summary>
        [MaxLength(2)]
        [Required]
        [Comment("Тип на идентификатора")]
        public string PidType { get; set; } = null!;

        /// <summary>
        /// Име
        /// </summary>
        [Required]
        [Comment("Име")]
        [MaxLength(300)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Дата на създаване
        /// </summary>
        [Required]
        [Comment("Дата на създаване")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Регистри
        /// </summary>
        public List<RegisterPersonRecord> RegisterPersonRecords { get; set; } = new();
    }
}
