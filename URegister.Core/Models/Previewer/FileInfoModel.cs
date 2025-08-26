namespace URegister.Core.Models.Previewer
{
    /// <summary>
    /// Информация за файл
    /// </summary>
    public class FileInfoModel
    {
        /// <summary>
        /// Идентификатор на файл
        /// </summary>
        public string Id { get; set; } = null!;

        /// <summary>
        /// Име на файл
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Тип на съдържанието
        /// </summary>
        public string ContentType { get; set; } = null!;

        /// <summary>
        /// Размер на файл
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Файлът е подписан
        /// </summary>
        public bool IsSigned { get; set; }

        /// <summary>
        /// Подпис на файл, ако файлът е подписан разкачено
        /// </summary>
        public string? Signature { get; set; } = null;
    }
}
