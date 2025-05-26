using Microsoft.AspNetCore.Mvc;
using marketimnet.Core.Entities;
using marketimnet.Service.Abstract;
using Microsoft.AspNetCore.Authorization;
using marketimnet.wepUI.Areas.Admin.Filters;

namespace marketimnet.wepUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpGet]
        [ResponseCache(Duration = 60)] // 1 dakika önbellek
        public async Task<IActionResult> Index()
        {
            try
            {
                var orders = await _orderService.GetAllOrdersWithDetailsAsync();
                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Siparişler listelenirken hata oluştu");
                TempData["Error"] = "Siparişler yüklenirken bir hata oluştu.";
                return View(new List<Order>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdWithDetailsAsync(id);
                if (order == null)
                {
                    return NotFound();
                }
                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Sipariş detayları görüntülenirken hata oluştu. Sipariş ID: {id}");
                TempData["Error"] = "Sipariş detayları yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            try
            {
                var success = await _orderService.UpdateOrderStatusAsync(id, status);
                if (!success)
                {
                    return NotFound();
                }
                TempData["Success"] = "Sipariş durumu başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Sipariş durumu güncellenirken hata oluştu. Sipariş ID: {id}");
                TempData["Error"] = "Sipariş durumu güncellenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _orderService.DeleteOrderAsync(id);
            if (!success)
            {
                return NotFound();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}