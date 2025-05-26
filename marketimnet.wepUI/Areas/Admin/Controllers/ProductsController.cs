using marketimnet.Core.Entities;
using marketimnet.Data;
using marketimnet.Service.Abstract;
using marketimnet.wepUI.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace marketimnet.wepUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            IProductService productService,
            ICategoryService categoryService,
            IWebHostEnvironment hostEnvironment,
            ILogger<ProductsController> logger)
        {
            _productService = productService;
            _categoryService = categoryService;
            _hostEnvironment = hostEnvironment;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ürün listesi yüklenirken bir hata oluştu");
                return Problem("Ürün listesi yüklenirken bir hata oluştu");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                _logger?.LogInformation("Ürünler getiriliyor...");
                
                var products = await _productService.GetQueryable()
                    .Include(p => p.Category)
                    .AsNoTracking()
                    .ToListAsync();
                
                if (products == null || !products.Any())
                {
                    _logger?.LogWarning("Hiç ürün bulunamadı");
                    return Json(new { data = new object[] { } });
                }

                var productsData = products.Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    description = p.Description,
                    price = p.Price,
                    discountedPrice = p.DiscountedPrice,
                    productCode = p.ProductCode,
                    imageUrl = !string.IsNullOrEmpty(p.ImageUrl) ? p.ImageUrl : "/img/no-image.png",
                    categoryName = p.Category?.Name ?? "Kategori Yok",
                    isActive = p.IsActive,
                    isHome = p.IsHome,
                    createdDate = p.CreatedDate.ToString("dd.MM.yyyy HH:mm")
                }).OrderByDescending(p => p.id).ToList();

                _logger?.LogInformation($"{productsData.Count} adet ürün başarıyla getirildi");
                return Json(new { data = productsData });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ürünler yüklenirken bir hata oluştu");
                return Json(new { error = "Ürünler yüklenirken bir hata oluştu: " + ex.Message });
            }
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                _logger?.LogInformation("Ürün ekleme sayfası yükleniyor");
                
                var categories = await _categoryService.GetAllAsync();

                if (!categories.Any())
                {
                    TempData["Warning"] = "Henüz hiç kategori eklenmemiş. Önce kategori eklemeniz gerekiyor.";
                }

                var viewModel = new ProductViewModel
                {
                    Categories = new SelectList(categories.Where(c => c.IsActive), "Id", "Name"),
                    IsActive = true,
                    Price = 0 // Varsayılan fiyat 0 olarak ayarlandı
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ürün ekleme sayfası yüklenirken bir hata oluştu");
                TempData["Error"] = "Sayfa yüklenirken bir hata oluştu. Lütfen tekrar deneyin.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        _logger?.LogInformation("Yeni ürün ekleniyor: {ProductName}", model.Name);
                        
                        string? uniqueFileName = null;
                        if (model.Image != null)
                        {
                            string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "img", "products");
                            Directory.CreateDirectory(uploadsFolder); // Klasör yoksa oluştur
                            uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Image.FileName;
                            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await model.Image.CopyToAsync(fileStream);
                            }
                        }

                        var product = new Product
                        {
                            Name = model.Name,
                            Description = model.Description ?? string.Empty,
                            Price = model.Price,
                            DiscountedPrice = model.DiscountedPrice,
                            ProductCode = model.ProductCode,
                            CategoryId = model.CategoryId,
                            IsActive = model.IsActive,
                            IsHome = true,
                            ImageUrl = uniqueFileName != null ? "/img/products/" + uniqueFileName : null,
                            CreatedDate = DateTime.Now
                        };

                        var addedProduct = await _productService.AddAsync(product);
                        _logger?.LogInformation("Ürün başarıyla eklendi. ID: {ProductId}", addedProduct.Id);

                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        {
                            return Json(new { 
                                success = true, 
                                message = "Ürün başarıyla eklendi", 
                                id = addedProduct.Id,
                                name = addedProduct.Name
                            });
                        }

                        TempData["Success"] = "Ürün başarıyla eklendi.";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Ürün eklenirken bir hata oluştu: {Message}", ex.Message);
                        ModelState.AddModelError("", "Ürün eklenirken bir hata oluştu: " + ex.Message);
                        
                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        {
                            return StatusCode(500, "Ürün eklenirken bir hata oluştu: " + ex.Message);
                        }
                    }
                }
                else
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    _logger?.LogWarning("Geçersiz model durumu: {Errors}", string.Join("; ", errors));
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return BadRequest(ModelState);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ürün ekleme işleminde beklenmeyen bir hata oluştu");
                ModelState.AddModelError("", "Beklenmeyen bir hata oluştu: " + ex.Message);
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return StatusCode(500, "Beklenmeyen bir hata oluştu: " + ex.Message);
                }
            }

            try
            {
                var categories = await _categoryService.GetAllAsync();
                model.Categories = new SelectList(categories.Where(c => c.IsActive), "Id", "Name", model.CategoryId);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ürün ekleme formunu yeniden yüklerken hata oluştu");
                TempData["Error"] = "Form yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var categories = await _categoryService.GetAllAsync();

            var viewModel = new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                DiscountedPrice = product.DiscountedPrice,
                ImageUrl = product.ImageUrl,
                IsActive = product.IsActive,
                IsHome = product.IsHome,
                CategoryId = product.CategoryId,
                ProductCode = product.ProductCode,
                Categories = new SelectList(categories.Where(c => c.IsActive), "Id", "Name")
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var product = await _productService.GetByIdAsync(id);
                    if (product == null)
                    {
                        return NotFound();
                    }

                    string uniqueFileName = null;
                    if (model.Image != null)
                    {
                        // Eski resmi sil
                        if (!string.IsNullOrEmpty(product.ImageUrl))
                        {
                            string oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, product.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "img", "products");
                        Directory.CreateDirectory(uploadsFolder); // Klasör yoksa oluştur
                        uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Image.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.Image.CopyToAsync(fileStream);
                        }

                        product.ImageUrl = "/img/products/" + uniqueFileName;
                    }

                    product.Name = model.Name;
                    product.Description = model.Description;
                    product.Price = model.Price;
                    product.DiscountedPrice = model.DiscountedPrice;
                    product.ProductCode = model.ProductCode;
                    product.CategoryId = model.CategoryId;
                    product.IsActive = model.IsActive;
                    product.IsHome = model.IsHome;
                    product.UpdatedDate = DateTime.Now;

                    await _productService.UpdateAsync(product);
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ProductExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            var categories = await _categoryService.GetAllAsync();
            model.Categories = new SelectList(categories, "Id", "Name", model.CategoryId);
            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product != null)
            {
                // Ürün resmini sil
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    string imagePath = Path.Combine(_hostEnvironment.WebRootPath, product.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                await _productService.DeleteAsync(product);
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> ProductExists(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            return product != null;
        }
    }
} 