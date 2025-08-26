using System.ComponentModel.DataAnnotations;
using URegister.Infrastructure.Constants;

namespace URegister.Core.Models.Register
{
    public class RegisterBaseAddressVM
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Базов адрес
        /// </summary>
        [Display(Name = "Базов адрес")]
        public string? BaseAddress { get; set; }

        /// <summary>
        /// Код на регистър
        /// </summary>
        [Display(Name = "Код на регистър")]
        public string? Code { get; set; }

        /// <summary>
        /// Име на регистър
        /// </summary>
        [Display(Name = "Име на регистър")]
        public string? Name { get; set; } 
    }
}
