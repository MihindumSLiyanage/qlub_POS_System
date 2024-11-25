using POSIntegration.Models;
using Square.Authentication;
using Square.Exceptions;
using Square.Models;
using Square;

namespace POSIntegration.Services
{
    public class OrderService
    {
        private readonly SquareClient _squareClient;
        private readonly string _locationId;

        // Constructor to initialize SquareClient using configuration settings.
        public OrderService(Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            var accessToken = configuration["Square:AccessToken"];
            _locationId = configuration["Square:LocationId"];

            _squareClient = new SquareClient.Builder()
                .Environment(Square.Environment.Sandbox) // Sandbox environment for testing.
                .BearerAuthCredentials(new BearerAuthModel.Builder(accessToken).Build())
                .Build();
        }

        // Method to create an order using Square API.
        public async Task<OrderResponse> CreateOrder(OrderData orderData)
        {
            try
            {
                var order = BuildOrder(orderData); // Build order object.
                var createOrderRequest = new CreateOrderRequest.Builder()
                    .Order(order)
                    .IdempotencyKey(Guid.NewGuid().ToString()) // Ensure request uniqueness.
                    .Build();

                var response = await _squareClient.OrdersApi.CreateOrderAsync(createOrderRequest); // Call API to create order.
                return ConvertToOrderResponse(response); // Convert API response to internal model.
            }
            catch (ApiException apiEx)
            {
                throw new Exception($"Error creating order: {apiEx.Message}");
            }
        }

        // Method to retrieve an order by its ID from Square API.
        public async Task<OrderResponse> GetOrderById(string orderId)
        {
            try
            {
                var retrieveOrderResponse = await _squareClient.OrdersApi.RetrieveOrderAsync(orderId);
                return MapOrderToOrderResponse(retrieveOrderResponse.Order ?? throw new Exception($"Order with ID {orderId} was not found."));
            }
            catch (ApiException apiEx)
            {
                throw new Exception($"Error retrieving order with ID {orderId}: {apiEx.Message}");
            }
        }

        // Method to fetch orders associated with a specific table number.
        public async Task<List<OrderResponse>> GetOrdersByTable(string tableNumber)
        {
            try
            {
                var searchRequest = new SearchOrdersRequest.Builder()
                    .LocationIds(new List<string> { _locationId }) // Restrict search to a specific location.
                    .Build();

                var searchResponse = await _squareClient.OrdersApi.SearchOrdersAsync(searchRequest);
                return searchResponse.Orders?
                    .Where(order => order.ReferenceId == tableNumber) // Filter by table number.
                    .Select(MapOrderToOrderResponse)
                    .ToList() ?? new List<OrderResponse>();
            }
            catch (ApiException apiEx)
            {
                throw new Exception($"Error retrieving orders for table {tableNumber}: {apiEx.Message}");
            }
        }

        // Helper method to build a Square order object.
        private Order BuildOrder(OrderData orderData)
        {
            return new Order.Builder(_locationId)
                .ReferenceId(orderData.TableNumber) // Assign table number as reference.
                .LineItems(MapOrderItems(orderData.Items)) // Map order items.
                .Build();
        }

        // Maps order item data to Square order line items.
        private List<OrderLineItem> MapOrderItems(List<OrderItemData> items)
        {
            return items.Select(item =>
            {
                // Create the base order line item
                var orderLineItemBuilder = new OrderLineItem.Builder(item.Quantity.ToString())
                    .Name(item.Name) // Set item name.
                    .BasePriceMoney(new Money.Builder()
                        .Amount((long)(item.Price * 100)) // Convert price to smallest currency unit.
                        .Currency("USD")
                        .Build());
                
                List<OrderLineItemModifier> modifiers = new List<OrderLineItemModifier>();

                // Add modifiers to the list if they exist
                if (item.Modifiers != null && item.Modifiers.Any())
                {
                    foreach (var modifier in item.Modifiers)
                    {
                        var modifierMoney = new Money.Builder()
                            .Amount((long)(modifier.UnitPrice * 100)) // Convert modifier price to smallest currency unit
                            .Currency("USD")
                            .Build();

                        var lineItemModifier = new OrderLineItemModifier.Builder()
                            .Name(modifier.Name)
                            .BasePriceMoney(modifierMoney)
                            .Quantity(modifier.Quantity.ToString())
                            .Build();

                        modifiers.Add(lineItemModifier);
                    }
                }

                // Build the final OrderLineItem with discounts and modifiers
                return orderLineItemBuilder
                    .Modifiers(modifiers)  
                    .Build();
            }).ToList();
        }

