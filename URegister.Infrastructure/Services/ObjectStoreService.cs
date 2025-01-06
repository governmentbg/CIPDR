using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using URegister.Infrastructure.Contracts;

namespace URegister.Infrastructure.Services
{
    /// <summary>
    /// Methods for
    /// S3 compatible object store
    /// </summary>
    public class ObjectStoreService : IObjectStoreService
    {
        private readonly IConfiguration config;

        private readonly IAmazonS3 S3Client;

        private readonly ILogger logger;

        private readonly string defaultBucketName;

        /// <summary>
        /// Inversion of control
        /// </summary>
        /// <param name="_config">Configuration Api</param>
        /// <param name="_S3client">Amazon S3 client</param>
        /// <param name="_logger"></param>
        public ObjectStoreService(
            IConfiguration _config,
            IAmazonS3 _S3client,
            ILogger<ObjectStoreService> _logger)
        {
            config = _config;
            defaultBucketName = config.GetValue<string>("Objectstore:DocumentsBucket") ?? string.Empty;
            S3Client = _S3client;
            AmazonS3Config? conf = S3Client.Config as AmazonS3Config;
            logger = _logger;

            if (conf != null)
            {
                // This is needed for MinIO server
                conf.ForcePathStyle = true;
            }
        }

        /// <summary>
        /// Generate dowmload URL
        /// </summary>
        /// <param name="objectKey">Object key</param>
        /// <param name="duration">URL time to expire in minutes</param>
        /// <param name="contentType"></param>
        /// <param name="bucketName">Bucket</param>
        /// <returns>Generated URL</returns>
        public string GetPresignedUrl(string objectKey, double duration = 0.0, string contentType = "application/octet-stream", string? bucketName = null)
        {
            double configDuration = config.GetValue<double?>("Objectstore:DefaultPresignedUrlValidity") ?? 0.0;
            duration = duration != 0.0 ? duration : configDuration;
            string presignedUrl = string.Empty;

            try
            {
                GetPreSignedUrlRequest request = new GetPreSignedUrlRequest
                {
                    BucketName = bucketName ?? defaultBucketName,
                    Key = objectKey,
                    Expires = DateTime.UtcNow.AddMinutes(duration),
                    Verb = HttpVerb.GET,
                    ResponseHeaderOverrides = new ResponseHeaderOverrides() { ContentType = contentType }
                };

                presignedUrl = S3Client.GetPreSignedURL(request);
            }
            catch (AmazonS3Exception e)
            {
                logger.LogError(e, "ObjectStoreService/GetPresignedUrl");

                throw new ApplicationException("Error generating presign url", e);
            }

            return presignedUrl;
        }

        /// <summary>
        /// Save object in store
        /// </summary>
        /// <param name="obj">Object to store</param>
        /// <param name="contentType">Object Content Type</param>
        /// <param name="bucketName">Bucket</param>
        /// <returns>Object key</returns>
        public async Task<string> SaveObject(byte[] obj, string contentType = "application/octet-stream", string? bucketName = null)
        {
            string objectKey = Guid.NewGuid().ToString();

            return await SaveObject(obj, objectKey, contentType, bucketName);
        }

        /// <summary>
        /// Save object in store
        /// </summary>
        /// <param name="obj">Object to store</param>
        /// <param name="objectKey">Unique identifier of the file</param>
        /// <param name="contentType">Object Content Type</param>
        /// <param name="bucketName">Bucket</param>
        /// <returns>Object key</returns>
        public async Task<string> SaveObject(byte[] obj, string objectKey, string contentType = "application/octet-stream", string? bucketName = null)
        {
            using MemoryStream ms = new MemoryStream(obj);

            var putRequest = new PutObjectRequest
            {
                BucketName = bucketName ?? defaultBucketName,
                Key = objectKey,
                InputStream = ms,
                ContentType = contentType
            };

            try
            {
                PutObjectResponse response = await S3Client.PutObjectAsync(putRequest);
            }
            catch (AmazonS3Exception e)
            {
                logger.LogError(e, "ObjectStoreService/SaveObject");

                throw new ApplicationException("Error storing document in S3", e);
            }

            return objectKey;
        }

        /// <summary>
        /// Get object from store
        /// </summary>
        /// <param name="objectKey">Object key</param>
        /// <param name="bucketName">Bucket</param>
        /// <returns>Retreived object</returns>
        public async Task<(byte[] data, string contentType)> GetObject(string objectKey, string? bucketName = null)
        {
            byte[] data = null!;
            string contentType = null!;

            var getRequest = new GetObjectRequest
            {
                BucketName = bucketName ?? defaultBucketName,
                Key = objectKey
            };

            try
            {
                GetObjectResponse response = await S3Client.GetObjectAsync(getRequest);
                contentType = response.Headers.ContentType;

                using (MemoryStream ms = new MemoryStream())
                {
                    response.ResponseStream.CopyTo(ms);
                    data = ms.ToArray();
                }
            }
            catch (AmazonS3Exception e)
            {
                logger.LogError(e, "ObjectStoreService/GetObject");

                throw new ApplicationException("Error getting document in S3", e);
            }

            return (data, contentType);
        }

        /// <summary>
        /// Delete object from store
        /// </summary>
        /// <param name="objectKey">Object key</param>
        /// <param name="bucketName">Bucket</param>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<bool> DeleteObject(string objectKey, string? bucketName = null)
        {
            DeleteObjectRequest request = new DeleteObjectRequest()
            {
                BucketName = bucketName ?? defaultBucketName,
                Key = objectKey
            };

            bool result = false;

            try
            {
                DeleteObjectResponse response = await S3Client.DeleteObjectAsync(request);
                result = true;
            }
            catch (AmazonS3Exception e)
            {
                logger.LogError(e, "ObjectStoreService/DeleteObject");

                throw new ApplicationException("Error deleting document in S3", e);
            }

            return result;
        }
    }
}
