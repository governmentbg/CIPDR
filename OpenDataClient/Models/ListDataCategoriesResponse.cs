namespace OpenDataClient.Models
{
    public class ListDataCategoriesResponseData
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Locale { get; set; }
        public int DatasetsCount { get; set; }
    }
    internal class ListDataCategoriesResponse : ApiResponseBase
    {
        public List<ListDataCategoriesResponseData> Categories { get; set; } = null!;
    }
}
