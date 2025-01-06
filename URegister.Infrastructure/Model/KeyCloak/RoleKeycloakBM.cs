using Newtonsoft.Json;

namespace URegister.Infrastructure.Model.KeyCloak
{
    public class RoleKeycloakBM
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name{ get; set; }
    }
}
