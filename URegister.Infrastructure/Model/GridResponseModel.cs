using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URegister.Infrastructure.Model
{
    public class GridResponseModel
    {
        public int page { get; set; }
        public int size { get; set; }
        public int total_rows { get; set; }
        public int total_pages { get; set; }
        public required object data { get; set; }
    }
}
