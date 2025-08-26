using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Infrastructure.Constants;

namespace URegister.Core.Models.Deadline
{
    public class DeadlineVM
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на услуга
        /// </summary>
        [Display(Name = "Услуга")]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        public int ServiceId { get; set; }

        /// <summary>
        /// Име на услуга
        /// </summary>
        public string? ServiceName { get; set; }

        /// <summary>
        /// Вид срок за изпълнение на услуга
        /// </summary>
        [Display(Name = "Вид срок за изпълнение на услуга")]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        public string DeadlineTypeId { get; set; } = null!;

        /// <summary>
        /// Вид срок за изпълнение на услуга
        /// </summary>
        public string? DeadlineType { get; set; }

        /// <summary>
        /// Работни/календарни дни
        /// </summary>
        [Display(Name = "Работни/календарни дни")]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        public string DayTypeId { get; set; } = null!;

        /// <summary>
        /// Работни/календарни дни
        /// </summary>
        public string? DayType { get; set; }


        /// <summary>
        /// Дни за изпълнение на услуга
        /// </summary>
        [Display(Name ="Дни за изпълнение на услуга")]
        public int Days { get; set; }

    }
}
