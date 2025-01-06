using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using URegister.Infrastructure.Constants;

namespace URegister.Infrastructure.Model.KeyCloak
{
    public class UserKeyCloak
    {
        [JsonProperty("id")]
        [JsonIgnore]
        public string Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }
      
        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("attributes")]
        public AttributesKeyCloak Attributes { get; set; }

        [JsonProperty("emailVerified")]
        public bool EmailVerified { get; set; }

        [JsonProperty("groups")]
        public List<string> Groups { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

    }
}
