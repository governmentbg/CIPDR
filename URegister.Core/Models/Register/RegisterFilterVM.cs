using System.ComponentModel.DataAnnotations;

namespace URegister.Core.Models.Register
{
    /// <summary>
    /// Филтър за регистри
    /// </summary>
    public class RegisterFilterVM
    {
        /// <summary>
        /// Код
        /// </summary>
        [Display(Name = "Код")]
        public string? Code { get; set; }

        /// <summary>
        /// Име
        /// </summary>
        [Display(Name = "Име")]
        public string? Name { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        [Display(Name = "Описание")]
        public string? Description { get; set; }
    }
}
