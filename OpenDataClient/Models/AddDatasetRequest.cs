namespace  OpenDataClient.Models
{
    internal class AddDatasetRequestData
    {
        public int OrgId { get; set; }
        public ResourceName Name { get; set; }
        public int CategoryId { get; set; }
        public int? TermsOfUseId { get; set; }
        public int? Visibility { get; set; }
    }
    internal class AddDatasetRequest : ApiRequestBase
    {
        public AddDatasetRequestData Data { get; set; }
    }
}
