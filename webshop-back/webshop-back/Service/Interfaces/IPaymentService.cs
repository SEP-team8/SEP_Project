using webshop_back.Data.Models;

namespace webshop_back.Service.Interfaces
{
    public interface IPaymentService
    {
        // Initialize payment with acquirer (calls PSP, returns payment url / qr payload)
        Task<PaymentInitResponse> InitializePaymentToAcquirerAsync(PaymentInitRequest req, Order order);
    }
}
