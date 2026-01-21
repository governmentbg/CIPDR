namespace URegister.Infrastructure.Model.RegisterForms
{
    /// <summary>
    /// Модел за изглед на форма
    /// </summary>
    public class FormViewModel : DesignerViewModel
    {
        /// <summary>
        /// Списък с данни за полета
        /// </summary>
        public List<FormField> FormFields { get; set; }

        /// <summary>
        /// Json дърво с условия към формата
        /// </summary>
        public string ConditionTree { get; set; }
    }
}
