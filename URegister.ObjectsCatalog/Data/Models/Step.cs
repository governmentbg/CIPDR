using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace URegister.ObjectsCatalog.Data.Models
{
    /// <summary>
    /// Стъпка
    /// </summary>
    [Comment("Стъпка")]
    public class Step
    {
        /// <summary>
        /// Идентификатор на стъпка
        /// </summary>
        [Key]
        [Comment("Идентификатор на стъпка")]
        public int Id { get; set; }

        /// <summary>
        /// Име на стъпка
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Comment("Име на стъпка")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Тип на обработчик на стъпка
        /// </summary>
        [Required]
        [MaxLength(200)]
        [Comment("Тип на обработчик на стъпка")]
        public string Type { get; set; } = null!;

        /// <summary>
        /// Метод на обработчик на стъпка
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Comment("Метод на обработчик на стъпка")]
        public string Method { get; set; } = null!;

        /// <summary>
        /// Конфигурация на стъпка
        /// </summary>
        [Column(TypeName = "jsonb")]
        [Comment("Конфигурация на стъпка")]
        public string? Configuration { get; set; }

        /// <summary>
        /// Стъпката е достъпна при публична услуга
        /// </summary>
        [Comment("Стъпката е достъпна при публична услуга")]
        public bool IsForPublicUse { get; set; } = true;

        /// <summary>
        /// Стъпката е достъпна при официална услуга
        /// </summary>
        [Comment("Стъпката е достъпна при официална услуга")]
        public bool IsForOfficialUse { get; set; } = true;

        /// <summary>
        /// Видове услуги към стъпка
        /// </summary>
        public List<ServiceTypeStep> ServiceTypeSteps { get; set; } = new List<ServiceTypeStep>();
    }
}
