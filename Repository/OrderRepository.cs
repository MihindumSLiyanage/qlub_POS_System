using Microsoft.EntityFrameworkCore;
using POSIntegration.Data;
using POSIntegration.Models.Enitities;
using POSIntegration.Repositories.Interfaces;

namespace POSIntegration.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Order> AddOrderAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<Order> GetOrderByIdAsync(string orderId)
        {
            return await _context.Orders.FindAsync(orderId);
        }

        public async Task<IEnumerable<Order>> GetOrdersByTableAsync(string tableNumber)
        {
            return await _context.Orders.Where(o => o.TableNumber == tableNumber  ).ToListAsync();
        }

    }
}
