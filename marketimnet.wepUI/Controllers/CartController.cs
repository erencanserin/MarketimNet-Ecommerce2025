using Microsoft.AspNetCore.Mvc;
using marketimnet.Core.Entities;
using marketimnet.Service.Abstract;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.SignalR;
using marketimnet.wepUI.Hubs;
using marketimnet.Core.ViewModels;

namespace marketimnet.wepUI.Controllers
{
    public class CartController : Controller
    {
        private readonly IProductService _productService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CartController> _logger;
        private readonly IAntiforgery _antiforgery;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IOrderService _orderService;
        private const string CartSessionKey = "Cart";
        private const int CacheDurationMinutes = 30;
        private const int CacheEntrySize = 1; // Size in bytes for cache entries

        public CartController(
            IProductService productService,
            IMemoryCache cache,
            ILogger<CartController> logger,
            IAntiforgery antiforgery,
            IHubContext<NotificationHub> hubContext,
            IOrderService orderService)
        {
            _productService = productService;
            _cache = cache;
            _logger = logger;
            _antiforgery = antiforgery;
            _hubContext = hubContext;
            _orderService = orderService;
        }

        private List<CartItem> GetCartFromSession()
        {
            try
            {
                var cartJson = HttpContext.Session.GetString(CartSessionKey);
                if (string.IsNullOrEmpty(cartJson))
                {
                    return new List<CartItem>();
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReferenceHandler = ReferenceHandler.Preserve,
                    WriteIndented = true
                };

                var cart = JsonSerializer.Deserialize<List<CartItem>>(cartJson, options);
                return cart ?? new List<CartItem>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sepet verisi session'dan alınırken hata oluştu");
                return new List<CartItem>();
            }
        }

