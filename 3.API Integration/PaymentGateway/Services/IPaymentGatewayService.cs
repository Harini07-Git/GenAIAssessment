using System.Threading.Tasks;
using PaymentGateway.Models;

namespace PaymentGateway.Services
{
    public interface IPaymentGatewayService
    {
        Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request);
        Task<RefundResponse> ProcessRefundAsync(RefundRequest request);
        Task<PaymentStatusResponse> GetPaymentStatusAsync(string transactionId);
    }
}
