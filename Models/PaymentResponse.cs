namespace POSIntegration.Models
{
    public class PaymentResponse
    {
        public string OrderId { get; set; }
        public double BillAmount { get; set; }
        public double TipAmount { get; set; }
        public string SourceId { get; set; }
    }
}
