namespace POSIntegration.Models
{
    public class OrderItemModifierResponse
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name {  get; set; }
        public double UnitPrice{get; set; }
        public double Quantity {  get; set; }
        public double Amount {  get; set; }
    }
}
