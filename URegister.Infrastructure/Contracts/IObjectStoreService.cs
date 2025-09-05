namespace URegister.Infrastructure.Contracts
{
    /// <summary>
    /// Methods for
    /// S3 compatible object store
    /// </summary>
    public interface IObjectStoreService
    {
        /// <summary>
        /// Запазване на файл в хранилището
        /// </summary>
        /// <param name="filename">Име на качения файл</param>
        /// <param name="obj">Файлът като масив от байтове</param>
        /// <param name="contentType">Тип на съдържанието на файла</param>
        /// <param name="bucketName">име на bucket</param>
        /// <returns>Object key</returns>
        Task<string> SaveObject(string filename, 
            byte[] obj, 
            string contentType = "application/octet-stream", 
            string? bucketName = null);

        /// <summary>
        /// Save object in store
        /// </summary>
        /// <param name="filename">Име на качения файл</param>
        /// <param name="obj">Файлът като масив от байтове</param>
        /// <param name="objectKey">Ключ на файла</param>
        /// <param name="contentType">Тип на съдържанието на файла</param>
        /// <param name="bucketName">име на bucket</param>
        /// <returns>Object key</returns>
        Task<string> SaveObject(
            string filename,
            byte[] obj, 
            string objectKey, 
            string contentType = "application/octet-stream", 
            string? bucketName = null);

        /// <summary>
        /// Създава URL връзка към качения файл
        /// </summary>
        /// <param name="objectKey">Ключ на файла</param>
        /// <param name="duration">Минути на валидност на връзката към файла</param>
        /// <param name="contentType">Тип на съдържанието на файла</param>
        /// <param name="bucketName">име на bucket</param>
        /// <returns>Generated URL</returns>
        Task<string> GetPresignedUrl(string objectKey, double duration = 0.0, string contentType = "application/octet-stream", string? bucketName = null);

        /// <summary>
        /// Връща качения файл
        /// </summary>
        /// <param name="objectKey">Ключ на файла</param>
        /// <param name="bucketName">Bucket</param>
        /// <returns>Каченият файл</returns>
        Task<(byte[] data, string contentType)> GetObject(string objectKey, string? bucketName = null);

        /// <summary>
        /// Изтрива файл от хранилището
        /// </summary>
        /// <param name="objectKey">Ключ на файла</param>
        /// <param name="bucketName">Bucket</param>
        /// <returns>Дали изтриването е успешно</returns>
        Task<bool> DeleteObject(string objectKey, string? bucketName = null);
    }
}
