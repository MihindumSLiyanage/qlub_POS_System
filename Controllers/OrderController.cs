using Microsoft.AspNetCore.Mvc;
using POSIntegration.Models;
using POSIntegration.Models.Enitities;
using POSIntegration.Repositories.Interfaces;
using POSIntegration.Services;

namespace POSIntegration.Controllers
{
    [Route("api/order")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly OrderService _orderService;

        // Constructor to initialize repository and service dependencies.
        public OrderController(IOrderRepository orderRepository, OrderService orderService)
        {
            _orderRepository = orderRepository;
            _orderService = orderService;
        }

        // Creates a new order, saves it to the database, and returns the response.
        [HttpPost]
        public async Task<IActionResult> CreateOrderAsync([FromBody] OrderData orderData)
        {
            var squareOrderResponse = await _orderService.CreateOrder(orderData); // Create order via Square API.

            var order = new Order
            {
                Id = squareOrderResponse.Id,
                TableNumber = orderData.TableNumber,
                Resturant = orderData.ResturantId,
                CreatedAt = DateTime.UtcNow,
                OrderItems = orderData.Items.Select(item => new OrderItem
                {
                    Name = item.Name,
                    Quantity = item.Quantity,
                    Price = (double)item.Price // Convert to double for database storage.
                }).ToList()
            };

            var createdOrder = await _orderRepository.AddOrderAsync(order); // Save order to the database.

            var orderResponse = new OrderResponse
            {
                Id = createdOrder.Id,
                OpenedAt = createdOrder.CreatedAt,
                IsClosed = false,
                TableNumber = createdOrder.TableNumber,
                ResturantId = createdOrder.Resturant,
                Items = createdOrder.OrderItems.Select(item => new OrderItemResponse
                {
                    Name = item.Name,
                    Quantity = item.Quantity,
                    UnitPrice = (double)item.Price
                }).ToList(),
                Totals = squareOrderResponse.Totals // Include totals from Square API.
            };

            return Ok(orderResponse); // Return response.
        }

        // Retrieves an order by its ID from the database and Square API.
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderByIdAsync(string orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return NotFound();
            }

            var orderFromSquare = await _orderService.GetOrderById(orderId);
            return Ok(orderFromSquare);
        }

        // Retrieves all orders for a specific table number from the database and Square API.
        [HttpGet("table/{tableNumber}")]
        public async Task<IActionResult> GetOrdersByTableAsync(string tableNumber)
        {
            var orders = await _orderRepository.GetOrdersByTableAsync(tableNumber);
            if (orders == null || !orders.Any())
            {
                return NotFound();
            }

            var orderFromSquare = await _orderService.GetOrdersByTable(tableNumber);
            return Ok(orderFromSquare);
        }
    }
}