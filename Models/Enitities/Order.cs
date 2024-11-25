namespace POSIntegration.Models.Enitities
{
    public class Order
    {
        public string Id { get; set; }
        public string TableNumber { get; set; }
        public string Resturant { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
