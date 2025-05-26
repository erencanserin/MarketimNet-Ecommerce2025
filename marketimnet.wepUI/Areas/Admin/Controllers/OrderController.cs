using marketimnet.Service.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace marketimnet.wepUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetAllOrdersWithDetailsAsync();
            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService.GetOrderByIdWithDetailsAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var success = await _orderService.UpdateOrderStatusAsync(id, status);
            if (!success)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _orderService.DeleteOrderAsync(id);
            if (!success)
            {
                return NotFound();
            }
            await _orderService.DeleteOrderAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
} 