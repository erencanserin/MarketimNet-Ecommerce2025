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
        public async Task<IActionResult> AddToCart([FromBody] AddToCartModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Geçersiz istek parametreleri" });
                }

                if (model.ProductId <= 0)
                {
                    _logger.LogWarning("Geçersiz ürün ID'si: {ProductId}", model.ProductId);
                    return Json(new { success = false, message = "Geçersiz ürün" });
                }

                if (model.Quantity <= 0)
                {
                    _logger.LogWarning("Geçersiz miktar: {Quantity} for ProductId: {ProductId}", model.Quantity, model.ProductId);
                    return Json(new { success = false, message = "Geçersiz miktar" });
                }

                var cacheKey = $"Product_{model.ProductId}";
                Product product;

                if (!_cache.TryGetValue(cacheKey, out product))
                {
                    product = await _productService.GetByIdAsync(model.ProductId);
                    if (product == null)
                    {
                        _logger.LogWarning("Ürün bulunamadı: {ProductId}", model.ProductId);
                        return Json(new { success = false, message = "Ürün bulunamadı" });
                    }

                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSize(CacheEntrySize)
                        .SetSlidingExpiration(TimeSpan.FromMinutes(CacheDurationMinutes));
                    _cache.Set(cacheKey, product, cacheEntryOptions);
                }

                var cart = GetCartFromSession();
                var cartItem = cart.FirstOrDefault(i => i.ProductId == model.ProductId);

                if (cartItem != null)
                {
                    cartItem.Quantity += model.Quantity;
                    cartItem.UpdatedDate = DateTime.Now;
                }
                else
                {
                    cart.Add(new CartItem
                    {
                        ProductId = model.ProductId,
                        ProductName = product.Name,
                        Price = product.Price,
                        ImageUrl = product.ImageUrl,
                        Quantity = model.Quantity,
                        Product = product,
                        CreatedDate = DateTime.Now
                    });
                }

                SaveCartToSession(cart);

                var totalQuantity = cart.Sum(x => x.Quantity);
                _logger.LogInformation("Ürün sepete eklendi: {ProductId}, Toplam Miktar: {TotalQuantity}", model.ProductId, totalQuantity);

                // Send real-time notification
                await _hubContext.Clients.All.SendAsync("ReceiveCartNotification", $"{product.Name} sepete eklendi", totalQuantity);
                
                return Json(new { success = true, message = "Ürün sepete eklendi", cartCount = totalQuantity });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün sepete eklenirken hata oluştu: {ProductId}", model.ProductId);
                return Json(new { success = false, message = "Ürün sepete eklenirken bir hata oluştu" });
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
                var cart = GetCartFromSession();
                if (cart == null || !cart.Any())
                {
                    ModelState.AddModelError("", "Sepetinizde ürün bulunmamaktadır.");
                    return View(model);
                }

                if (!ModelState.IsValid)
                {
                    model.CartItems = cart;
                    model.TotalAmount = cart.Sum(x => x.Price * x.Quantity);
                    return View(model);
                }

                // Kredi kartı doğrulama ve ödeme işlemi burada yapılacak
                // Şimdilik sadece başarılı kabul ediyoruz
                bool paymentSuccess = true;

                if (!paymentSuccess)
                {
                    ModelState.AddModelError("", "Ödeme işlemi başarısız oldu. Lütfen tekrar deneyin.");
                    model.CartItems = cart;
                    model.TotalAmount = cart.Sum(x => x.Price * x.Quantity);
                    return View(model);
                }

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
                    OrderItems = cart.Select(item => new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price,
                        TotalPrice = item.Price * item.Quantity
                    }).ToList()
                };

                // Siparişi kaydet
                await _orderService.AddAsync(order);

                // Sepeti temizle
                HttpContext.Session.Remove(CartSessionKey);

                // Bildirim gönder
                await _hubContext.Clients.All.SendAsync("ReceiveOrderNotification", $"Yeni sipariş alındı: {order.OrderNumber}");

                TempData["Success"] = "Siparişiniz başarıyla oluşturuldu.";
                return RedirectToAction("OrderConfirmation", "Order", new { orderNumber = order.OrderNumber });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş oluşturulurken hata oluştu");
                ModelState.AddModelError("", "Sipariş oluşturulurken bir hata oluştu. Lütfen tekrar deneyin.");
                model.CartItems = GetCartFromSession();
                model.TotalAmount = model.CartItems.Sum(x => x.Price * x.Quantity);
                return View(model);
            }
        }

        public async Task<IActionResult> OrderConfirmation(string orderNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(orderNumber))
                {
                    return RedirectToAction("Index", "Home");
                }

                var order = await _orderService.GetByOrderNumberAsync(orderNumber);
                if (order == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş onay sayfası yüklenirken hata oluştu");
                return RedirectToAction("Index", "Home");
            }
        }
    }
} 