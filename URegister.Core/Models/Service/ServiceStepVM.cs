using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using URegister.Infrastructure.Constants;

namespace URegister.Core.Models.Service
{
    public class ServiceStepVM
    {
        /// <summary>
        /// Идентификатор на стъпка
        /// </summary>
        [Display(Name = "Идентификатор на стъпка")]
        public int Id { get; set; }

        /// <summary>
        /// Поредност
        /// </summary>
        [Display(Name = "Поредност")]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        public int OrderNum { get; set; }

        /// <summary>
        /// Стъпка 
        /// </summary>
        [Display(Name = "Стъпка")]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        public int StepId { get; set; }

        /// <summary>
        /// Статус 
        /// </summary>
        [Display(Name = "Статус")]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        public int StatusId { get; set; }


        [Display(Name = "Роли")]
        public List<Guid> Roles { get; set; } = new();

        /// <summary>
        /// Статус 
        /// </summary>
        [Display(Name = "Наименование")]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        public string Name { get; set; } = null!;

        public int Index { get; set; }

    }
}
