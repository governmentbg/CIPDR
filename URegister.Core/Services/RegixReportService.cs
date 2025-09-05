using Microsoft.Extensions.Logging;
using URegister.Core.Contracts;
using URegister.Core.Data;
using URegister.Core.Data.Models.Logging;

namespace URegister.Core.Services
{
    public class RegixReportService : BaseService, IRegixReportService
    {
        private readonly IUserContext _userContext;

        public RegixReportService(
            IApplicationRepository repo, 
            ILogger<BaseService> logger,
            IUserContext userContext) : base(repo, logger)
        {
            _userContext = userContext;
        }

        /// <summary>
        /// Записва в базата данни заявката към и отговора от Regix
        /// </summary>
        /// <param name="request">Заявка към Regix</param>
        /// <param name="response">Отговор от Regix</param>
        /// <param name="regixRequestType">Номенклатура за типа заявка</param>
        public async Task<SaveOperationResult> CreateRegixReport(string request, string response,
            string regixRequestType)
        {
            try
            {
                RegixReport report = new RegixReport()
                {
                    RequestData = request,
                    ResponseData = response,
                    EventDate = DateTime.UtcNow,
                    UserId = _userContext.UserId,
                    RegixRequestType = regixRequestType
                };

                await Repo.AddAsync(report);
                await Repo.SaveChangesAsync();

                return new SaveOperationResult(true, report.Id);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Проблем при запис на репорт за Regix заявка в {nameof(CreateRegixReport)}");
                return new SaveOperationResult("Проблем при запис на репорт за Regix заявка");
            }
        }
    }
}
