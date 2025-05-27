using marketimnet.Core.Entities;
using marketimnet.Core.ViewModels;
using System.Linq.Expressions;

namespace marketimnet.Service.Abstract
{
    public interface IOrderService
    {
        Task<Order> AddAsync(Order entity);
        Task<Order> UpdateAsync(Order entity);
        Task<bool> DeleteAsync(Order entity);
        Task<Order> GetByIdAsync(int id);
        Task<List<Order>> GetAllAsync();
        Task<List<Order>> GetAllAsync(Expression<Func<Order, bool>> expression);
        Task<bool> AnyAsync(Expression<Func<Order, bool>> expression);
        Task<Order> GetByOrderNumberAsync(string orderNumber);
        Task<List<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<Order>> GetOrdersByStatusAsync(string status);
        Task<List<Order>> GetOrdersByCustomerAsync(string customerEmail);
        Task<decimal> GetTotalSalesAsync(DateTime startDate, DateTime endDate);
        Task<List<OrderItem>> GetOrderItemsAsync(int orderId);
        Task<bool> UpdateOrderStatusAsync(int orderId, string newStatus);
        Task<Order> GetOrderByIdWithDetailsAsync(int id);
        Task<Order> UpdateOrderAsync(Order order);
        Task<bool> DeleteOrderAsync(int id);
        Task<int> GetTotalOrderCountAsync();
        Task<List<OrderListViewModel>> GetAllOrdersWithDetailsAsync();
    }
} 