using OpenDataClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDataClient
{
    public interface IOpenDataClientService
    {
        Task<string> AddDatasetAsync(int orgId, string nameBG, string nameEN, int categoryId, int? termsOfUseId);
        Task<bool> AddResourceAsync(string dataSetUri, string nameBG, string nameEN, IEnumerable<IEnumerable<string>> data);
        Task<List<ListDataCategoriesResponseData>> ListDataCategoriesAsync();
        Task<List<GetUserOrganisationsResponseData>> GetUserOrganisationsAsync();
        Task<AddResourceMetadataResponse> AddResourceMetadataAsync(string datasetUri, string nameBG, string nameEN);
        Task<AddResourceDataResponse> AddResourceDataAsync(string resourceUri, IEnumerable<IEnumerable<string>> data);
        Task<AddResourceDataResponse> UpdateResourceDataAsync(string resourceUri, IEnumerable<IEnumerable<string>> data);
    }
}
