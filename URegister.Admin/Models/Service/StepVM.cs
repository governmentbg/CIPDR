using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using URegister.Infrastructure.Constants;

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
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [MaxLength(100, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [Display(Name = "Име на стъпка")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Потребителска роля
        /// </summary>
        [Comment("Потребителска роля")]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [Display(Name = "Потребителска роля")]
        public Guid? RoleId { get; set; }

        /// <summary>
        /// Тип на обработчик на стъпка
        /// </summary>
        //[Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [MaxLength(200, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [Display(Name = "Тип на обработчик на стъпка")]
        public string? Type { get; set; }

        /// <summary>
        /// Метод на обработчик на стъпка
        /// </summary>
        //[Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [MaxLength(100, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [Display(Name = "Метод на обработчик на стъпка")]
        public string? Method { get; set; } 

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
