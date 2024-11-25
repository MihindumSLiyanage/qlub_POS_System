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

        public OrderService(Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            var accessToken = configuration["Square:AccessToken"];
            _locationId = configuration["Square:LocationId"];

            _squareClient = new SquareClient.Builder()
                .Environment(Square.Environment.Sandbox)
                .BearerAuthCredentials(new BearerAuthModel.Builder(accessToken).Build())
                .Build();
        }

        public async Task<OrderResponse> CreateOrder(OrderData orderData)
        {
            try
            {
                var order = BuildOrder(orderData);
                var createOrderRequest = new CreateOrderRequest.Builder()
                    .Order(order)
                    .IdempotencyKey(Guid.NewGuid().ToString())
                    .Build();

                var response = await _squareClient.OrdersApi.CreateOrderAsync(createOrderRequest);
                return ConvertToOrderResponse(response);
            }
            catch (ApiException apiEx)
            {
                throw new Exception($"Error creating order: {apiEx.Message}");
            }
        }

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

        public async Task<List<OrderResponse>> GetOrdersByTable(string tableNumber)
        {
            try
            {
                var searchRequest = new SearchOrdersRequest.Builder()
                    .LocationIds(new List<string> { _locationId })
                    .Build();

                var searchResponse = await _squareClient.OrdersApi.SearchOrdersAsync(searchRequest);
                return searchResponse.Orders?
                    .Where(order => order.ReferenceId == tableNumber)
                    .Select(MapOrderToOrderResponse)
                    .ToList() ?? new List<OrderResponse>();
            }
            catch (ApiException apiEx)
            {
                throw new Exception($"Error retrieving orders for table {tableNumber}: {apiEx.Message}");
            }
        }

        private Order BuildOrder(OrderData orderData)
        {
            return new Order.Builder(_locationId)
                .ReferenceId(orderData.TableNumber)
                .LineItems(MapOrderItems(orderData.Items))
                .Build();
        }

        private List<OrderLineItem> MapOrderItems(List<OrderItemData> items)
        {
            return items.Select(item => new OrderLineItem.Builder(item.Quantity.ToString())
                .Name(item.Name)
                .BasePriceMoney(new Money.Builder()
                    .Amount((long)(item.Price * 100))
                    .Currency("USD")
                    .Build())
                .Build()).ToList();
        }

        private OrderResponse ConvertToOrderResponse(CreateOrderResponse response)
        {
            var order = response.Order;
            return MapOrderToOrderResponse(order);
        }

        private OrderResponse MapOrderToOrderResponse(Square.Models.Order order)
        {
            return new OrderResponse
            {
                Id = order.Id,
                OpenedAt = DateTime.Parse(order.CreatedAt),
                IsClosed = order.State == "COMPLETED",
                TableNumber = order.ReferenceId,
                ResturantId = _locationId,
                Items = MapOrderItemsResponse(order.LineItems),
                Totals = MapOrderTotalsResponse(order)
            };
        }

        private List<OrderItemResponse> MapOrderItemsResponse(IList<OrderLineItem> items)
        {
            return items?.Select(item => new OrderItemResponse
            {
                Name = item.Name,
                Quantity = Convert.ToDouble(item.Quantity),
                UnitPrice = Convert.ToDouble(item.BasePriceMoney?.Amount ?? 0) / 100,
                Amount = Convert.ToDouble(item.TotalMoney?.Amount ?? 0) / 100,
                Discounts = MapItemDiscounts(item),
                Modifiers = MapItemModifiers(item)
            }).ToList() ?? new List<OrderItemResponse>();
        }

        private List<OrderItemDiscountResponse> MapItemDiscounts(OrderLineItem item)
        {
            return item.AppliedDiscounts?.Select(discount => new OrderItemDiscountResponse
            {
                Name = discount.DiscountUid,
                Amount = Convert.ToDouble(discount.AppliedMoney?.Amount ?? 0) / 100,
                IsPercentage = false
            }).ToList() ?? new List<OrderItemDiscountResponse>();
        }

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

        private OrderTotalResponse MapOrderTotalsResponse(Square.Models.Order order)
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
