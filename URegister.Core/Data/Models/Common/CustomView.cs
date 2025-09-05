using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Data.Common;

namespace URegister.Core.Data.Models.Common
{
    /// <summary>
    /// Персонализирани изгледи
    /// </summary>
    [Comment("Вписвания")]
    public class CustomView : EntityBaseWithLastModifiedInfo
    {
        /// <summary>
        /// Системен идентификатор
        /// </summary>
        [Key]
        [Comment(AttributeConstants.Identifier)]
        public int Id { get; set; }

        /// <summary>
        /// Наименование
        /// </summary>
        [Comment("Наименование")]
        [StringLength(150)]
        public string Name { get; set; }

        /// <summary>
        /// Избрани полета за колони
        /// </summary>
        [Comment("Избрани полета за колони")]
        [Column(TypeName = AttributeConstants.Jsonb)]
        public List<string> SelectedColumns { get; set; } = new();
    }
}
