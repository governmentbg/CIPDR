using Microsoft.EntityFrameworkCore;

namespace URegister.RegistersCatalog.Data.Models
{
    /// <summary>
    /// Записи на лица в регистър
    /// </summary>
    [Comment("Записи на лица в регистър")]
    [PrimaryKey(nameof(RegisterId), nameof(MasterPersonRecordId))]
    public class RegisterPersonRecord
    {
        /// <summary>
        /// Идентификатор на регистър
        /// </summary>
        [Comment("Идентификатор на регистър")]
        public int RegisterId { get; set; }

        /// <summary>
        /// Регистър
        /// </summary>
        [Comment("Регистър")]
        public Register Register { get; set; } = null!;

        /// <summary>
        /// Идентификатор на партида
        /// </summary>
        [Comment("Идентификатор на партида")]
        public Guid MasterPersonRecordId { get; set; }

        /// <summary>
        /// Партида
        /// </summary>
        [Comment("Партида")]
        public MasterPersonRecordsIndex MasterPersonRecord { get; set; } = null!;
    }
}
