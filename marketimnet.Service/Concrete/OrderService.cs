using marketimnet.Core.Entities;
using marketimnet.Data;
using marketimnet.Data.Abstract;
using marketimnet.Service.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace marketimnet.Service.Concrete
{
    public class OrderService : IOrderService
    {
        private readonly IRepository<Order> _repository;
        private readonly DatabaseContext _context;

        public OrderService(IRepository<Order> repository, DatabaseContext context)
        {
            _repository = repository;
            _context = context;
        }

        public async Task<Order> AddAsync(Order entity)
        {
            return await _repository.AddAsync(entity);
        }

        public async Task<bool> AnyAsync(Expression<Func<Order, bool>> expression)
        {
            return await _repository.AnyAsync(expression);
        }

        public async Task<int> DeleteAsync(Order entity)
        {
            return await _repository.DeleteAsync(entity);
        }

        public async Task<List<Order>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<List<Order>> GetAllAsync(Expression<Func<Order, bool>> expression)
        {
            return await _repository.GetAllAsync(expression);
        }

        public async Task<Order> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Order> GetByOrderNumberAsync(string orderNumber)
        {
            return await _repository.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        }

        public async Task<Order> UpdateAsync(Order entity)
        {
            return await _repository.UpdateAsync(entity);
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
            return await _repository.GetAllAsync(o => o.Email == customerEmail);
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

        public async Task<List<Order>> GetAllOrdersWithDetailsAsync()
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();
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