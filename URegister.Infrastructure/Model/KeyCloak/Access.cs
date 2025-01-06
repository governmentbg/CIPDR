using Newtonsoft.Json;

namespace URegister.Infrastructure.Model.KeyCloak
{
    public class Access
    {
        [JsonProperty("manageGroupMembership")]
        public bool ManageGroupMembership { get; set; }

        [JsonProperty("view")]
        public bool View { get; set; }

        [JsonProperty("mapRoles")]
        public bool MapRoles { get; set; }

        [JsonProperty("impersonate")]
        public bool Impersonate { get; set; }

        [JsonProperty("manage")]
        public bool Manage { get; set; }
    }
}
