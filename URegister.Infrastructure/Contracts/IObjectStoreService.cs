namespace URegister.Infrastructure.Contracts
{
    /// <summary>
    /// Methods for
    /// S3 compatible object store
    /// </summary>
    public interface IObjectStoreService
    {
        /// <summary>
        /// Save object in store
        /// </summary>
        /// <param name="obj">Object to store</param>
        /// <param name="contentType">Object Content Type</param>
        /// <param name="bucketName">Bucket</param>
        /// <returns>Object key</returns>
        Task<string> SaveObject(byte[] obj, string contentType = "application/octet-stream", string? bucketName = null);

        /// <summary>
        /// Save object in store
        /// </summary>
        /// <param name="obj">Object to store</param>
        /// <param name="objectKey">Unique identifier of the file</param>
        /// <param name="contentType">Object Content Type</param>
        /// <param name="bucketName">Bucket</param>
        /// <returns>Object key</returns>
        Task<string> SaveObject(byte[] obj, string objectKey, string contentType = "application/octet-stream", string? bucketName = null);

        /// <summary>
        /// Generate dowmload URL
        /// </summary>
        /// <param name="objectKey">Object key</param>
        /// <param name="duration">URL time to expire in minutes</param>
        /// <param name="contentType">Content type of the object</param>
        /// <param name="bucketName">Bucket</param>
        /// <returns>Generated URL</returns>
        string GetPresignedUrl(string objectKey, double duration = 0.0, string contentType = "application/octet-stream", string? bucketName = null);

        /// <summary>
        /// Get object from store
        /// </summary>
        /// <param name="objectKey">Object key</param>
        /// <param name="bucketName">Bucket</param>
        /// <returns>Retreived object</returns>
        Task<(byte[] data, string contentType)> GetObject(string objectKey, string? bucketName = null);

        /// <summary>
        /// Delete object from store
        /// </summary>
        /// <param name="objectKey">Object key</param>
        /// <param name="bucketName">Bucket</param>
        /// <returns>Successfuly deleted</returns>
        Task<bool> DeleteObject(string objectKey, string? bucketName = null);
    }
}
