using Newtonsoft.Json;

namespace URegister.Infrastructure.Model.KeyCloak
{
    public class AttributesKeyCloak
    {
        [JsonProperty("locale")]
        public string Locale { get; set; }
        
        [JsonProperty("administrationID")]
        public string AdministrationId { get; set; }
    }
}