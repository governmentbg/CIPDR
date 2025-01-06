using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace URegister.Infrastructure.Model.KeyCloak
{
    public class GroupResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("subGroupCount")]
        public int SubGroupCount { get; set; }

        [JsonProperty("subGroups")]
        public List<object> SubGroups { get; set; }

        [JsonProperty("access")]
        public Access Access { get; set; }
    }
}
