namespace OpenDataClient.Models
{
    internal class AddResourceMetadataRequestData
    {
        public ResourceName Name { get; set; }
        public string FileFormat { get; set; }
        public int Type { get; set; }
    }
    internal class AddResourceMetadataRequest : ApiRequestBase
    {
        public string DatasetUri { get; set; }
        public AddResourceMetadataRequestData Data { get; set; }
    }
}
