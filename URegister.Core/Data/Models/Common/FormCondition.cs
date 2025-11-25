using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Data.Common;

namespace URegister.Core.Data.Models.Common
{
    public class FormCondition : EntityBaseWithLastModifiedInfo
    {
        /// <summary>
        /// Системен идентификатор
        /// </summary>
        [Key]
        [Comment(AttributeConstants.Identifier)]
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на първата версия на формата
        /// </summary>        
        [Comment("Идентификатор на първата версия на формата")]
        [Required]
        public int? FormParentId { get; set; }

        /// <summary>
        /// Име на полето активиращо условие
        /// </summary>
        [Required]
        [Comment("Име на полето активиращо условие")]
        [StringLength(255)]
        public string TriggeringFieldName { get; set; } = null!;

        [StringLength(20, MinimumLength = 1)]
        [Required]
        [Comment("Код на номенклатура активираща условие")]
        public string TriggeringNomenclatureValue { get; set; } = null!;

        [StringLength(1000, MinimumLength = 1)]
        [Comment("Полета за скриване")]
        public string FieldsToHide { get; set; } = null!;
    }
}
