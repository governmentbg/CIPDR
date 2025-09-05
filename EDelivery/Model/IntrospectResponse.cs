using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.Model
{
    public class IntrospectResponse
    {
        public bool active { get; set; }
        public string? token_type { get; set; }
        public string? client_id { get; set; }
        public string? username { get; set; }
        public string? sub { get; set; }
        public int exp { get; set; }
        public DateTime expstr { get; set; }
        public int iat { get; set; }
        public int nbf { get; set; }
        public DateTime nbfstr { get; set; }
        public string? scope { get; set; }
        public string? miscinfo { get; set; }
        public int consented_on { get; set; }
        public DateTime consented_on_str { get; set; }
        public string? grant_type { get; set; }
    }
}
