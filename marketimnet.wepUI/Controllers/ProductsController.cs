using marketimnet.Service.Abstract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using marketimnet.Core.Entities;
using marketimnet.wepUI.Models;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace marketimnet.wepUI.Controllers
{
    [OutputCache(Duration = 600)]  // 10 dakikaya çıkaralım
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ILogger<ProductsController> _logger;
        private readonly IMemoryCache _cache;
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

        public ProductsController(
            IProductService productService,
            ICategoryService categoryService,
            ILogger<ProductsController> logger,
            IMemoryCache cache)
        {
            _productService = productService;
            _categoryService = categoryService;
            _logger = logger;
            _cache = cache;
        }

        [OutputCache(Duration = 600, VaryByQueryKeys = new[] { "categoryId", "search", "page" })]
        public async Task<IActionResult> Index(int? categoryId, string? search, int page = 1)
        {
            const int pageSize = 12;

            var query = _productService.GetQueryable()
                .Include(p => p.Category)
                .Where(p => p.IsActive);

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(search) ||
                    p.Description.ToLower().Contains(search) ||
                    p.ProductCode.ToLower().Contains(search));
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            page = Math.Max(1, Math.Min(page, totalPages));

            var products = await query
                .OrderByDescending(p => p.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var categories = await _categoryService.GetAllAsync();

            var viewModel = new ProductListViewModel
            {
                Products = products,
                Categories = categories,
                CurrentCategory = categoryId.HasValue ? await _categoryService.GetByIdAsync(categoryId.Value) : null,
                SearchTerm = search,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(viewModel);
        }

        [OutputCache(Duration = 300)]
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var cacheKey = $"product_detail_{id}";
                
                if (_cache.TryGetValue(cacheKey, out ProductDetailViewModel cachedViewModel))
                {
                    return View(cachedViewModel);
                }

                var product = await _productService.GetByIdAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                var relatedProducts = await _productService.GetQueryable()
                    .Include(p => p.Category)
                    .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id && p.IsActive)
                    .Take(4)
                    .ToListAsync();

                var viewModel = new ProductDetailViewModel
                {
                    Product = product,
                    RelatedProducts = relatedProducts
                };

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
                    .SetSize(1);

                _cache.Set(cacheKey, viewModel, cacheOptions);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün detayı görüntülenirken bir hata oluştu");
                return View("Error");
            }
        }

        [OutputCache(Duration = 600)]
        public async Task<IActionResult> Discounted(int page = 1)
        {
            const int pageSize = 12;

            var query = _productService.GetQueryable()
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.DiscountedPrice.HasValue && p.DiscountedPrice < p.Price);

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            page = Math.Max(1, Math.Min(page, totalPages));

            var products = await query
                .OrderByDescending(p => (p.Price - p.DiscountedPrice) / p.Price) // En yüksek indirim oranına göre sırala
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var categories = await _categoryService.GetAllAsync();

            var viewModel = new ProductListViewModel
            {
                Products = products,
                Categories = categories,
                CurrentPage = page,
                TotalPages = totalPages,
                IsDiscountedPage = true
            };

            return View("Index", viewModel);
        }
    }
}