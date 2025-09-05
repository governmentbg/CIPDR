using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Data.Common;

namespace URegister.Core.Data.Models.Common
{
    /// <summary>
    /// Информация за връчване откази/удостоверения/указания
    /// </summary>
    [Comment("Информация за връчяане откази/удостоверения/указания")]
    public class ProcessDelivery : EntityBaseWithLastModifiedInfo
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        [Comment("Идентификатор")]
        public Guid Id { get; set; } = Guid.NewGuid();


        /// <summary>
        /// Идентификатор на източник
        /// </summary>
        [Comment("Идентификатор на  източник")]
        public int SourceTypeId { get; set; } = 0;

        /// <summary>
        /// Идентификатор на роля на файла
        /// </summary>
        [Comment("Идентификатор на източник")]
        public string? SourceId { get; set; }

        /// <summary>
        /// Идентификатор на заявена услуга
        /// </summary>
        [Comment("Идентификатор на заявена услуга")]
        public Guid ProcessId { get; set; }

        /// <summary>
        /// Начин на връчване
        /// </summary>
        [Comment("Начин на връчване")]
        public string? ChannelId { get; set; }

        /// <summary>
        /// описание
        /// </summary>
        [Column("Oписание")]
        public string? Description { get; set; }

        /// <summary>
        /// Дата на връчване
        /// </summary>
        [Comment("Дата на връчване")]
        [Column(TypeName = AttributeConstants.Timestamptz)]
        public DateTime? DeliveryDate { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        [Comment("Статус")]
        public int StatusId { get; set; }

        /// <summary>
        /// Заявена услуга
        /// </summary>
        [ForeignKey(nameof(ProcessId))]
        public virtual Process.Process Process { get; set; } = null!;
    }
}