
using POSIntegration.Data;
using POSIntegration.Models.Enitities;
using POSIntegration.Repositories.Interfaces;

namespace POSIntegration.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _context; 

        public PaymentRepository(AppDbContext context)
        {
            _context = context;
        }

        // Method to submit a payment and save it to the database
        public async Task<Payment> SubmitPaymentAsync(Payment payment)
        {
            _context.Payments.Add(payment); 
            await _context.SaveChangesAsync(); 
            return payment; 
        }
    }

}
