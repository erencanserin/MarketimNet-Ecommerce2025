using marketimnet.Service.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace marketimnet.wepUI.ViewComponents
{
    public class CategoryMenuViewComponent : ViewComponent
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryMenuViewComponent> _logger;

        public CategoryMenuViewComponent(ICategoryService categoryService, ILogger<CategoryMenuViewComponent> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                var categories = await _categoryService.GetAllAsync();
                
                // Sadece aktif ve üst menüde gösterilecek kategorileri filtrele
                var menuCategories = categories
                    .Where(c => c.IsActive && c.IsTopMenu && !c.ParentId.HasValue)
                    .OrderBy(c => c.OrderNo)
                    .ToList();
                
                _logger.LogInformation("Ana menü için {Count} kategori yüklendi", menuCategories.Count);
                
                return View(menuCategories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategoriler yüklenirken bir hata oluştu");
                return View(new List<Core.Entities.Category>());
            }
        }
    }
} 