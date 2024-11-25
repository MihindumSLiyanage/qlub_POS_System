using POSIntegration.Models;
using Square.Authentication;
using Square.Exceptions;
using Square.Models;
using Square;

namespace POSIntegration.Services
{
    // Service for handling payments via Square API.
    public class PaymentService
    {
        private readonly SquareClient _squareClient;
        private readonly string _locationId;

        // Constructor to initialize SquareClient with configuration settings.
        public PaymentService(Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            var accessToken = configuration["Square:AccessToken"];
            _locationId = configuration["Square:LocationId"];

            _squareClient = new SquareClient.Builder()
                .Environment(Square.Environment.Sandbox) // Use the sandbox environment for testing.
                .BearerAuthCredentials(new BearerAuthModel.Builder(accessToken).Build())
                .Build();
        }

        // Method to create a payment via Square API.
        public async Task<PaymentResponse> CreatePayment(PaymentData paymentData)
        {
            try
            {
                string sourceId = "CASH"; // Indicates payment source as cash.

                var paymentRequest = new CreatePaymentRequest.Builder(sourceId, paymentData.OrderId)
                    .AmountMoney(new Money.Builder()
                        .Amount((long)(paymentData.BillAmount * 100)) // Convert amount to cents.
                        .Currency("USD")
                        .Build())
                    .TipMoney(new Money.Builder()
                        .Amount((long)(paymentData.TipAmount * 100)) // Convert tip amount to cents.
                        .Currency("USD")
                        .Build())
                    .CashDetails(new CashPaymentDetails(new Money.Builder()
                        .Amount(100000000000000) // Example for a very large cash payment.
                        .Currency("USD")
                        .Build()))
                    .LocationId(_locationId)
                    .IdempotencyKey(Guid.NewGuid().ToString()) // Ensure uniqueness of the request.
                    .Build();

                var response = await _squareClient.PaymentsApi.CreatePaymentAsync(paymentRequest); // API call to create payment.

                return new PaymentResponse
                {
                    OrderId = paymentData.OrderId,
                    BillAmount = paymentData.BillAmount,
                    TipAmount = paymentData.TipAmount,
                    SourceId = sourceId
                };
            }
            catch (ApiException apiEx)
            {
                throw new Exception($"Error creating payment: {apiEx.Message}");
            }
        }

        // Helper method to build the payment request object.
        public CreatePaymentRequest BuildPaymentRequest(PaymentData paymentData)
        {
            string sourceId = "CASH";

            return new CreatePaymentRequest.Builder(sourceId, paymentData.OrderId)
                .AmountMoney(new Money.Builder()
                    .Amount((long)(paymentData.BillAmount * 100))
                    .Currency("USD")
                    .Build())
                .TipMoney(new Money.Builder()
                    .Amount((long)(paymentData.TipAmount * 100))
                    .Currency("USD")
                    .Build())
                .LocationId(_locationId)
                .IdempotencyKey(Guid.NewGuid().ToString())
                .Build();
        }
    }
}
