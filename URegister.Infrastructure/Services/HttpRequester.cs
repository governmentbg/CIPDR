using URegister.Infrastructure.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace URegister.Infrastructure.Services
{
    public class HttpRequester : IHttpRequester
    {
        public string ApiKey { get; set; }

        public string BasicAuth { get; set; }

        public string FileName { get; set; }


        private readonly IHttpClientFactory clientFactory;

        public string BearerToken { get; set; }

        public Dictionary<string, string> HeaderParams { get; set; }

        public Dictionary<string, string> FormUrlEncodedParams { get; set; }

        public HttpRequester(IHttpClientFactory _clientFactory)
        {
            clientFactory = _clientFactory;
        }

        public async Task<HttpResponseMessage> DeleteAsync(string? httpClientName, string url)
        {
            return await Request(httpClientName, url, HttpMethod.Delete);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string? httpClientName, string url, object data)
        {
            return await Request(httpClientName, url, HttpMethod.Delete, data);
        }

        public async Task<T> GetAsync<T>(string? httpClientName, string url)
        {
            var response = await Request(httpClientName, url, HttpMethod.Get);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(content);
            };

            throw new InvalidOperationException("Unexpected error!",
                new ApplicationException(response.ReasonPhrase));
        }

        public async Task<byte[]> GetFileAsync(string? httpClientName, string url)
        {
            var response = await Request(httpClientName, url, HttpMethod.Get);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            };
            throw new InvalidOperationException("Unexpected error!",
                new ApplicationException(response.ReasonPhrase));
        }
        public async Task<HttpResponseMessage> GetAsync(string? httpClientName, string url, object data = null)
        {
            return await Request(httpClientName, url, HttpMethod.Get, data);
        }

        public async Task<HttpResponseMessage> PostAsync(string? httpClientName, string url, object data)
        {
            return await Request(httpClientName, url, HttpMethod.Post, data);
        }

        public async Task<HttpResponseMessage> PutAsync(string? httpClientName, string url, object data = null)
        {
            return await Request(httpClientName, url, HttpMethod.Put, data);
        }

        public async Task<HttpResponseMessage> PostFileAsync(string? httpClientName, string url, object data)
        {
            return await RequestFile(httpClientName, url, HttpMethod.Post, data);
        }

        private async Task<HttpResponseMessage> Request(string? httpClientName, string url, HttpMethod method, object data = null)
        {
            var request = new HttpRequestMessage(method, url);
            request.Version = new Version(2, 0);
            HttpClient client;

            if (string.IsNullOrEmpty(httpClientName))
            {
                client = clientFactory.CreateClient();
            }
            else
            {
                client = clientFactory.CreateClient(httpClientName);
            }

            if (data != null)
            {
                var jsonData = JsonConvert.SerializeObject(data);
                request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            }

            if (this.ApiKey != null)
            {
                client.DefaultRequestHeaders.Add("X-apiKey", this.ApiKey);
            }

            if (string.IsNullOrEmpty(this.BearerToken) == false)
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", this.BearerToken);
            }

            if (string.IsNullOrEmpty(this.BasicAuth) == false)
            {
                var byteArray = Encoding.ASCII.GetBytes(this.BasicAuth);
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (this.HeaderParams != null)
            {
                foreach (var item in this.HeaderParams)
                {
                    client.DefaultRequestHeaders.Add(item.Key, item.Value);
                }
            }

            if (FormUrlEncodedParams != null)
            {
                request.Content = new FormUrlEncodedContent(FormUrlEncodedParams);
            }

            return await client.SendAsync(request);
        }

        private async Task<HttpResponseMessage> RequestFile(string? httpClientName, string url, HttpMethod method, object data = null)
        {
            var request = new HttpRequestMessage(method, url);
            request.Version = new Version(2, 0);
            HttpClient client;

            if (string.IsNullOrEmpty(httpClientName))
            {
                client = clientFactory.CreateClient();
            }
            else
            {
                client = clientFactory.CreateClient(httpClientName);
            }

            if (data != null)
            {
                var jsonData = JsonConvert.SerializeObject(data);

                var form = new MultipartFormDataContent();
                var fileContent = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(jsonData)));
                //var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(jsonData));
                //fileContent.Headers.Add("Content-Disposition", "form-data; name=\"file\"; filename=\"" + this.FileName + "\"");
                form.Add(fileContent, "file", this.FileName);
                request.Content = form;
            }

            if (this.ApiKey != null)
            {
                client.DefaultRequestHeaders.Add("X-apiKey", this.ApiKey);
            }

            if (string.IsNullOrEmpty(this.BasicAuth) == false)
            {
                var byteArray = Encoding.ASCII.GetBytes(this.BasicAuth);
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }

            if (string.IsNullOrEmpty(this.BearerToken) == false)
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", this.BearerToken);
            }

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

            return await client.SendAsync(request);
        }       
    }
}
