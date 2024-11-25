using POSIntegration.Models.Enitities;

namespace POSIntegration.Repositories.Interfaces
{
    public interface IPaymentRepository
    {
        Task<Payment> SubmitPaymentAsync(Payment payment);
    }
}
