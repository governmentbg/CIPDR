namespace OpenDataClient
{
    using global::OpenDataClient.Models;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    
    /// <summary>
    /// Connector for Open Data API
    /// </summary>
    public class OpenDataClientService(
        IHttpClientFactory clientFactory,
        IOptions<OpenDataOptions> options) : IOpenDataClientService
    {
        
        /// <summary>
        /// Will add Resource to a given data set
        /// TODO: change this when we have propper values for the resource description fields
        /// </summary>
        /// <param name="dataSetHandler">Handler for the data set</param>
        /// <param name="nameBG">Name in Bulgarian</param>
        /// <param name="nameEN">Name in English</param>
        /// <param name="data">Data to be uploaded</param>
        /// <returns>If it's successful</returns>
        public async Task<bool> AddResourceAsync(string dataSetUri, string nameBG, string nameEN, IEnumerable<IEnumerable<string>> data)
        {
            var resourceMetadataResponse = await AddResourceMetadataInternalAsync(dataSetUri, nameBG, nameEN);
            var resourceDataResponse = await AddResourceDataAsync(resourceMetadataResponse.Data.Uri, data);
            return resourceDataResponse.Success;
        }

        public async Task<string> AddDatasetAsync(int orgId, string nameBG, string nameEN, int categoryId, int? termsOfUseId)
        {
            var requestData = new AddDatasetRequest()
            {
                ApiKey = options.Value.ApiKey,
                Data = new AddDatasetRequestData()
                {
                    OrgId = orgId,
                    Name = new ResourceName()
                    {
                        BG = nameBG,
                        EN = nameEN,
                    },
                    CategoryId = categoryId,
                    TermsOfUseId = termsOfUseId,
                },
            };
            var endpoint = "addDataset";
            var responseData = await PostAsync<AddDatasetResponse, AddDatasetRequest>(endpoint, requestData);
            return responseData.Uri;
        }

        private async Task<AddResourceMetadataResponse> AddResourceMetadataInternalAsync(string datasetUri, string nameBG, string nameEN)
        {
            var requestData = new AddResourceMetadataRequest()
            {
                ApiKey = options.Value.ApiKey,
                DatasetUri = datasetUri,
                Data = new AddResourceMetadataRequestData()
                {
                    Name = new ResourceName()
                    {
                        EN = nameEN,
                        BG = nameBG,
                    },
                    FileFormat = "csv",
                    Type = 1,
                },
            };
            var endpoint = "addResourceMetadata";
            var responseData = await PostAsync<AddResourceMetadataResponse, AddResourceMetadataRequest>(endpoint, requestData);
            return responseData;
        }

        private async Task<AddResourceDataResponse> AddResourceDataAsync(string resourceUri, IEnumerable<IEnumerable<string>> data)
        {
            var requestData = new AddResourceDataRequest()
            {
                ApiKey = options.Value.ApiKey,
                ResourceUri = resourceUri,
                ExtensionFormat = "csv",
                Data = data,
            };
            var endpoint = "addResourceData";
            var responseData = await PostAsync<AddResourceDataResponse, AddResourceDataRequest>(endpoint, requestData);
            return responseData;
        }

        private async Task<TResponse> PostAsync<TResponse, TRequest>(string requestUri, TRequest requestData)
        {
            var httpClient = clientFactory.CreateClient("openDataClient");
            var uri = new Uri(new Uri(options.Value.BaseAddr), requestUri);
            var data = SerializeRequest(requestData);
            var response = await httpClient.PostAsync(uri, data);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                var responseString = await response.Content.ReadAsStringAsync();

                // this is done because part of the response is escaped UTF-16 sequence...
                //responseString = JToken
                //    .Parse(responseString)
                //    .ToString();
                throw new Exception("There could be difference between the API and client implementation."  + responseString);
            }

            return await ParseResponseAsync<TResponse>(response);
        }

        private HttpContent SerializeRequest<T>(T data)
        {
            var json = JsonSerializer.Serialize(data, DefaultSerilizeOptions());
            var stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
            return stringContent;
        }

        private static JsonSerializerOptions DefaultSerilizeOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            };
        }

        private async Task<T> ParseResponseAsync<T>(HttpResponseMessage response)
        {
            var jsonString = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(jsonString, DefaultSerilizeOptions());
        }
    }
}
