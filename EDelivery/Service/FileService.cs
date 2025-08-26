using EDelivery.Constants;
using EDelivery.Contracts;
using EDelivery.Integration.Contracts;
using EDelivery.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EDelivery.Service
{
    public class FileService(
        IHttpClientFactory clientFactory,
        ITokenService tokenService,
        ILogger<FileService> logger) : IFileService
    {
        public async Task<BlobDO?> UploadFile(string fileName, byte[] fileData)
        {
            var token = await tokenService.GetToken();
            var httpClient = clientFactory.CreateClient("uploadClient");
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var form = new MultipartFormDataContent();
            var fileContent = new StreamContent(new MemoryStream(fileData));
            form.Add(fileContent, "file", fileName);
            var result = await httpClient.PostAsync($"ed2/upload/blobs?type=Storage", form);
            var contentResponse = await result.Content.ReadAsStringAsync().ConfigureAwait(false);

            return JsonSerializer.Deserialize<BlobDO>(contentResponse);
        }

        public async Task<byte[]> DownLoadFile(string url)
        {
            var httpClient = clientFactory.CreateClient("downLoadClient");
            // Send a GET request to the specified Uri
            using (var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode(); // Throw if not a success code.
                return await response.Content.ReadAsByteArrayAsync();
            }
        }
    }
}
