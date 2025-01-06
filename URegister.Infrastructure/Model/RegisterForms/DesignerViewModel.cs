namespace URegister.Infrastructure.Model.RegisterForms
{
    /// <summary>
    /// Модел за дизайнера на форми
    /// </summary>
    public class DesignerViewModel
    {
        /// <summary>
        /// Родителски идентификатор на формата
        /// </summary>
        public int FormParentId { get; set; }

        /// <summary>
        /// Заглавието на формата
        /// </summary>
        public string FormTitle { get; set; }

        /// <summary>
        /// Предназначение на формата
        /// </summary>
        public string Purpose { get; set; }

        /// <summary>
        /// Тип поле избрано при зареждане на страницата
        /// </summary>
        public string SelectedType { get; set; } = string.Empty;
    }
}
