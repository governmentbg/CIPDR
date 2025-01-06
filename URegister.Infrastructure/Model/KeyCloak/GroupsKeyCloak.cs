using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace URegister.Infrastructure.Model.KeyCloak
{
    public class GroupsKeyCloak
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("subGroups")]
        public string[] SubGroups { get; set; }
    }
}
