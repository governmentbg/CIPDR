using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using URegister.Infrastructure.Constants;

namespace URegister.Infrastructure.Model.RegisterForms
{
    /// <summary>
    /// Модел за условие към форма
    /// </summary>
    public class AddConditionViewModel
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор на родител на форма
        /// </summary>
        public int FormParentId { get; set; }

        /// <summary>
        /// Поле източник на събитие
        /// </summary>
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [DisplayName("При промяна в поле:")]
        [StringLength(250, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        public string? TriggeringFieldName { get; set; }

        /// <summary>
        /// Стойност източник на събитие
        /// </summary>
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [DisplayName("И избрана стойност:")]
        [StringLength(20, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        public string? TriggeringNomenclatureValue { get; set; }

        /// <summary>
        /// Полета за скриване
        /// </summary>
        [DisplayName("Скрий следните полета:")]
        public List<string> FieldsToHide { get; set; }
    }
}
