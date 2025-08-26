



using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace URegister.NumberGenerator.Data
{
    /// <summary>
    /// Интерфейс за репозитори на генератора на номера
    /// </summary>
    public interface IAuditLogRepository
    {
        Task AddAsync<T>(T entity) where T : class;
        Task AddRangeAsync<T>(IEnumerable<T> entities) where T : class;
        IQueryable<T> All<T>() where T : class;
        IQueryable<T> All<T>(Expression<Func<T, bool>> search) where T : class;
        IQueryable<T> AllReadonly<T>() where T : class;
        IQueryable<T> AllReadonly<T>(Expression<Func<T, bool>> search) where T : class;
        Task<IDbContextTransaction> BeginTransactionAsync();
        void ChangeTrackerClear();
        Task<int> DeleteAsNoTrackingAsync<T>(Expression<Func<T, bool>> search) where T : class;
        Task DeleteAsync<T>(object id) where T : class;
        Task DeleteAsync<T>(Expression<Func<T, bool>> search) where T : class;
        void Detach<T>(T entity) where T : class;
        void Dispose();
        Task<T?> GetByIdAsync<T>(object id) where T : class;
        Task<T?> GetByIdsAsync<T>(object[] id) where T : class;
        Task<int> SaveChangesAsync();
    }
}
