using marketimnet.Service.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace marketimnet.wepUI.ViewComponents
{
    public class SidebarCategoryMenuViewComponent : ViewComponent
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<SidebarCategoryMenuViewComponent> _logger;

        public SidebarCategoryMenuViewComponent(ICategoryService categoryService, ILogger<SidebarCategoryMenuViewComponent> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                var categories = await _categoryService.GetAllAsync();
                
                // Tüm aktif kategorileri getir
                var sidebarCategories = categories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.OrderNo)
                    .ToList();
                
                _logger.LogInformation("Sidebar için {Count} kategori yüklendi", sidebarCategories.Count);
                
                return View(sidebarCategories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sidebar kategorileri yüklenirken bir hata oluştu");
                return View(new List<Core.Entities.Category>());
            }
        }
    }
} 