        // Converts Square API response to internal order response model.
        private OrderResponse ConvertToOrderResponse(CreateOrderResponse response)
        {
            var order = response.Order;
            return MapOrderToOrderResponse(order);
        }

        // Maps Square order object to internal order response model.
        private OrderResponse MapOrderToOrderResponse(Order order)
        {
            return new OrderResponse
            {
                Id = order.Id,
                OpenedAt = DateTime.Parse(order.CreatedAt),
                IsClosed = order.State == "COMPLETED",
                TableNumber = order.ReferenceId,
                ResturantId = _locationId,
                Items = MapOrderItemsResponse(order.LineItems), // Map order items.
                Totals = MapOrderTotalsResponse(order) // Map order totals.
            };
        }

        // Maps Square line items to internal response model.
        private List<OrderItemResponse> MapOrderItemsResponse(IList<OrderLineItem> items)
        {
            return items?.Select(item => new OrderItemResponse
            {
                Name = item.Name,
                Quantity = Convert.ToDouble(item.Quantity), // Convert quantity.
                UnitPrice = Convert.ToDouble(item.BasePriceMoney?.Amount ?? 0) / 100, // Convert price.
                Amount = Convert.ToDouble(item.TotalMoney?.Amount ?? 0) / 100,
                Discounts = MapItemDiscounts(item), // Map discounts.
                Modifiers = MapItemModifiers(item) // Map modifiers.
            }).ToList() ?? new List<OrderItemResponse>();
        }

        // Maps applied discounts for a line item.
        private List<OrderItemDiscountResponse> MapItemDiscounts(OrderLineItem item)
        {
            return item.AppliedDiscounts?.Select(discount => new OrderItemDiscountResponse
            {
                Name = discount.DiscountUid,
                Amount = Convert.ToDouble(discount.AppliedMoney?.Amount ?? 0) / 100,
                IsPercentage = false
            }).ToList() ?? new List<OrderItemDiscountResponse>();
        }

        // Maps applied modifiers for a line item.
        private List<OrderItemModifierResponse> MapItemModifiers(OrderLineItem item)
        {
            return item.Modifiers?.Select(modifier => new OrderItemModifierResponse
            {
                Name = modifier.Name,
                Quantity = Convert.ToDouble(modifier.Quantity),
                UnitPrice = Convert.ToDouble(modifier.BasePriceMoney?.Amount ?? 0) / 100,
                Amount = Convert.ToDouble(modifier.TotalPriceMoney?.Amount ?? 0) / 100
            }).ToList() ?? new List<OrderItemModifierResponse>();
        }

        // Maps order totals to internal response model.
        private OrderTotalResponse MapOrderTotalsResponse(Order order)
        {
            return new OrderTotalResponse
            {
                Discounts = Convert.ToDouble(order.TotalDiscountMoney?.Amount ?? 0) / 100,
                Due = Convert.ToDouble(order.NetAmountDueMoney?.Amount ?? 0) / 100,
                Tax = Convert.ToDouble(order.TotalTaxMoney?.Amount ?? 0) / 100,
                ServiceCharge = Convert.ToDouble(order.TotalServiceChargeMoney?.Amount ?? 0) / 100,
                Tips = Convert.ToDouble(order.TotalTipMoney?.Amount ?? 0) / 100,
                Paid = order.Tenders?.Sum(tender => Convert.ToDouble(tender.AmountMoney?.Amount ?? 0)) / 100 ?? 0,
                Total = Convert.ToDouble(order.TotalMoney?.Amount ?? 0) / 100
            };
        }
    }
}
