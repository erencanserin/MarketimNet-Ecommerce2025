using marketimnet.Core.Entities;
using System.Linq.Expressions;

namespace marketimnet.Service.Abstract
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<IEnumerable<Product>> GetAllAsync(params Expression<Func<Product, object>>[] includes);
        Task<Product> GetByIdAsync(int id);
        Task<Product> AddAsync(Product entity);
        Task UpdateAsync(Product entity);
        Task DeleteAsync(Product entity);
        Task<bool> AnyAsync(Expression<Func<Product, bool>> expression);
        IQueryable<Product> GetQueryable();
        Task<IEnumerable<Product>> GetAllAsync(Expression<Func<Product, object>> include, Expression<Func<Product, bool>> filter);
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);
        Task<int> GetTotalProductCountAsync();
    }
} 