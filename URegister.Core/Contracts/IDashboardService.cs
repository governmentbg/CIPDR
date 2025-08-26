using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Core.Models.Process;

namespace URegister.Core.Contracts
{
     public interface IDashboardService
    {
        /// <summary>
        /// Връща данни за Dashboard
        /// </summary>     
        /// <returns></returns>
        public Task<DashboardVM> GetDashboardData();
    }
}
