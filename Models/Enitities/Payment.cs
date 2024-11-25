namespace POSIntegration.Models.Enitities
{
    public class Payment
    {
        public string Id { get; set; }
        public string OrderId { get; set; }
        public double BillAmount { get; set; }
        public double TipAmount { get; set; }
    }
}
