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
    }
}
