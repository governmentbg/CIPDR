using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Uregister.Users.Data.Identity;
using URegister.Infrastructure.Data.Common;

namespace Uregister.Users.Data.Models
{
    public class UserАbsence : EntityBaseWithLastModifiedInfo
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        [Comment("Идентификатор")]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }
        public string RegisterCode { get; set; } = null!;
        public Guid AdministrationId { get; set; }
        public string? Reason { get; set; }

        /// <summary>
        /// Отсъствие от дата
        /// </summary>
        [Column(TypeName = "date")]
        [Comment("Отсъствие от дата")]
        public DateTime DateFrom { get; set; }

        /// <summary>
        /// Отсъствие до дата
        /// </summary>
        [Column(TypeName = "date")]
        [Comment("Отсъствие до дата")]
        public DateTime DateTo { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser ApplicationUser { get; set; } = null!;
    }
}
