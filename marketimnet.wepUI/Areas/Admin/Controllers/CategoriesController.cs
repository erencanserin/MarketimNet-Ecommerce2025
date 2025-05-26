using marketimnet.Core.Entities;
using marketimnet.Service.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using marketimnet.wepUI.Areas.Admin.Models;
using Microsoft.EntityFrameworkCore;
using marketimnet.Data;

namespace marketimnet.wepUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<CategoriesController>? _logger;

        public CategoriesController(ICategoryService categoryService, IWebHostEnvironment webHostEnvironment, ILogger<CategoriesController>? logger)
        {
            _categoryService = categoryService;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        // GET: Admin/Categories
        public async Task<IActionResult> Index()
        {
            try
            {
                var categories = await _categoryService.GetAllAsync();
                return View(categories);
            }
            catch (Exception ex)
            {
                // Log the error
                _logger?.LogError(ex, "Error loading categories");
                return Problem("Kategoriler yüklenirken bir hata oluştu.");
            }
        }

        // GET: Admin/Categories/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Admin/Categories/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                await LoadParentCategories();
                return View(new CategoryViewModel { IsActive = true });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Kategori oluşturma sayfası yüklenirken bir hata oluştu");
                TempData["Error"] = "Kategori sayfası yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        _logger?.LogInformation("Yeni kategori ekleniyor: {CategoryName}", model.Name);

                        var category = new Category
                        {
                            Name = model.Name,
                            Description = model.Description ?? string.Empty,
                            IsActive = model.IsActive,
                            IsTopMenu = model.IsTopMenu,
                            OrderNo = model.OrderNo,
                            ParentId = model.ParentId,
                            CreatedDate = DateTime.Now
                        };

                        if (model.ImageFile != null)
                        {
                            string imagePath = await SaveImageAsync(model.ImageFile);
                            category.Image = imagePath;
                        }

                        await _categoryService.AddAsync(category);
                        _logger?.LogInformation("Kategori başarıyla eklendi. ID: {CategoryId}", category.Id);

                        TempData["Success"] = "Kategori başarıyla eklendi.";

                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        {
                            return Json(new { success = true, message = "Kategori başarıyla eklendi.", redirectUrl = Url.Action("Index") });
                        }

                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Kategori eklenirken bir hata oluştu: {ErrorMessage}", ex.Message);
                        ModelState.AddModelError("", "Kategori eklenirken bir hata oluştu: " + ex.Message);
                        
                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        {
                            return StatusCode(500, "Kategori eklenirken bir hata oluştu: " + ex.Message);
                        }
                    }
                }
                else
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    _logger?.LogWarning("Geçersiz model durumu: {Errors}", string.Join("; ", errors));
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return BadRequest(new { errors = errors });
                    }
                }

                await LoadParentCategories();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Kategori ekleme işleminde beklenmeyen bir hata oluştu");
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return StatusCode(500, "Beklenmeyen bir hata oluştu: " + ex.Message);
                }
                
                TempData["Error"] = "Beklenmeyen bir hata oluştu: " + ex.Message;
                await LoadParentCategories();
                return View(model);
            }
        }

        // GET: Admin/Categories/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            await LoadParentCategories(id);
            var viewModel = new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                IsTopMenu = category.IsTopMenu,
                OrderNo = category.OrderNo,
                ParentId = category.ParentId,
                Image = category.Image
            };

            return View(viewModel);
        }

        // POST: Admin/Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var category = await _categoryService.GetByIdAsync(id);
                    if (category == null)
                    {
                        return NotFound();
                    }

                    category.Name = model.Name;
                    category.Description = model.Description;
                    category.IsActive = model.IsActive;
                    category.IsTopMenu = model.IsTopMenu;
                    category.OrderNo = model.OrderNo;
                    category.ParentId = model.ParentId;
                    category.UpdatedDate = DateTime.Now;

                    if (model.ImageFile != null)
                    {
                        // Eski resmi sil
                        if (!string.IsNullOrEmpty(category.Image))
                        {
                            DeleteImage(category.Image);
                        }

                        // Yeni resmi kaydet
                        string imagePath = await SaveImageAsync(model.ImageFile);
                        category.Image = imagePath;
                    }

                    await _categoryService.UpdateAsync(category);
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await CategoryExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            await LoadParentCategories(id);
            return View(model);
        }

        // GET: Admin/Categories/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Admin/Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category != null)
            {
                // Kategori resmini sil
                if (!string.IsNullOrEmpty(category.Image))
                {
                    DeleteImage(category.Image);
                }

                await _categoryService.DeleteAsync(category);
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> CategoryExists(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            return category != null;
        }

        private async Task LoadParentCategories(int? excludeId = null)
        {
            try
            {
                var categories = await _categoryService.GetAllAsync();
                categories = categories.Where(c => c.Id != excludeId && c.IsActive).ToList();
                ViewBag.ParentCategories = new SelectList(categories, "Id", "Name");
                _logger?.LogInformation("Üst kategoriler başarıyla yüklendi: {Count} adet", categories.Count());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Üst kategoriler yüklenirken bir hata oluştu");
                ViewBag.ParentCategories = new SelectList(Enumerable.Empty<SelectListItem>());
            }
        }

        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "img", "categories");
            Directory.CreateDirectory(uploadsFolder); // Klasör yoksa oluştur

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return "/img/categories/" + uniqueFileName;
        }

        private void DeleteImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return;

            string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, imagePath.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }

        // GET: Admin/Categories/GetCategories
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _categoryService.GetAllAsync();
                return Json(new { data = categories });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading categories for DataTable");
                return Json(new { error = "Kategoriler yüklenirken bir hata oluştu." });
            }
        }
    }
} 