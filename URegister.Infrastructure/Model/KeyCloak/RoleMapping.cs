using Newtonsoft.Json;

namespace URegister.Infrastructure.Model.KeyCloak
{
    public class RoleMapping
    {
        [JsonProperty("realmMappings")]
        public List<RoleKeycloak> RealmMappings { get; set; }
    }
}
