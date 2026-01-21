namespace OpenDataClient.Models
{
    public class GetUserOrganisationsResponseData
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        
        public string? Locale { get; set; }
    }
    internal class GetUserOrganisationsResponse : ApiResponseBase
    {
        public List<GetUserOrganisationsResponseData> Organisations { get; set; } = null!;
    }
}
