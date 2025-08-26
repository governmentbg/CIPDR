using Microsoft.EntityFrameworkCore;
using URegister.Core.Services;

namespace URegister.Core.Contracts
{
    /// <summary>
    /// "Записва в базата данни заявката към и отговора от Regix"
    /// </summary>
    public interface IRegixReportService
    {
        /// <summary>
        /// Записва в базата данни заявката към и отговора от Regix
        /// </summary>
        /// <param name="request">Заявка към Regix</param>
        /// <param name="response">Отговор от Regix</param>
        /// <param name="regixRequestType">Номенклатура за типа заявка</param>
        public Task<SaveOperationResult> CreateRegixReport(string request, string response, string regixRequestType);
    }
}
