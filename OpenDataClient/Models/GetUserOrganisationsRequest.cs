using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDataClient.Models
{
    internal class GetUserOrganisationsRequest : ApiRequestBase
    {
        public int RecordsPerPage { get; set; } = 100000;
    }
}
