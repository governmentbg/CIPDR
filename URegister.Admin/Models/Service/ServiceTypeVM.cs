using System.ComponentModel.DataAnnotations;
using URegister.Core.Models.Common;
using URegister.Infrastructure.Constants;

namespace URegister.Admin.Models.Service
{
    public class ServiceTypeVM
    {
        /// <summary>
        /// Идентификатор на тип на услуга
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Име на тип на услуга
        /// </summary>
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [MaxLength(100, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [Display(Name = "Име на тип на услуга")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Услуги
        /// </summary>
        [Display(Name = "Стъпки")]
        public List<ChecklistItemViewModel> Steps { get; set; } = new();

        public bool IsInsert => Id <= 0;
    }
}
