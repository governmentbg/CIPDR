using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace URegister.Infrastructure.Model.KeyCloak
{
    public class AttributesResponse
    {
        [JsonProperty("administrationID")] 
        public List<string> AdministrationID { get; set; } = new();
    }
}
