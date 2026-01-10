using BankAPI.DTOs;
using BankAPI.Models;

namespace BankAPI.Services
{
    public interface IPaymentService
    {
        Task<string> ExecuteCardPayment(Guid paymentRequestId, CardPaymentRequest request);

        Task<PaymentRequestDto> GetPaymentRequest(Guid paymentRequestId);

        Task<InitializePaymentServiceResult> InitializePayment(InitPaymentRequestDto dto, Guid pspId, string signature, DateTime timestamp, bool isQrPayment);

        Task<QRPaymentResponseDto> GenerateQrPayment(Guid paymentRequestId);
    }
}
