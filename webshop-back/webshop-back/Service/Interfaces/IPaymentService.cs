using webshop_back.Data.Models;

namespace webshop_back.Service.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentInitResponse> InitializePaymentToAcquirerAsync(PaymentInitRequest req, Order order);
    }
}
