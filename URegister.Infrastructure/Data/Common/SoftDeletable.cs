using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using URegister.Infrastructure.Constants;

namespace URegister.Infrastructure.Data.Common
{
    public class SoftDeletable : ISoftDeletable
    {
        [Required]
        [Comment("Дали записът е активен")]
        public bool IsActive { get; set; } = true;

        [Comment("Дата на изтриване")]
        [Column(TypeName = AttributeConstants.Timestamptz)]
        public DateTime? DeletedOn { get; set; }
    }
}
