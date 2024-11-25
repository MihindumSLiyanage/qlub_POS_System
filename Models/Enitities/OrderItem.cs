namespace POSIntegration.Models.Enitities
{
    public class OrderItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public List<OrderItemModifierResponse> Modifiers { get; set; } = new List<OrderItemModifierResponse>();
    }
}
