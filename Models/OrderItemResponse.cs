namespace POSIntegration.Models
{
    public class OrderItemResponse
    {
        public string Name {  get; set; }
        public string Comment { get; set; }
        public double UnitPrice { get; set; }
        public double Quantity { get; set; }
        public double Amount { get; set; }
        public List<OrderItemDiscountResponse> Discounts { get; set; }
        public List<OrderItemModifierResponse> Modifiers { get; set; }
    }
}
