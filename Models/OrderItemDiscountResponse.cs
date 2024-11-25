namespace POSIntegration.Models
{
    public class OrderItemDiscountResponse
    {
        public string Name { get; set; }
        public bool IsPercentage {  get; set; }
        public double Value { get; set; }
        public double Amount {  get; set; }
    }
}
