namespace OpenDataClient.Models
{
    using System.Collections.Generic;

    internal class AddResourceDataRequest : ApiRequestBase
    {
        public string ResourceUri { get; set; }
        public string ExtensionFormat { get; set; }
        public IEnumerable<IEnumerable<string>> Data { get; set; }
    }
}
