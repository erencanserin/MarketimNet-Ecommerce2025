using Microsoft.AspNetCore.Mvc;
using marketimnet.Service.Abstract;
using marketimnet.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace marketimnet.wepUI.Controllers
{
    [Authorize] // Sadece giriş yapmış kullanıcılar erişebilir
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            IOrderService orderService,
            ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Kullanıcının email adresini al
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(userEmail))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Kullanıcının siparişlerini getir
                var orders = await _orderService.GetOrdersByCustomerAsync(userEmail);

                // Siparişleri tarihe göre sırala (en yeniden en eskiye)
                orders = orders.OrderByDescending(o => o.OrderDate).ToList();

                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Siparişler listelenirken bir hata oluştu");
                TempData["Error"] = "Siparişleriniz yüklenirken bir hata oluştu.";
                return View(new List<Order>());
            }
        }

        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                var order = await _orderService.GetByIdAsync(id);

                if (order == null || order.Email != userEmail)
                {
                    return NotFound();
                }

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş detayı görüntülenirken bir hata oluştu");
                TempData["Error"] = "Sipariş detayları yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
} 