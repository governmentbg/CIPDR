namespace OpenDataClient.Models
{
    internal class AddResourceMetadataResponseData
    {
        public string Uri { get; set; }
    }
    internal class AddResourceMetadataResponse : ApiResponseBase
    {
        public AddResourceMetadataResponseData Data { get; set; }
    }
}
