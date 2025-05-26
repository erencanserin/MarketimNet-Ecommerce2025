using System.Diagnostics;
using marketimnet.Core.Entities;
using marketimnet.Service.Abstract;
using marketimnet.wepUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace marketimnet.wepUI.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IProductService productService,
            ICategoryService categoryService,
            ILogger<HomeController> logger)
        {
            _productService = productService;
            _categoryService = categoryService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try 
            {
                _logger?.LogInformation("Anasayfa ürünleri yükleniyor...");
                
                var products = await _productService.GetQueryable()
                    .Include(p => p.Category)
                    .Where(p => p.IsActive)
                    .OrderByDescending(p => p.CreatedDate)
                    .ToListAsync();

                _logger?.LogInformation($"{products.Count} adet ürün bulundu.");
                
                if (!products.Any())
                {
                    _logger?.LogWarning("Hiç ürün bulunamadı.");
                }

                return View(products);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Anasayfada ürünler yüklenirken hata oluştu: {Message}", ex.Message);
                return View(new List<Product>());
            }
        }

        [OutputCache(Duration = 3600)]  // 1 saat cache
        public IActionResult Privacy()
        {
            return View();
        }

        [Route("hakkimizda")]
        public IActionResult About()
        {
            return View();
        }

        [Route("iletisim")]
        public IActionResult Contact()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
