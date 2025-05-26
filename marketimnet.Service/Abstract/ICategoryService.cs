using marketimnet.Core.Entities;
using System.Linq.Expressions;

namespace marketimnet.Service.Abstract
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<IEnumerable<Category>> GetAllAsync(params Expression<Func<Category, object>>[] includes);
        Task<Category> GetByIdAsync(int id);
        Task<Category> AddAsync(Category entity);
        Task UpdateAsync(Category entity);
        Task DeleteAsync(Category entity);
        Task<bool> AnyAsync(Expression<Func<Category, bool>> expression);
        IQueryable<Category> GetQueryable();
        Task<int> GetTotalCategoryCountAsync();
    }
} 