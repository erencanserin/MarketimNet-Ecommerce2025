using marketimnet.Data;
using marketimnet.Core.Entities;
using marketimnet.Service.Abstract;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace marketimnet.Service.Concrete
{
    public class UserService : IUserService
    {
        private readonly DatabaseContext _context;

        public UserService(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AppUser>> GetAllAsync()
        {
            return await _context.AppUsers
                .OrderByDescending(u => u.CreatedDate)
                .ToListAsync();
        }

        public async Task<AppUser> GetByIdAsync(int id)
        {
            return await _context.AppUsers.FindAsync(id);
        }

        public async Task<AppUser> GetByEmailAsync(string email)
        {
            return await _context.AppUsers
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<AppUser> AddAsync(AppUser user)
        {
            if (await _context.AppUsers.AnyAsync(u => u.Email == user.Email))
            {
                throw new InvalidOperationException("Bu e-posta adresi zaten kullanılıyor.");
            }

            user.CreatedDate = DateTime.Now;
            await _context.AppUsers.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateAsync(AppUser user)
        {
            var existingUser = await GetByIdAsync(user.Id);
            if (existingUser == null)
            {
                throw new InvalidOperationException("Güncellenecek kullanıcı bulunamadı.");
            }

            if (await _context.AppUsers.AnyAsync(u => u.Email == user.Email && u.Id != user.Id))
            {
                throw new InvalidOperationException("Bu e-posta adresi zaten kullanılıyor.");
            }

            existingUser.Name = user.Name;
            existingUser.Email = user.Email;
            existingUser.IsActive = user.IsActive;
            existingUser.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(AppUser user)
        {
            _context.AppUsers.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> AnyAsync(Expression<Func<AppUser, bool>> expression)
        {
            return await _context.AppUsers.AnyAsync(expression);
        }

        public async Task<AppUser> ValidateAdminAsync(string email, string password)
        {
            var admin = await _context.AppUsers
                .FirstOrDefaultAsync(u => u.Email == email && u.IsAdmin);

            if (admin == null)
            {
                return null;
            }

            var hashedPassword = HashPassword(password);
            if (admin.Password != hashedPassword)
            {
                return null;
            }

            return admin;
        }

        public async Task<AppUser> GetAdminUserAsync()
        {
            return await _context.AppUsers
                .FirstOrDefaultAsync(u => u.IsAdmin);
        }

        public async Task<int> GetTotalUserCountAsync()
        {
            return await _context.AppUsers.CountAsync();
        }

        public async Task<AppUser> UpdateAdminPasswordAsync(string currentPassword, string newPassword)
        {
            var admin = await _context.AppUsers.FirstOrDefaultAsync(u => u.IsAdmin);
            if (admin == null)
                return null;

            var currentHashedPassword = HashPassword(currentPassword);
            if (admin.Password != currentHashedPassword)
                return null;

            admin.Password = HashPassword(newPassword);
            admin.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            
            return admin;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
} 