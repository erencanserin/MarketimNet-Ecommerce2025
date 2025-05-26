using marketimnet.Service.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace marketimnet.wepUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IProductService productService,
            ICategoryService categoryService,
            ILogger<DashboardController> logger)
        {
            _productService = productService;
            _categoryService = categoryService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var totalProducts = await _productService.GetTotalProductCountAsync();
                var totalCategories = await _categoryService.GetTotalCategoryCountAsync();

                ViewBag.TotalProducts = totalProducts;
                ViewBag.TotalCategories = totalCategories;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dashboard yüklenirken bir hata oluştu");
                TempData["Error"] = "Dashboard yüklenirken bir hata oluştu: " + ex.Message;
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var totalProducts = await _productService.GetTotalProductCountAsync();
                var totalCategories = await _categoryService.GetTotalCategoryCountAsync();

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        totalProducts,
                        totalCategories
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dashboard istatistikleri alınırken bir hata oluştu");
                return Json(new
                {
                    success = false,
                    message = "İstatistikler alınırken bir hata oluştu: " + ex.Message
                });
            }
        }
    }
} 