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
     public class RegisterStatusListItemVM 
     {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Id { get; set; } 

        /// <summary>
        /// Идентификатор на статус
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Забележка
        /// </summary>
        public string? Remark { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }

    }
}
