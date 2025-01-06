using System.Text.Json.Serialization;
using Newtonsoft.Json;
using URegister.Infrastructure.Model.KeyCloak;

namespace URegister.Infrastructure.Models.KeyCloak
{
    public class UserResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("emailVerified")]
        public bool EmailVerified { get; set; }

        [JsonProperty("attributes")]
        public AttributesResponse Attributes { get; set; }

        [JsonProperty("createdTimestamp")]
        public long CreatedTimestamp { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("totp")]
        public bool Totp { get; set; }

        [JsonProperty("disableableCredentialTypes")]
        public List<string> DisableableCredentialTypes { get; set; } = new();

        [JsonProperty("requiredActions")]
        public List<string> RequiredActions { get; set; } = new();

        [JsonProperty("notBefore")]
        public int NotBefore { get; set; }

        [JsonProperty("access")]
        public Access Access { get; set; }
    }
}
