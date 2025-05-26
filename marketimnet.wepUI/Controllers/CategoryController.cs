using Microsoft.AspNetCore.Mvc;
using marketimnet.Service.Abstract;

namespace marketimnet.wepUI.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var categories = await _categoryService.GetAllAsync();
                var activeCategories = categories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.OrderNo)
                    .ToList();

                return View(activeCategories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategoriler listelenirken bir hata oluştu");
                return View(new List<Core.Entities.Category>());
            }
        }
    }
} 