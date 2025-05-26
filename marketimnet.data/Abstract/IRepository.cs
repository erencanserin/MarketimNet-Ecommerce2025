using System.Linq.Expressions;

namespace marketimnet.Data.Abstract
{
    public interface IRepository<T> where T : class
    {
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<int> DeleteAsync(T entity);
        Task<T> GetByIdAsync(int id);
        Task<List<T>> GetAllAsync();
        Task<List<T>> GetAllAsync(Expression<Func<T, bool>> expression);
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> expression);
        Task<bool> AnyAsync(Expression<Func<T, bool>> expression);
    }
} 