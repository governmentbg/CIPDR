using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using URegister.Infrastructure.Data.Common;

namespace URegister.RegistersCatalog.Data.Models
{
    /// <summary>
    /// Регистри
    /// </summary>
    [Comment("Регистри")]
    [Index(nameof(Code), IsUnique = true)]
    public class Register : EntityBaseWithLastModifiedInfo
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        [Comment("Идентификатор")]
        public int Id { get; set; }

        /// <summary>
        /// Код на регистър
        /// </summary>
        [Required]
        [MaxLength(10)]
        [Comment("Код на регистър")]
        public string Code { get; set; } = null!;

        /// <summary>
        /// Име на регистър
        /// </summary>
        [Required]
        [MaxLength(500)]
        [Comment("Име на регистър")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Име на регистър на английски език
        /// </summary>
        [MaxLength(500)]
        [Comment("Име на регистър на английски език")]
        public string NameEn { get; set; } = null;

        /// <summary>
        /// Съкратено име на регистър ползва се при пращане на съобщение към ССЕВ
        /// </summary>
        [MaxLength(200)]
        [Comment("Съкратено име на регистър ползва се при пращане на съобщение към ССЕВ")]
        public string? NameEDelivery { get; set; } 

        /// <summary>
        /// Описание
        /// </summary>
        [MaxLength(1000)]
        [Comment("Описание")]
        public string? Description { get; set; }

        /// <summary>
        /// Правно основание
        /// </summary>
        [Required]
        [MaxLength(1000)]
        [Comment("Правно основание")]
        public string LegalBasis { get; set; } = null!;

        /// <summary>
        /// Вид на регистъра
        /// </summary>
        [Required]
        [MaxLength(5)]
        [Comment("Вид на регистъра")]
        public string Type { get; set; } = null!;

        /// <summary>
        /// Ниво на осигуреност на средствата за електронна идентификация
        /// </summary>
        //[Required]
        [MaxLength(5)]
        [Comment("Ниво на осигуреност на средствата за електронна идентификация")]
        public string? IdentitySecurityLevel { get; set; } 


        /// <summary>
        /// Начин на вписване
        /// </summary>
        [Required]
        [MaxLength(5)]
        [Comment("Начин на вписване")]
        public string TypeEntry { get; set; } = null!;

        /// <summary>
        /// Дата на създаване
        /// </summary>
        [Required]
        [Comment("Дата на създаване")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;


        /// <summary>
        /// Дата на стартиране
        /// </summary>
        [Comment("Дата на стартиране")]
        public DateTime? StartedOn { get; set; }

        /// <summary>
        /// Базов адрес
        /// </summary>
        [Comment("Базов адрес")]
        public string? BaseAddress { get; set; }

        /// <summary>
        /// Идентификатор на статус
        /// </summary>
        [Comment("Идентификатор на статус")]
        public int StatusId { get; set; }

        /// <summary>
        /// Стартиран ли е контейнер за регистъра
        /// </summary>
        [Comment("Стартиран ли е контейнер за регистъра")]
        public bool Deployed { get; set; }

        /// <summary>
        /// Категория opendata
        /// </summary>
        [Comment("Категория opendata")]
        public int OpenDataCategoryId { get; set; }

        /// <summary>
        /// Тагове opendata
        /// </summary>
        [Comment("Тагове opendata")]
        [MaxLength(500)]
        public string? OpenDataTags { get; set; }

        /// <summary>
        /// AppId за Stampit
        /// </summary>
        [Comment("AppId за Stampit")]
        [MaxLength(100)]
        public string? AppId { get; set; }

        /// <summary>
        /// AppSecret за Stampit
        /// </summary>
        [Comment("AppSecret за Stampit")]
        [MaxLength(100)]
        public string? AppSecret { get; set; }

        /// <summary>
        /// Дата на старт deploy
        /// </summary>
        [Comment("Дата на старт на deploy")]
        public DateTime? DateDeploy { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Администрации
        /// </summary>
        public List<RegisterAdministration> RegisterAdministrations { get; set; } = new ();

        /// <summary>
        /// Историята не е публична
        /// </summary>
        [Comment("Историята не е публична")]
        public bool? HistoryNotPublic { get; set; } = false;
    }
}
