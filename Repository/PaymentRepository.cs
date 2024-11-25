
using POSIntegration.Data;
using POSIntegration.Models.Enitities;
using POSIntegration.Repositories.Interfaces;

namespace POSIntegration.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _context;

        // Constructor to initialize the database context.
        public PaymentRepository(AppDbContext context)
        {
            _context = context;
        }

        // Submits a payment by adding it to the database and saving changes.
        public async Task<Payment> SubmitPaymentAsync(Payment payment)
        {
            _context.Payments.Add(payment); // Add payment entity.
            await _context.SaveChangesAsync(); // Persist changes.
            return payment;
        }
    }
}
