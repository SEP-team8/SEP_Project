namespace webshop_back.Service
{
    public class PspStubService
    {
        private readonly IConfiguration _config;
        private readonly Random _rnd = new Random();

        public PspStubService(IConfiguration config)
        {
            _config = config;
        }

        public (string paymentId, string paymentUrl, string stan, string pspTimestamp) InitiatePayment(string baseUrl, decimal amount, string currency, string merchantOrderId)
        {
            var paymentId = "PAY-" + Guid.NewGuid().ToString();
            var stan = _rnd.Next(0, 99999999).ToString("D8");
            var pspTimestamp = DateTime.UtcNow.ToString("o");
            // paymentUrl: PSP mock page on backend that simulates bank UI
            var paymentUrl = $"{baseUrl}/psp/mock-pay?payment_id={Uri.EscapeDataString(paymentId)}";
            return (paymentId, paymentUrl, stan, pspTimestamp);
        }
    }
}
