using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace URegister.Admin.Models.Service
{
    public class StepVM
    {
        /// <summary>
        /// Идентификатор на стъпка
        /// </summary>
        [Display(Name = "Идентификатор на стъпка")]
        public int Id { get; set; }

        /// <summary>
        /// Име на стъпка
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Display(Name = "Име на стъпка")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Тип на обработчик на стъпка
        /// </summary>
        [Required]
        [MaxLength(200)]
        [Display(Name = "Тип на обработчик на стъпка")]
        public string Type { get; set; } = null!;

        /// <summary>
        /// Метод на обработчик на стъпка
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Display(Name = "Метод на обработчик на стъпка")]
        public string Method { get; set; } = null!;

        /// <summary>
        /// Стъпката е достъпна при публична услуга
        /// </summary>
        [Display(Name = "Стъпката е достъпна при публична услуга")]
        public bool IsForPublicUse { get; set; } = true;

        /// <summary>
        /// Стъпката е достъпна при официална услуга
        /// </summary>
        [Display(Name = "Стъпката е достъпна при официална услуга")]
        public bool IsForOfficialUse { get; set; } = true;

        public bool IsInsert => Id <= 0;

    }
}
