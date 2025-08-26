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

        /// <summary>
        /// Идентификатор на форма
        /// </summary>
        public int FormId { get; set; }

        /// <summary>
        /// Чака ли одобрение конфигурацията ли е конмфигурацията
        /// </summary>
        public bool IsWaitingApproval { get; set; }

        /// <summary>
        /// Дали формата се ползва за тест на дизайн или за запис на данни
        /// </summary>
        public bool DontUploadFilesToStorage { get; set; } = false;

        /// <summary>
        /// Минути отстъп на потребителстата времева зона от UTC
        /// </summary>
        public int UserTimeZoneOffsetInMinutes { get; set; } = 0;
        
        /// <summary>
        /// Формата има ли нужда от заявител
        /// </summary>
        public bool IsSubmitterRequired { get; set; } = true;
    }
}
