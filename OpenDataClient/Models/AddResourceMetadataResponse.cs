namespace OpenDataClient.Models
{
    public class AddResourceMetadataResponseData
    {
        public string Uri { get; set; }
    }
    public class AddResourceMetadataResponse : ApiResponseBase
    {
        public AddResourceMetadataResponseData Data { get; set; }
    }
}
