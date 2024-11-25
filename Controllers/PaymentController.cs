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

        public PaymentController(IPaymentRepository paymentRepository, PaymentService paymentService)
        {
            _paymentRepository = paymentRepository;
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitPaymentAsync([FromBody] PaymentData paymentData)
        {
            var squarePaymentResponse = await _paymentService.CreatePayment(paymentData);

            var payment = new Payment
            {
                Id = Guid.NewGuid().ToString(),
                OrderId = paymentData.OrderId,
                BillAmount = paymentData.BillAmount,
                TipAmount = paymentData.TipAmount,
            };

            var createdPayment = await _paymentRepository.SubmitPaymentAsync(payment);

            var paymentResponse = new PaymentResponse
            {
                OrderId = createdPayment.OrderId,
                BillAmount = createdPayment.BillAmount,
                TipAmount = createdPayment.TipAmount,
            };

            return Ok(paymentResponse);
        }
    }
}
