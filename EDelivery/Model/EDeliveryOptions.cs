using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.Model
{
    public class EDeliveryOptions
    {
        public string ClientId { get; set; } = null!;
        public string TokenUrl { get; set; } = null!;
        public string EDeliveryUrl { get; set; } = null!;
        public string CertPath { get; set; } = null!;
        public string CertPass { get; set; } = null!;
    }
}
