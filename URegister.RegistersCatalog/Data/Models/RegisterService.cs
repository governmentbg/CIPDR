using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Data.Common;

namespace URegister.RegistersCatalog.Data.Models
{
    /// <summary>
    /// Услуга в регистъра
    /// </summary>
    [Comment("Услуга в регистъра")]
    [PrimaryKey(nameof(RegisterId), nameof(ServiceId))]
    public class RegisterService : EntityBaseWithLastModifiedInfo
    {
        /// <summary>
        /// Идентификатор на регистър
        /// </summary>
        [Comment("Идентификатор на регистър")]
        public int RegisterId { get; set; }

        /// <summary>
        /// Идентификатор на услуга
        /// </summary>
        [Comment("Идентификатор на услуга")]
        public int ServiceId { get; set; }

        /// <summary>
        /// Идентификатор на тип услуга
        /// </summary>
        [Comment("Идентификатор на услуга")]
        public int ServiceTypeId { get; set; }

        /// <summary>
        /// Референтен номер на услуга (РНУ)
        /// </summary>
        [Comment("Референтен номер на услуга (РНУ)")]
        public string? EFormCode { get; set; }


        /// <summary>
        /// Регистър
        /// </summary>
        [ForeignKey(nameof(RegisterId))]
        public Register Register { get; set; } = null!;
    }
}