        private void SaveCartToSession(List<CartItem> cart)
        {
            try
            {
                if (cart == null)
                {
                    _logger.LogWarning("SaveCartToSession'a null cart gönderildi");
                    return;
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReferenceHandler = ReferenceHandler.Preserve,
                    WriteIndented = true
                };

                // Circular reference'ları kaldır
                foreach (var item in cart)
                {
                    if (item.Product != null)
                    {
                        item.ProductName = item.Product.Name;
                        item.Price = item.Product.Price;
                        item.ImageUrl = item.Product.ImageUrl;
                        item.Product = null; // Circular reference'ı kaldır
                    }
                }

                var cartJson = JsonSerializer.Serialize(cart, options);
                HttpContext.Session.SetString(CartSessionKey, cartJson);
                _logger.LogInformation("Sepet session'a kaydedildi. Ürün sayısı: {Count}", cart.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sepet verisi session'a kaydedilirken hata oluştu");
                throw;
            }
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var cart = GetCartFromSession();
                var productTasks = cart.Select(async item =>
                {
                    var cacheKey = $"Product_{item.ProductId}";
                    if (!_cache.TryGetValue(cacheKey, out Product product))
                    {
                        product = await _productService.GetByIdAsync(item.ProductId);
                        if (product != null)
                        {
                            var cacheEntryOptions = new MemoryCacheEntryOptions()
                                .SetSize(CacheEntrySize)
                                .SetSlidingExpiration(TimeSpan.FromMinutes(CacheDurationMinutes));
                            _cache.Set(cacheKey, product, cacheEntryOptions);
                        }
                    }
                    item.Product = product;
                    item.ProductName = product?.Name;
                    item.Price = product?.Price ?? 0;
                    item.ImageUrl = product?.ImageUrl;
                    return item;
                });

                await Task.WhenAll(productTasks);
                return View(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sepet sayfası yüklenirken hata oluştu");
                return View(new List<CartItem>());
            }
        }

        public class AddToCartModel
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            try
            {
                _logger.LogInformation("AddToCart isteği alındı: ProductId={ProductId}, Quantity={Quantity}", 
                    productId, quantity);

                if (productId <= 0)
                {
                    _logger.LogWarning("Geçersiz ürün ID'si: {ProductId}", productId);
                    return Json(new { success = false, message = "Geçersiz ürün" });
                }

                if (quantity <= 0)
                {
                    _logger.LogWarning("Geçersiz miktar: {Quantity} for ProductId: {ProductId}", quantity, productId);
                    return Json(new { success = false, message = "Geçersiz miktar" });
                }

                var cacheKey = $"Product_{productId}";
                Product product;

                if (!_cache.TryGetValue(cacheKey, out product))
                {
                    _logger.LogInformation("Ürün cache'de bulunamadı, veritabanından alınıyor: {ProductId}", productId);
                    product = await _productService.GetByIdAsync(productId);
                    if (product == null)
                    {
                        _logger.LogWarning("Ürün bulunamadı: {ProductId}", productId);
                        return Json(new { success = false, message = "Ürün bulunamadı" });
                    }

                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSize(CacheEntrySize)
                        .SetSlidingExpiration(TimeSpan.FromMinutes(CacheDurationMinutes));
                    _cache.Set(cacheKey, product, cacheEntryOptions);
                }

                _logger.LogInformation("Ürün bulundu: {ProductName} (ID: {ProductId})", product.Name, product.Id);

                var cart = GetCartFromSession();
                var cartItem = cart.FirstOrDefault(i => i.ProductId == productId);

                if (cartItem != null)
                {
                    cartItem.Quantity += quantity;
                    cartItem.UpdatedDate = DateTime.Now;
                    _logger.LogInformation("Mevcut ürün güncellendi. Yeni miktar: {Quantity}", cartItem.Quantity);
                }
                else
                {
                    cart.Add(new CartItem
                    {
                        ProductId = productId,
                        ProductName = product.Name,
                        Price = product.Price,
                        ImageUrl = product.ImageUrl,
                        Quantity = quantity,
                        Product = product,
                        CreatedDate = DateTime.Now
                    });
                    _logger.LogInformation("Yeni ürün sepete eklendi: {ProductName}", product.Name);
                }

                SaveCartToSession(cart);

                var totalQuantity = cart.Sum(x => x.Quantity);
                _logger.LogInformation("Sepet güncellendi. Toplam ürün: {TotalQuantity}", totalQuantity);

                // Send real-time notification
                await _hubContext.Clients.All.SendAsync("ReceiveCartNotification", $"{product.Name} sepete eklendi", totalQuantity);
                
                return Json(new { success = true, message = "Ürün sepete eklendi", cartCount = totalQuantity });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün sepete eklenirken hata oluştu: {ProductId}", productId);
                return Json(new { success = false, message = "Ürün sepete eklenirken bir hata oluştu: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Geçersiz istek parametreleri" });
                }

                var cart = GetCartFromSession();
                var cartItem = cart.FirstOrDefault(i => i.ProductId == productId);

                if (cartItem == null)
                {
                    _logger.LogWarning("Güncellenecek ürün sepette bulunamadı: {ProductId}", productId);
                    return Json(new { success = false, message = "Ürün sepette bulunamadı" });
                }

                if (quantity > 0)
                {
                    cartItem.Quantity = quantity;
                    cartItem.UpdatedDate = DateTime.Now;
                }
                else
                {
                    cart.Remove(cartItem);
                }

                SaveCartToSession(cart);

                var totalQuantity = cart.Sum(x => x.Quantity);
                _logger.LogInformation("Sepet güncellendi: {ProductId}, Yeni Miktar: {Quantity}", productId, quantity);
                
                return Json(new { success = true, message = "Sepet güncellendi", cartCount = totalQuantity });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sepet güncellenirken hata oluştu: {ProductId}", productId);
                return Json(new { success = false, message = "Sepet güncellenirken bir hata oluştu" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromCart(int productId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Geçersiz istek parametreleri" });
                }

                var cart = GetCartFromSession();
                var cartItem = cart.FirstOrDefault(i => i.ProductId == productId);

                if (cartItem != null)
                {
                    cart.Remove(cartItem);
                    SaveCartToSession(cart);
                    _logger.LogInformation("Ürün sepetten kaldırıldı: {ProductId}", productId);
                }

                var totalQuantity = cart.Sum(x => x.Quantity);
                return Json(new { success = true, message = "Ürün sepetten kaldırıldı", cartCount = totalQuantity });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün sepetten kaldırılırken hata oluştu: {ProductId}", productId);
                return Json(new { success = false, message = "Ürün sepetten kaldırılırken bir hata oluştu" });
            }
        }

        public IActionResult Clear()
        {
            try
            {
                HttpContext.Session.Remove(CartSessionKey);
                _logger.LogInformation("Sepet temizlendi");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sepet temizlenirken hata oluştu");
                TempData["Error"] = "Sepet temizlenirken bir hata oluştu";
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult Checkout()
        {
            try
            {
                var cart = GetCartFromSession();
                if (cart == null || !cart.Any())
                {
                    TempData["Error"] = "Sepetinizde ürün bulunmamaktadır.";
                    return RedirectToAction(nameof(Index));
                }

                var model = new OrderViewModel
                {
                    CartItems = cart,
                    TotalAmount = cart.Sum(x => x.Price * x.Quantity)
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş sayfası yüklenirken hata oluştu");
                TempData["Error"] = "Sipariş sayfası yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(OrderViewModel model)
        {
            try
            {
                _logger.LogInformation("Checkout işlemi başlatıldı");
                _logger.LogInformation($"Gelen model: ShippingAddress={model.ShippingAddress}, CardHolderName={model.CardHolderName}");

                var cart = GetCartFromSession();
                if (cart == null || !cart.Any())
                {
                    _logger.LogWarning("Sepet boş veya null");
                    return Json(new { success = false, message = "Sepetinizde ürün bulunmamaktadır." });
                }

                _logger.LogInformation($"Sepet ürün sayısı: {cart.Count}, Toplam tutar: {cart.Sum(x => x.Price * x.Quantity)}");

                // Model validasyonu
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Model validation failed");
                    foreach (var modelState in ModelState.Values)
                    {
                        foreach (var error in modelState.Errors)
                        {
                            _logger.LogWarning($"Validation error: {error.ErrorMessage}");
                        }
                    }

                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return Json(new { success = false, message = "Lütfen tüm alanları doldurun.", errors = errors });
                }

                // Kredi kartı doğrulama ve ödeme işlemi burada yapılacak
                // Şimdilik sadece başarılı kabul ediyoruz
                bool paymentSuccess = true;

                if (!paymentSuccess)
                {
                    return Json(new { success = false, message = "Ödeme işlemi başarısız oldu. Lütfen tekrar deneyin." });
                }

                _logger.LogInformation("Sipariş oluşturuluyor...");

                // Sipariş oluştur
                var order = new Order
                {
                    OrderNumber = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper(),
                    OrderDate = DateTime.Now,
                    Status = "Beklemede",
                    FullName = model.CardHolderName,
                    Address = model.ShippingAddress,
                    Note = model.Notes,
                    TotalAmount = cart.Sum(x => x.Price * x.Quantity),
                    Email = null, // Opsiyonel alan
                    Phone = null, // Opsiyonel alan
                    City = null, // Opsiyonel alan
                    District = null, // Opsiyonel alan
                    ZipCode = null, // Opsiyonel alan
                    OrderItems = cart.Select(item => new OrderItem
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price,
                        TotalPrice = item.Price * item.Quantity
                    }).ToList()
                };

                _logger.LogInformation($"Sipariş nesnesi oluşturuldu: OrderNumber={order.OrderNumber}, TotalAmount={order.TotalAmount}");

                try
                {
                    // Siparişi kaydet
                    await _orderService.AddAsync(order);
                    _logger.LogInformation($"Sipariş başarıyla kaydedildi: {order.OrderNumber}");
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Sipariş veritabanına kaydedilirken hata oluştu");
                    throw; // Üst catch bloğuna yönlendir
                }

                // Sepeti temizle
                HttpContext.Session.Remove(CartSessionKey);
                _logger.LogInformation("Sepet temizlendi");

                // Bildirim gönder
                try
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveOrderNotification", $"Yeni sipariş alındı: {order.OrderNumber}");
                    _logger.LogInformation("Bildirim gönderildi");
                }
                catch (Exception hubEx)
                {
                    _logger.LogError(hubEx, "Bildirim gönderilirken hata oluştu");
                    // Bildirim hatası kritik değil, devam et
                }

                return Json(new { 
                    success = true, 
                    message = "Siparişiniz başarıyla oluşturuldu."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş oluşturulurken kritik hata oluştu");
                _logger.LogError($"Hata detayı: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"İç hata: {ex.InnerException.Message}");
                }
                return Json(new { success = false, message = "Sipariş oluşturulurken bir hata oluştu. Lütfen tekrar deneyin." });
            }
        }
    }
} 