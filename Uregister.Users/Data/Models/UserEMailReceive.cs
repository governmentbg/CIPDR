using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Uregister.Users.Data.Identity;
using URegister.Infrastructure.Data.Common;

namespace Uregister.Users.Data.Models
{
    public class UserEMailReceive : EntityBaseWithLastModifiedInfo
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
        public bool ReceiveEFormNotification { get; set; }

        public bool ReceiveInstructionResponse { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser ApplicationUser { get; set; } = null!;
    }
}
