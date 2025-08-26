using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using System.Linq.Expressions;
using URegister.AuditLog.Data;

namespace URegister.NumberGenerator.Data
{
    /// <summary>
    /// Репозитори за генератора на номера
    /// </summary>
    public class AuditLogRepository(AuditLogDbContext Context) : IAuditLogRepository
    {
        /// <summary>
        /// Representation of table in database
        /// </summary>
        protected DbSet<T> DbSet<T>() where T : class
        {
            return Context.Set<T>();
        }

      
        /// <summary>
        /// Adds entity to the database
        /// </summary>
        /// <param name="entity">Entity to add</param>
        public async Task AddAsync<T>(T entity) where T : class
        {
            await DbSet<T>().AddAsync(entity);
        }

        /// <summary>
        /// Ads collection of entities to the database
        /// </summary>
        /// <param name="entities">Enumerable list of entities</param>
        public async Task AddRangeAsync<T>(IEnumerable<T> entities) where T : class
        {
            await DbSet<T>().AddRangeAsync(entities);
        }

        /// <summary>
        /// All records in a table
        /// </summary>
        /// <returns>Queryable expression tree</returns>
        public IQueryable<T> All<T>() where T : class
        {
            return DbSet<T>().AsQueryable();
        }

        public IQueryable<T> All<T>(Expression<Func<T, bool>> search) where T : class
        {
            return this.DbSet<T>().Where(search).AsQueryable();
        }

        /// <summary>
        /// The result collection won't be tracked by the context
        /// </summary>
        /// <returns>Expression tree</returns>
        public IQueryable<T> AllReadonly<T>() where T : class
        {
            return this.DbSet<T>()
                .AsQueryable()
                .AsNoTracking();
        }
        public IQueryable<T> AllReadonly<T>(Expression<Func<T, bool>> search) where T : class
        {
            return this.DbSet<T>()
                .Where(search)
                .AsQueryable()
                .AsNoTracking();
        }

        /// <summary>
        /// Deletes a record from database
        /// </summary>
        /// <param name="id">Identificator of record to be deleted</param>
        public async Task DeleteAsync<T>(object id) where T : class
        {
            T entity = await GetByIdAsync<T>(id);

            Delete<T>(entity);
        }

        /// <summary>
        /// Deletes a record from database
        /// </summary>
        /// <param name="entity">Entity representing record to be deleted</param>
        public void Delete<T>(T entity) where T : class
        {
            EntityEntry entry = Context.Entry(entity);

            if (entry.State == EntityState.Detached)
            {
                this.DbSet<T>().Attach(entity);
            }
            entry.State = EntityState.Deleted;
        }

        /// <summary>
        /// Detaches given entity from the context
        /// </summary>
        /// <param name="entity">Entity to be detached</param>
        public void Detach<T>(T entity) where T : class
        {
            EntityEntry entry = Context.Entry(entity);

            entry.State = EntityState.Detached;
        }

        /// <summary>
        /// Disposing the context when it is not neede
        /// Don't have to call this method explicitely
        /// Leave it to the IoC container
        /// </summary>
        public void Dispose()
        {
            Context.Dispose();
        }

        /// <summary>
        /// Gets specific record from database by primary key
        /// </summary>
        /// <param name="id">record identificator</param>
        /// <returns>Single record or null</returns>
        public async Task<T?> GetByIdAsync<T>(object id) where T : class
        {
            return await DbSet<T>().FindAsync(id);
        }

        /// <summary>
        /// Gets specifi record from database by primary key
        /// </summary>
        /// <param name="id">Composite key</param>
        /// <returns>Single record or null</returns>
        public async Task<T?> GetByIdsAsync<T>(object[] id) where T : class
        {
            return await DbSet<T>().FindAsync(id);
        }

        /// <summary>
        /// Saves all made changes in transaction
        /// </summary>
        /// <returns>Number of entries written to the base</returns>
        public async Task<int> SaveChangesAsync()
        {
            return await Context.SaveChangesAsync();
        }

        public void DeleteRange<T>(IEnumerable<T> entities) where T : class
        {
            foreach (var entity in entities)
            {
                EntityEntry entry = Context.Entry(entity);

                if (entry.State == EntityState.Detached)
                {
                    DbSet<T>().Attach(entity);
                }

                entry.State = EntityState.Deleted;
            }
        }

        /// <summary>
        /// Truncate table
        /// </summary>
        /// <param name="table">Table name</param>
        public async Task Truncate(string table)
        {
            await Context.Database.ExecuteSqlAsync($"TRUNCATE TABLE {table} RESTART IDENTITY");
        }

        /// <summary>
        /// Clear change tracker
        /// </summary>
        public void ChangeTrackerClear()
        {
            Context.ChangeTracker.Clear();
        }

        /// <summary>
        /// Deletes records imediately from database without tracking
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="search">Predicate</param>
        /// <returns></returns>
        public async Task<int> DeleteAsNoTrackingAsync<T>(Expression<Func<T, bool>> search) where T : class
        {
            int result = 0;
            var collection = this.DbSet<T>()
                .Where(search);

            result = await collection.ExecuteDeleteAsync();
            
            return result;
        }

        public async Task DeleteAsync<T>(Expression<Func<T, bool>> search) where T : class
        {
            List<T> entities = await this.DbSet<T>()
                .Where(search)
                .ToListAsync();

            foreach (var entity in entities)
            {
                EntityEntry entry = Context.Entry(entity);

                if (entry.State == EntityState.Detached)
                {
                    DbSet<T>().Attach(entity);
                }

                entry.State = EntityState.Deleted;
            }
        }

        /// <summary>
        /// Begin Transaction
        /// </summary>
        /// <returns></returns>
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await Context.Database.BeginTransactionAsync();
        }
    }
}
