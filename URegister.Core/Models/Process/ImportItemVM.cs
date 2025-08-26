using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Infrastructure.Constants;

namespace URegister.Core.Models.Process
{
    public class ImportItemVM
    {
        public string Key { get; set; } = null!;
        public string? Value { get; set; }

        public string? Error { get; set; }
    }
}
