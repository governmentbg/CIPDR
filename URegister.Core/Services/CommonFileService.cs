using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Core.Contracts;
using URegister.Core.Data;

namespace URegister.Core.Services
{
    public class CommonFileService : BaseService, ICommonFileService
    {
        public CommonFileService(IApplicationRepository repo,
            ILogger<CommonFileService> logger)
            : base(repo, logger)
        {
            
        }
    }
}
