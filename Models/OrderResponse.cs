namespace POSIntegration.Models
{
    public class OrderResponse
    {
        public string Id { get; set; }
        public DateTime OpenedAt { get; set; }
        public bool IsClosed { get; set; }
        public string TableNumber { get; set; }
        public string ResturantId { get; set; }
        public List<OrderItemResponse> Items { get; set; }
        public OrderTotalResponse Totals { get; set; }
    }
}
