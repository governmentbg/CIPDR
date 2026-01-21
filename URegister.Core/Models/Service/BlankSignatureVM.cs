using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Infrastructure.Constants;

namespace URegister.Core.Models.Service
{
    public class BlankSignatureVM
    {
        /// <summary>
        /// идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Поредност
        /// </summary>
        [Display(Name = "Поредност")]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        public int OrderNum { get; set; }

        // <summary>
        /// Подписва се от обработващия служител
        /// </summary>
        [Display(Name = "Подписва се от обработващия служител")]
        public bool SignByOperator { get; set; }

        /// <summary>
        /// Идентификатор на роля
        /// </summary>
        [Display(Name = "Идентификатор на роля")]
        public Guid? RoleId { get; set; }

        public int Index { get; set; }
    }
}
