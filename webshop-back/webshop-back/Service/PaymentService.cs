using System.Text;
using System.Text.Json;
using webshop_back.Data.Models;
using webshop_back.Service.Interfaces;

namespace webshop_back.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IRepository _repo;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _env;

        public PaymentService(
            IHttpClientFactory httpClientFactory,
            IRepository repo,
            IConfiguration config,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment env)
        {
            _httpClientFactory = httpClientFactory;
            _repo = repo;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
            _env = env;
        }

        public async Task<PaymentInitResponse> InitializePaymentToAcquirerAsync(PaymentInitRequest req, Order order)
        {
            var merchant = _repo.GetMerchantByMerchantId(req.MerchantId);
            if (merchant == null)
                throw new InvalidOperationException($"Merchant {req.MerchantId} not found.");

            var merchantTimestamp = DateTime.UtcNow.ToString("o");

            var pspRequest = new
            {
                MerchantId = merchant.PspMerchantId,
                MerchantPassword = merchant.PspMerchantSecret,
                Amount = req.Amount,
                Currency = req.Currency,
                MerchantOrderId = req.MerchantOrderId,
                MerchantTimestamp = merchantTimestamp,
                SuccessUrl = req.SuccessUrl,
                FailedUrl = req.FailedUrl,
                ErrorUrl = req.ErrorUrl
            };

            var json = JsonSerializer.Serialize(pspRequest, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            // ==== LOG request + merchant secret ====
            LogToFileAndConsole("PSP REQUEST", $"{json}\n");

            var client = _httpClientFactory.CreateClient("psp-client");

            // SPEC: MERCHANT_PASSWORD = PSP secret
            client.DefaultRequestHeaders.Add("X-Merchant-Password", merchant.PspMerchantSecret);

            var response = await client.PostAsync(
                "/api/payments/init",
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            var respBody = await response.Content.ReadAsStringAsync();
            LogToFileAndConsole("PSP RESPONSE", respBody);

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(respBody);

            using var doc = JsonDocument.Parse(respBody);
            var root = doc.RootElement;


            // Menjamo status: Pending (dok PSP ne izvrši callback) ili Success/Failed
            order.Status = OrderStatus.Pending;
            order.UpdatedAt = DateTime.UtcNow;

            _repo.UpdateOrder(order);

            return new PaymentInitResponse
            {
                Amount = req.Amount,
                Currency = req.Currency
            };
        }

        private void LogToFileAndConsole(string title, string content)
        {
            var log = $"""
                ===== {title} =====
                {DateTime.UtcNow:O}
                {content}

                """;

            Console.WriteLine(log);

            var path = Path.Combine(AppContext.BaseDirectory, "Logs", "psp.log");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.AppendAllText(path, log);
        }

        private string GetBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null) return _config["App:PublicBaseUrl"] ?? "";
            return $"{request.Scheme}://{request.Host}";
        }
    }
}
