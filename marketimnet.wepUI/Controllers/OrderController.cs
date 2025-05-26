using Microsoft.AspNetCore.Mvc;
using marketimnet.Core.Entities;
using marketimnet.Service.Abstract;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.SignalR;
using marketimnet.wepUI.Hubs;
using marketimnet.wepUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using marketimnet.Core.ViewModels;

namespace marketimnet.wepUI.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<OrderController> _logger;
        private const string CartSessionKey = "CartItems";

        public OrderController(
            IOrderService orderService, 
            IProductService productService,
            IHubContext<NotificationHub> hubContext,
            ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _productService = productService;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<IActionResult> Checkout()
        {
            try
            {
                var cartItems = await GetCartItems();
                if (!cartItems.Any())
                {
                    TempData["Error"] = "Sepetinizde ürün bulunmamaktadır.";
                    return RedirectToAction("Index", "Cart");
                }

                var model = new OrderViewModel
                {
                    CartItems = cartItems,
                    TotalAmount = cartItems.Sum(item => item.Product.Price * item.Quantity)
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Checkout sayfası yüklenirken hata oluştu");
                TempData["Error"] = "Sipariş sayfası yüklenirken bir hata oluştu.";
                return RedirectToAction("Index", "Cart");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(OrderViewModel model)
        {
            try
            {
                var cartItems = await GetCartItems();
                if (!cartItems.Any())
                {
                    ModelState.AddModelError("", "Sepetinizde ürün bulunmamaktadır.");
                    return View(model);
                }

                if (!ModelState.IsValid)
                {
                    model.CartItems = cartItems;
                    model.TotalAmount = cartItems.Sum(item => item.Product.Price * item.Quantity);
                    return View(model);
                }

                // Kredi kartı doğrulama ve ödeme işlemi burada yapılacak
                // Şimdilik sadece başarılı kabul ediyoruz
                bool paymentSuccess = true;

                if (!paymentSuccess)
                {
                    ModelState.AddModelError("", "Ödeme işlemi başarısız oldu. Lütfen tekrar deneyin.");
                    model.CartItems = cartItems;
                    model.TotalAmount = cartItems.Sum(item => item.Product.Price * item.Quantity);
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
                    TotalAmount = model.TotalAmount,
                    OrderItems = cartItems.Select(item => new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Product.Price,
                        TotalPrice = item.Product.Price * item.Quantity
                    }).ToList()
                };

                // Siparişi kaydet
                await _orderService.AddAsync(order);

                // Sepeti temizle
                HttpContext.Session.Remove(CartSessionKey);

                // Bildirim gönder
                await _hubContext.Clients.All.SendAsync("ReceiveOrderNotification", $"Yeni sipariş alındı: {order.OrderNumber}");

                TempData["Success"] = "Siparişiniz başarıyla oluşturuldu.";
                return RedirectToAction("OrderConfirmation", new { orderNumber = order.OrderNumber });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş oluşturulurken hata oluştu");
                ModelState.AddModelError("", "Sipariş oluşturulurken bir hata oluştu. Lütfen tekrar deneyin.");
                model.CartItems = await GetCartItems();
                model.TotalAmount = model.CartItems.Sum(item => item.Product.Price * item.Quantity);
                return View(model);
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var order = await _orderService.GetByIdAsync(id);
                if (order == null)
                {
                    return NotFound();
                }

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş detayları görüntülenirken hata oluştu: {OrderId}", id);
                TempData["Error"] = "Sipariş detayları görüntülenirken bir hata oluştu.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            try
            {
                var order = await _orderService.GetByIdAsync(id);
                if (order == null)
                {
                    return NotFound();
                }

                order.Status = status;
                order.UpdatedDate = DateTime.Now;

                await _orderService.UpdateAsync(order);

                // Bildirim gönder
                await _hubContext.Clients.All.SendAsync("ReceiveOrderNotification", $"Sipariş durumu güncellendi: {order.OrderNumber} - {status}");

                TempData["Success"] = "Sipariş durumu başarıyla güncellendi.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş durumu güncellenirken hata oluştu: {OrderId}", id);
                TempData["Error"] = "Sipariş durumu güncellenirken bir hata oluştu.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        public async Task<IActionResult> OrderConfirmation(string orderNumber)
        {
            var order = await _orderService.GetByOrderNumberAsync(orderNumber);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        private async Task<List<CartItem>> GetCartItems()
        {
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(cartJson))
                return new List<CartItem>();

            var cartItems = JsonSerializer.Deserialize<List<CartItem>>(cartJson);
            
            // Load product information for each cart item
            foreach (var item in cartItems)
            {
                item.Product = await _productService.GetByIdAsync(item.ProductId);
            }
            
            return cartItems;
        }
    }
} 