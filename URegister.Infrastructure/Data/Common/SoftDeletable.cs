using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace URegister.Infrastructure.Data.Common
{
    public class SoftDeletable : ISoftDeletable
    {
        [Required]
        [Comment("Дали записът е активен")]
        public bool IsActive { get; set; } = true;

        [Comment("Дата на изтриване")]
        public DateTime? DeletedOn { get; set; }
    }
}
