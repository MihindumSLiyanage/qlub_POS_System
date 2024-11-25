namespace POSIntegration.Models
{
    public class OrderItemData
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public List<OrderItemModifierResponse> Modifiers { get; set; } = new List<OrderItemModifierResponse>();
    }
}
