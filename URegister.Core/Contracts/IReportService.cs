using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Infrastructure.Model.Report;

namespace URegister.Core.Contracts
{
    public interface IReportService : IBaseService
    {
        /// <summary>
        /// Генериране на статистическа справка
        /// </summary>
        /// <param name="dateFrom">От дата</param>
        /// <param name="dateTo">До дата</param>
        /// <param name="registerTypeEntry">Начин на вписване в регистър</param>
        /// <returns></returns>
        public Task<StatisticalReportViewModel> GenerateStatisticalReport(DateTime? dateFrom, DateTime? dateTo, string registerTypeEntry);
    }
}
