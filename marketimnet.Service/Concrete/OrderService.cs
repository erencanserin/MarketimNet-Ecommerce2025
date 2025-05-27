using marketimnet.Core.Entities;
using marketimnet.Core.ViewModels;
using marketimnet.Data;
using marketimnet.Data.Abstract;
using marketimnet.Service.Abstract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace marketimnet.Service.Concrete
{
    public class OrderService : IOrderService
    {
        private readonly IRepository<Order> _repository;
        private readonly DatabaseContext _context;
        private readonly ILogger<OrderService> _logger;

        public OrderService(IRepository<Order> repository, DatabaseContext context, ILogger<OrderService> logger)
        {
            _repository = repository;
            _context = context;
            _logger = logger;
        }

        public async Task<Order> AddAsync(Order entity)
        {
            try
            {
                _logger.LogInformation($"Sipariş ekleniyor: {entity.OrderNumber}");
                var result = await _repository.AddAsync(entity);
                _logger.LogInformation($"Sipariş başarıyla eklendi: {entity.OrderNumber}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Sipariş eklenirken hata oluştu: {entity.OrderNumber}");
                throw;
            }
        }

        public async Task<bool> AnyAsync(Expression<Func<Order, bool>> expression)
        {
            return await _repository.AnyAsync(expression);
        }

        public async Task<bool> DeleteAsync(Order entity)
        {
            try
            {
                _logger.LogInformation($"Sipariş siliniyor: {entity.OrderNumber}");
                var result = await _repository.DeleteAsync(entity);
                _logger.LogInformation($"Sipariş başarıyla silindi: {entity.OrderNumber}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Sipariş silinirken hata oluştu: {entity.OrderNumber}");
                throw;
            }
        }

        public async Task<List<Order>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Tüm siparişler getiriliyor");
                var orders = await _repository.GetAllAsync();
                _logger.LogInformation($"Toplam {orders.Count} sipariş getirildi");
                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Siparişler getirilirken hata oluştu");
                throw;
            }
        }

        public async Task<List<Order>> GetAllAsync(Expression<Func<Order, bool>> expression)
        {
            return await _repository.GetAllAsync(expression);
        }

        public async Task<Order> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Sipariş getiriliyor: {id}");
                var order = await _repository.GetByIdAsync(id);
                if (order != null)
                {
                    _logger.LogInformation($"Sipariş bulundu: {order.OrderNumber}");
                }
                else
                {
                    _logger.LogWarning($"Sipariş bulunamadı: {id}");
                }
                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Sipariş getirilirken hata oluştu: {id}");
                throw;
            }
        }

        public async Task<Order> GetByOrderNumberAsync(string orderNumber)
        {
            return await _repository.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        }

        public async Task<Order> UpdateAsync(Order entity)
        {
            try
            {
                _logger.LogInformation($"Sipariş güncelleniyor: {entity.OrderNumber}");
                var result = await _repository.UpdateAsync(entity);
                _logger.LogInformation($"Sipariş başarıyla güncellendi: {entity.OrderNumber}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Sipariş güncellenirken hata oluştu: {entity.OrderNumber}");
                throw;
            }
        }

        public async Task<List<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _repository.GetAllAsync(o => 
                o.CreatedDate >= startDate && 
                o.CreatedDate <= endDate);
        }

        public async Task<List<Order>> GetOrdersByStatusAsync(string status)
        {
            return await _repository.GetAllAsync(o => o.Status == status);
        }

        public async Task<List<Order>> GetOrdersByCustomerAsync(string customerEmail)
        {
            return await _repository.GetAllAsync(o => o.Email == customerEmail || o.FullName.Contains(customerEmail));
        }

        public async Task<decimal> GetTotalSalesAsync(DateTime startDate, DateTime endDate)
        {
            var orders = await GetOrdersByDateRangeAsync(startDate, endDate);
            return orders.Sum(o => o.TotalAmount);
        }

        public async Task<List<OrderItem>> GetOrderItemsAsync(int orderId)
        {
            var order = await _repository.GetByIdAsync(orderId);
            return order?.OrderItems.ToList();
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string newStatus)
        {
            var order = await _repository.GetByIdAsync(orderId);
            if (order == null) return false;

            order.Status = newStatus;
            order.UpdatedDate = DateTime.Now;
            await _repository.UpdateAsync(order);
            return true;
        }

        public async Task<List<OrderListViewModel>> GetAllOrdersWithDetailsAsync()
        {
            try
            {
                _logger.LogInformation("Tüm siparişler detaylarıyla getiriliyor");
                var orders = await _repository.GetAllAsync();
                var orderViewModels = orders.Select(o => new OrderListViewModel
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    FullName = o.FullName,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status
                }).ToList();
                _logger.LogInformation($"Toplam {orderViewModels.Count} sipariş detayı getirildi");
                return orderViewModels;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş detayları getirilirken hata oluştu");
                throw;
            }
        }

        public async Task<Order> GetOrderByIdWithDetailsAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Order> UpdateOrderAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            var order = await GetByIdAsync(id);
            if (order == null) return false;

            await DeleteAsync(order);
            return true;
        }

        public async Task<int> GetTotalOrderCountAsync()
        {
            return await _context.Orders.CountAsync();
        }
    }
}