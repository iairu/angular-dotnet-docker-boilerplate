using System.Linq.Expressions;

namespace Core.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(long id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate);
    Task<T?> GetSingleAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<int> CountAsync();
    Task<int> CountAsync(Expression<Func<T, bool>> predicate);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
}

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    Task<int> Complete();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

public interface IUserRepository : IRepository<Core.Entities.User>
{
    Task<Core.Entities.User?> GetByUsernameAsync(string username);
    Task<Core.Entities.User?> GetByEmailAsync(string email);
    Task<IReadOnlyList<Core.Entities.User>> GetUsersCreatedAfterAsync(DateTime date);
    Task<bool> UsernameExistsAsync(string username);
    Task<bool> EmailExistsAsync(string email);
}

public interface IDatabaseHealthService
{
    Task<bool> IsHealthyAsync();
    Task<string> GetDatabaseInfoAsync();
    Task TestConnectionAsync();
}