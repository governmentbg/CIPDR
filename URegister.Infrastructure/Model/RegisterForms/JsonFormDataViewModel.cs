using System.ComponentModel;

namespace URegister.Infrastructure.Model.RegisterForms
{
    /// <summary>
    /// Модел за подаване на данни като JSON файл
    /// </summary>
    public class JsonFormDataViewModel : DesignerViewModel
    {
        /// <summary>
        /// JSON файл със стойности на полета
        /// </summary>
        [DisplayName("Файл с данните")]
        public string JsonData { get; set; } = string.Empty;
    }
}
