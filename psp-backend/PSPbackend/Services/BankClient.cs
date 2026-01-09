using PSPbackend.DTOs.Bank;
using PSPbackend.Helpers;
using PSPbackend.Models;

namespace PSPbackend.Services
{
    public class BankClient: IBankClient
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _config;

        public BankClient(IHttpClientFactory httpFactory, IConfiguration config)
        {
            _httpFactory = httpFactory;
            _config = config;
        }

        public async Task<InitPaymentResponseDto> InitAsync(InitPaymentRequestDto dto, PaymentMethod paymentMethod, CancellationToken ct)
        {
            var bankApiBaseUrl = _config["Bank:ApiBaseUrl"]!;
            //var bankFrontBaseUrl = _config["Bank:FrontBaseUrl"];
            var pspId = Guid.Parse(_config["Psp:PspId"]!);
            var secret = _config["Psp:SharedSecret"]!;

            var timestampUtc = DateTime.UtcNow;

            var payload = SignatureHelper.BuildPayload(dto, timestampUtc);
            var signature = SignatureHelper.CreateSignature(payload, secret);

            var client = _httpFactory.CreateClient();
            client.BaseAddress = new Uri(bankApiBaseUrl);

            using var req = new HttpRequestMessage(HttpMethod.Post, "/api/payments/init");
            req.Content = JsonContent.Create(dto);

            var isQrPayment = paymentMethod.Equals(PaymentMethod.QrCode);

            req.Headers.Add("PspID", pspId.ToString());
            req.Headers.Add("Signature", signature);
            req.Headers.Add("Timestamp", timestampUtc.ToString("O"));
            req.Headers.Add("IsQrPayment", isQrPayment.ToString().ToLowerInvariant());

            // Send Request to Bank
            var res = await client.SendAsync(req, ct);

            if (!res.IsSuccessStatusCode)
            {
                var err = await res.Content.ReadAsStringAsync(ct);
                throw new InvalidOperationException($"Bank init failed: {(int)res.StatusCode} {err}");
            }

            var bankResponse = await res.Content.ReadFromJsonAsync<InitPaymentResponseDto>(cancellationToken: ct)
                      ?? throw new InvalidOperationException("Empty bank response");

            if (string.IsNullOrWhiteSpace(bankResponse.PaymentRequestUrl))
                throw new InvalidOperationException("Bank did not return PaymentRequestUrl.");

            return bankResponse;
        }
    }
}
