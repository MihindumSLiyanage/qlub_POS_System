using POSIntegration.Models;
using Square.Authentication;
using Square.Exceptions;
using Square.Models;
using Square;

namespace POSIntegration.Services
{
    public class PaymentService
    {
        private readonly SquareClient _squareClient;
        private readonly string _locationId;

        public PaymentService(Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            var accessToken = configuration["Square:AccessToken"];
            _locationId = configuration["Square:LocationId"];

            _squareClient = new SquareClient.Builder()
                .Environment(Square.Environment.Sandbox)
                .BearerAuthCredentials(new BearerAuthModel.Builder(accessToken).Build())
                .Build();
        }

        public async Task<PaymentResponse> CreatePayment(PaymentData paymentData)
        {
            try
            {
                string sourceId = "CASH";

                var paymentRequest = new CreatePaymentRequest.Builder(sourceId, paymentData.OrderId)
                    .AmountMoney(new Money.Builder()
                        .Amount((long)(paymentData.BillAmount * 100))
                        .Currency("USD")
                        .Build())
                    .TipMoney(new Money.Builder()
                        .Amount((long)(paymentData.TipAmount * 100))
                        .Currency("USD")
                        .Build())
                    .CashDetails(new CashPaymentDetails(new Money.Builder()
                        .Amount((100000000000000))
                        .Currency("USD")
                        .Build()))
                    .LocationId(_locationId)
                    .IdempotencyKey(Guid.NewGuid().ToString())
                    .Build();

                var response = await _squareClient.PaymentsApi.CreatePaymentAsync(paymentRequest);

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
