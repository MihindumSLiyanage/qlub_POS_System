using Microsoft.EntityFrameworkCore;
using POSIntegration.Data;
using POSIntegration.Models.Enitities;
using POSIntegration.Repositories.Interfaces;

namespace POSIntegration.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        // Constructor to initialize the database context.
        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        // Adds a new order to the database and saves changes.
        public async Task<Order> AddOrderAsync(Order order)
        {
            await _context.Orders.AddAsync(order); // Add order entity.
            await _context.SaveChangesAsync(); // Persist changes.
            return order;
        }

        // Retrieves an order from the database by its ID.
        public async Task<Order> GetOrderByIdAsync(string orderId)
        {
            return await _context.Orders.FindAsync(orderId); // Search for order by primary key.
        }

        // Retrieves all orders associated with a specific table number.
        public async Task<IEnumerable<Order>> GetOrdersByTableAsync(string tableNumber)
        {
            return await _context.Orders
                .Where(o => o.TableNumber == tableNumber) // Filter by table number.
                .ToListAsync();
        }
    }
}
