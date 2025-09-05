using System.ComponentModel.DataAnnotations;
using URegister.Infrastructure.Constants;

namespace URegister.Core.Models.CurrentRegister
{
    public class RegisterStartVM
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Display(Name = "Регистър")]
        public int Id { get; set; }

    }
}
