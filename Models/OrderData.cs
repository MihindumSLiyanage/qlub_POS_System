namespace POSIntegration.Models
{
    public class OrderData
    {
        public string TableNumber { get; set; }
        public string ResturantId { get; set; }
        public List<OrderItemData> Items { get; set; }
    }
}
