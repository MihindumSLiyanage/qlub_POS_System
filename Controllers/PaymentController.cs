using Microsoft.AspNetCore.Mvc;
using POSIntegration.Models;
using POSIntegration.Models.Enitities;
using POSIntegration.Repositories.Interfaces;
using POSIntegration.Services;

namespace POSIntegration.Controllers
{
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly PaymentService _paymentService;

        // Constructor to initialize repository and service dependencies.
        public PaymentController(IPaymentRepository paymentRepository, PaymentService paymentService)
        {
            _paymentRepository = paymentRepository;
            _paymentService = paymentService;
        }

        // Submits a payment, saves it to the database, and returns the response.
        [HttpPost]
        public async Task<IActionResult> SubmitPaymentAsync([FromBody] PaymentData paymentData)
        {
            var squarePaymentResponse = await _paymentService.CreatePayment(paymentData); // Create payment via Square API.

            var payment = new Payment
            {
                Id = Guid.NewGuid().ToString(), // Generate unique ID for payment.
                OrderId = paymentData.OrderId,
                BillAmount = paymentData.BillAmount,
                TipAmount = paymentData.TipAmount,
            };

            var createdPayment = await _paymentRepository.SubmitPaymentAsync(payment); // Save payment to the database.

            var paymentResponse = new PaymentResponse
            {
                OrderId = createdPayment.OrderId,
                BillAmount = createdPayment.BillAmount,
                TipAmount = createdPayment.TipAmount,
            };

            return Ok(paymentResponse); // Return response.
        }
    }
}
