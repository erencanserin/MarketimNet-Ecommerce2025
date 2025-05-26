using marketimnet.Data;
using marketimnet.Core.Entities;
using marketimnet.Service.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace marketimnet.Service.Concrete
{
    public class CategoryService : ICategoryService
    {
        private readonly DatabaseContext _context;

        public CategoryService(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories
                .Include(c => c.Parent)
                .Include(c => c.SubCategories)
                .OrderBy(c => c.OrderNo)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetAllAsync(params Expression<Func<Category, object>>[] includes)
        {
            var query = _context.Categories.AsQueryable();
            
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query
                .OrderBy(c => c.OrderNo)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Category> GetByIdAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.Parent)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Category> AddAsync(Category entity)
        {
            if (entity.ParentId.HasValue)
            {
                var parentExists = await _context.Categories.AnyAsync(c => c.Id == entity.ParentId);
                if (!parentExists)
                {
                    throw new InvalidOperationException("Seçilen üst kategori bulunamadı.");
                }
            }

            if (entity.OrderNo == 0)
            {
                var maxOrderNo = await _context.Categories
                    .Where(c => c.ParentId == entity.ParentId)
                    .MaxAsync(c => (int?)c.OrderNo) ?? 0;
                entity.OrderNo = maxOrderNo + 1;
            }

            entity.CreatedDate = DateTime.Now;
            await _context.Categories.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(Category entity)
        {
            var existingCategory = await GetByIdAsync(entity.Id);
            if (existingCategory == null)
            {
                throw new InvalidOperationException("Güncellenecek kategori bulunamadı.");
            }

            if (entity.ParentId.HasValue)
            {
                if (entity.ParentId == entity.Id)
                {
                    throw new InvalidOperationException("Bir kategori kendisini üst kategori olarak seçemez.");
                }

                var childCategories = await GetAllChildCategoriesAsync(entity.Id);
                if (childCategories.Any(c => c.Id == entity.ParentId))
                {
                    throw new InvalidOperationException("Bir kategori, kendi alt kategorisini üst kategori olarak seçemez.");
                }
            }

            existingCategory.Name = entity.Name;
            existingCategory.Description = entity.Description;
            existingCategory.Image = entity.Image;
            existingCategory.IsActive = entity.IsActive;
            existingCategory.IsTopMenu = entity.IsTopMenu;
            existingCategory.ParentId = entity.ParentId;
            existingCategory.OrderNo = entity.OrderNo;
            existingCategory.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Category entity)
        {
            var hasChildren = await _context.Categories.AnyAsync(c => c.ParentId == entity.Id);
            if (hasChildren)
            {
                throw new InvalidOperationException("Bu kategorinin alt kategorileri var. Önce alt kategorileri silmelisiniz.");
            }

            _context.Categories.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> AnyAsync(Expression<Func<Category, bool>> expression)
        {
            return await _context.Categories.AnyAsync(expression);
        }

        public IQueryable<Category> GetQueryable()
        {
            return _context.Categories;
        }

        private async Task<IEnumerable<Category>> GetAllChildCategoriesAsync(int parentId)
        {
            var allCategories = await GetAllAsync();
            var result = new List<Category>();

            void AddChildren(int pid)
            {
                var children = allCategories.Where(c => c.ParentId == pid);
                foreach (var child in children)
                {
                    result.Add(child);
                    AddChildren(child.Id);
                }
            }

            AddChildren(parentId);
            return result;
        }

        public async Task<int> GetTotalCategoryCountAsync()
        {
            return await _context.Categories.CountAsync();
        }
    }
} 