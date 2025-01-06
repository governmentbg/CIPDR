using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Data.Common;

namespace URegister.Core.Data.Models.Common
{
    /// <summary>
    /// Съдържа конфигурацияна на полетата във форма
    /// </summary>
    [Comment("Съдържа конфигурацияна на полетата във форма")]
    public class Form : EntityBaseWithLastModifiedInfo
    {
        /// <summary>
        /// Системен идентификатор
        /// </summary>
        [Key]
        [Comment(AttributeConstants.Identifier)]
        public int Id {  get; set; }

        /// <summary>
        /// Идентификатор на първата версия на формата
        /// </summary>        
        [Comment("Идентификатор на първата версия на формата")]
        public int? ParentId { get; set; }

        /// <summary>
        /// Заглавие на формата
        /// </summary>
        [StringLength(150)]
        [Comment("Заглавие на формата")]
        [Required]
        public string Title { get; set; }

        /// <summary>
        /// Предназначение на формата
        /// </summary>
        [StringLength(250)]
        [Comment("Предназначение на формата")]
        public string Purpose { get; set; }
        
        /// <summary>
        /// Конфигурация на полетата
        /// </summary>        
        [Comment("Конфигурация на полетата")]
        [Column(TypeName = "jsonb")]
        [Required]
        public string FieldConfiguration {  get; set; }

        /// <summary>
        /// Първата версия на формата
        /// </summary>
        [Comment("Първата версия на формата")]
        [ForeignKey(nameof(ParentId))]
        public virtual Form Parent { get; set; }
    }
}
