using Microsoft.Extensions.Logging;
using URegister.Core.Contracts;
using URegister.Core.Data;

namespace Core.Services
{
    public abstract class BaseService : IBaseService
    {
        protected BaseService(IApplicationRepository repo, ILogger<BaseService> logger)
        {
            this.Repo = repo;
            this.Logger = logger;
        }

        protected readonly IApplicationRepository Repo;
        protected readonly ILogger<BaseService> Logger;

        public async Task<T> GetByIdAsync<T>(object id) where T : class
        {
            return await Repo.GetByIdAsync<T>(id);
        }
    }
}
