using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace URegister.NumberGenerator.Data.Models
{
    /// <summary>
    /// Номератор на универсалния регистър
    /// </summary>
    [Index(nameof(Prefix), IsUnique = true)]
    [Comment("Номератор на универсалния регистър")]
    public class Numerator
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        [Comment("Идентификатор")]
        public long Id { get; set; }

        /// <summary>
        /// Префикс във формат yyddd
        /// </summary>
        [Required]
        [Comment("Префикс във формат yyddd")]
        public int Prefix { get; set; }

        /// <summary>
        /// Последователност
        /// </summary>
        [Required]
        [Comment("Последователност")]
        public int Sequence { get; set; }
    }
}
