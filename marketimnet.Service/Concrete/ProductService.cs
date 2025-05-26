using marketimnet.Data;
using marketimnet.Core.Entities;
using marketimnet.Service.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace marketimnet.Service.Concrete
{
    public class ProductService : IProductService
    {
        private readonly DatabaseContext _context;

        public ProductService(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedDate)
                .AsNoTracking()
                .Select(p => new Product
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    CategoryId = p.CategoryId,
                    Category = new Category { Id = p.Category.Id, Name = p.Category.Name }
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetAllAsync(params Expression<Func<Product, object>>[] includes)
        {
            var query = _context.Products.AsQueryable();
            
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query
                .OrderByDescending(p => p.CreatedDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetAllAsync(Expression<Func<Product, object>> include, Expression<Func<Product, bool>> filter)
        {
            return await _context.Products
                .Include(include)
                .Where(filter)
                .OrderByDescending(p => p.CreatedDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product> AddAsync(Product product)
        {
            // Kategori kontrolü
            if (product.CategoryId.HasValue && !await _context.Categories.AnyAsync(c => c.Id == product.CategoryId))
            {
                throw new InvalidOperationException("Seçilen kategori bulunamadı.");
            }

            // Ürün kodu benzersiz olmalı
            if (await _context.Products.AnyAsync(p => p.ProductCode == product.ProductCode))
            {
                throw new InvalidOperationException("Bu ürün kodu zaten kullanılıyor.");
            }

            product.CreatedDate = DateTime.Now;
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task UpdateAsync(Product product)
        {
            var existingProduct = await GetByIdAsync(product.Id);
            if (existingProduct == null)
            {
                throw new InvalidOperationException("Güncellenecek ürün bulunamadı.");
            }

            // Kategori kontrolü
            if (product.CategoryId.HasValue && !await _context.Categories.AnyAsync(c => c.Id == product.CategoryId))
            {
                throw new InvalidOperationException("Seçilen kategori bulunamadı.");
            }

            // Ürün kodu benzersiz olmalı (kendi ID'si hariç)
            if (await _context.Products.AnyAsync(p => p.ProductCode == product.ProductCode && p.Id != product.Id))
            {
                throw new InvalidOperationException("Bu ürün kodu zaten kullanılıyor.");
            }

            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.DiscountedPrice = product.DiscountedPrice;
            existingProduct.ProductCode = product.ProductCode;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.IsActive = product.IsActive;
            existingProduct.ImageUrl = product.ImageUrl;
            existingProduct.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Product product)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> AnyAsync(Expression<Func<Product, bool>> expression)
        {
            return await _context.Products.AnyAsync(expression);
        }

        public IQueryable<Product> GetQueryable()
        {
            return _context.Products
                .Include(p => p.Category);
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            var category = await _context.Categories
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category == null)
            {
                throw new InvalidOperationException("Kategori bulunamadı.");
            }

            var categoryIds = new List<int> { categoryId };
            void AddSubCategoryIds(ICollection<Category> subCategories)
            {
                if (subCategories == null) return;
                
                foreach (var subCategory in subCategories)
                {
                    categoryIds.Add(subCategory.Id);
                    if (subCategory.SubCategories != null)
                    {
                        AddSubCategoryIds(subCategory.SubCategories);
                    }
                }
            }

            if (category.SubCategories != null)
            {
                AddSubCategoryIds(category.SubCategories);
            }

            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId.HasValue && categoryIds.Contains(p.CategoryId.Value))
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }

        public async Task<int> GetTotalProductCountAsync()
        {
            return await _context.Products.CountAsync();
        }
    }
}