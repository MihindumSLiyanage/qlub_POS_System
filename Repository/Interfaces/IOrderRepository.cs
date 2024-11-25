using POSIntegration.Models.Enitities;

namespace POSIntegration.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order> AddOrderAsync(Order order);
        Task<Order> GetOrderByIdAsync(string orderId);
        Task<IEnumerable<Order>> GetOrdersByTableAsync(string tableNumber);
    }
}
