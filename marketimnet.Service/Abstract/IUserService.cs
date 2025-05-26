using marketimnet.Core.Entities;
using System.Linq.Expressions;

namespace marketimnet.Service.Abstract
{
    public interface IUserService
    {
        Task<IEnumerable<AppUser>> GetAllAsync();
        Task<AppUser> GetByIdAsync(int id);
        Task<AppUser> GetByEmailAsync(string email);
        Task<AppUser> AddAsync(AppUser user);
        Task UpdateAsync(AppUser user);
        Task DeleteAsync(AppUser user);
        Task<bool> AnyAsync(Expression<Func<AppUser, bool>> expression);
        Task<int> GetTotalUserCountAsync();
        Task<AppUser> ValidateAdminAsync(string email, string password);
        Task<AppUser> GetAdminUserAsync();
        Task<AppUser> UpdateAdminPasswordAsync(string currentPassword, string newPassword);
    }
} 