using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace URegister.Infrastructure.Contracts
{
    public interface IHttpRequester
    {
        /// <summary>
        /// Authorization Key if needed
        /// </summary>
        string ApiKey { get; set; }


        bool IgnoreSSLErrors { get; set; }

        public string BasicAuth { get; set; }

        public string FileName { get; set; }

        public string BearerToken { get; set; }

        public Dictionary<string, string> HeaderParams { get; set; }

        public Dictionary<string, string> FormUrlEncodedParams { get; set; }

        /// <summary>
        /// Get data from Rest service
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="url">Service endpoint</param>
        /// <returns>Received data</returns>
        Task<T> GetAsync<T>(string url);

        /// <summary>
        /// Get Data from Rest service
        /// </summary>
        /// <param name="url">Service endpoint</param>
        /// <returns>Service response</returns>
        Task<HttpResponseMessage> GetAsync(string url, object data = null);

        /// <summary>
        /// Post data to Rest service
        /// </summary>
        /// <param name="url">Service endpoint</param>
        /// <param name="data">Data to send</param>
        /// <returns>Service response</returns>
        Task<HttpResponseMessage> PostAsync(string url, object data);

        /// <summary>
        /// Put request to Rest service
        /// </summary>
        /// <param name="url">Service endpoint</param>
        /// <param name="data">Data to send</param>
        /// <returns>Service response</returns>
        Task<HttpResponseMessage> PutAsync(string url, object data = null);

        /// <summary>
        /// Delete from Rest service
        /// </summary>
        /// <param name="url">Service endpoint</param>
        /// <returns>Service response</returns>
        Task<HttpResponseMessage> DeleteAsync(string url);

        Task<HttpResponseMessage> DeleteAsync(string url, object data);

        Task<HttpResponseMessage> PostFileAsync(string url, object data);
    }
}
