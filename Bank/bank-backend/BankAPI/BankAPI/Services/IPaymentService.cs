using BankAPI.DTOs;
using BankAPI.Models;

namespace BankAPI.Services
{
    public interface IPaymentService
    {
        Task<PaymentExecutionResult> ExecuteCardPayment(Guid paymentRequestId, CardPaymentRequest request);

        Task<PaymentRequestDto> GetPaymentRequest(Guid paymentRequestId);

        Task<InitializePaymentServiceResult> InitializePayment(InitPaymentRequestDto dto, Guid pspId, string signature, DateTime timestamp);
    }
}
