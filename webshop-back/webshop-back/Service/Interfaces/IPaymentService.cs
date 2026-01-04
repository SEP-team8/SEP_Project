using webshop_back.Data.Models;

namespace webshop_back.Service.Interfaces
{
    public interface IPaymentService
    {
        // Initialize payment with acquirer (calls PSP, returns payment url / qr payload)
        PaymentInitResponse InitializePaymentToAcquirer(PaymentInitRequest req, Order order);
    }
}
