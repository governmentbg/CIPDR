namespace OpenDataClient.Models
{
    using System.Collections.Generic;

    public class AddResourceDataRequest : ApiRequestBase
    {
        public string ResourceUri { get; set; }
        public string ExtensionFormat { get; set; }
        public IEnumerable<IEnumerable<string>> Data { get; set; }
    }
}
