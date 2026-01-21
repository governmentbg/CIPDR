using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using URegister.Infrastructure.Data.Common;

namespace URegister.Core.Data.Models.Common
{
    /// <summary>
    /// Формула за изчисление стойност на поле
    /// </summary>
    public class FieldFormula : EntityBaseWithLastModifiedInfo
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        [Comment("Идентификатор")]
        public int Id { get; set; }

        /// <summary>
        /// Поредност
        /// </summary>
        [Comment("Поредност")]
        public int Priority { get; set; }

        /// <summary>
        /// Идентификатор
        /// </summary>
        [Required]
        [Comment("Идентификатор")]
        [MaxLength(255)]
        public string TargetField { get; set; }

        /// <summary>
        /// Формула
        /// </summary>
        [Required]
        [Comment("Формула")]
        [MaxLength(512)]
        public string Formula { get; set; }

        /// <summary>
        /// Идентификатор на първата версия на формата
        /// </summary>        
        [Comment("Идентификатор на първата версия на формата")]
        [Required]
        public int? FormParentId { get; set; }
    }
}
