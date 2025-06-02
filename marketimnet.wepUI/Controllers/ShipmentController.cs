using Microsoft.AspNetCore.Mvc;
using marketimnet.Service.Abstract;
using marketimnet.Core.ViewModels;
using System.Text.Json;

namespace marketimnet.wepUI.Controllers
{
    public class ShipmentController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<ShipmentController> _logger;

        public ShipmentController(
            IOrderService orderService,
            ILogger<ShipmentController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        public IActionResult Track()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Track(string email)
        {
            try
            {
                _logger.LogInformation($"Kargo takip sorgusu başladı. Email: {email}");

                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning("Email adresi boş gönderildi");
                    return Json(new { success = false, message = "Lütfen e-posta adresinizi giriniz." });
                }

                var orders = await _orderService.GetOrdersByCustomerAsync(email);
                _logger.LogInformation($"Bulunan sipariş sayısı: {orders?.Count ?? 0}");
                
                if (orders == null || !orders.Any())
                {
                    _logger.LogInformation($"'{email}' için sipariş bulunamadı");
                    return Json(new { success = false, message = "Bu e-posta adresine ait sipariş bulunamadı." });
                }

                var shipmentOrders = orders
                    .Where(o => o.Status != "Beklemede" && o.Status != "İptal Edildi")
                    .OrderByDescending(o => o.OrderDate)
                    .Select(o => new
                    {
                        orderId = o.Id,
                        orderDate = o.OrderDate,
                        status = o.Status ?? "Beklemede",
                        totalAmount = o.TotalAmount
                    })
                    .ToList();

                _logger.LogInformation($"Filtrelenmiş sipariş sayısı: {shipmentOrders.Count}");
                _logger.LogInformation($"Gönderilen veri: {JsonSerializer.Serialize(shipmentOrders)}");

                return Json(new { success = true, orders = shipmentOrders });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kargo takibi sırasında bir hata oluştu");
                return Json(new { success = false, message = "Kargo takibi sırasında bir hata oluştu. Lütfen daha sonra tekrar deneyiniz." });
            }
        }
    }
} 