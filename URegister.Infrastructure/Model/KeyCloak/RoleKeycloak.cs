using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URegister.Infrastructure.Model.KeyCloak
{
    public class RoleKeycloak
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("composite")]
        public bool Composite { get; set; }

        [JsonProperty("clientRole")]
        public bool ClientRole { get; set; }

        [JsonProperty("containerId")]
        public string ContainerId { get; set; }
    }
}
