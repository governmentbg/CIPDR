using System.ComponentModel.DataAnnotations;
using URegister.Infrastructure.Constants;

namespace URegister.Core.Models.Common
{
    public class PersonIdentifierVM
    {
        /// <summary>
        /// Тип на идентификатора
        /// </summary>
        [MaxLength(2, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        public string? PidType { get; set; }

        /// <summary>
        /// Идентификатор на лице
        /// </summary>
        [MaxLength(20, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [Display(Name = "Идентификатор на лице")]
        public string? Pid { get; set; } 
    }
}
