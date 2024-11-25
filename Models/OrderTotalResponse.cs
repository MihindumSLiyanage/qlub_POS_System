namespace POSIntegration.Models
{
    public class OrderTotalResponse
    {
        public double Discounts {  get; set; }
        public double Due { get; set; }
        public double Tax { get; set; }
        public double ServiceCharge { get; set; }
        public double Paid { get; set; }
        public double Tips { get; set; }
        public double Total { get; set; }
    }
}
