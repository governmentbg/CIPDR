using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URegister.Core.Models.Process
{
    public class CertificateVM
    {
        /// <summary>
        /// Идентификатор на процес
        /// </summary>
        public Guid ProcessId { get; set; }

        /// <summary>
        /// Идентификатор на файл
        /// </summary>
        public Guid? FileId { get; set; }
    }
}
