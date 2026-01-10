using PSPbackend.DTOs.Bank;
using PSPbackend.Helpers;
using PSPbackend.Models;
using PSPbackend.Models.Enums;

namespace PSPbackend.Services
{
    public class BankClient : IBankClient
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _config;

        public BankClient(IHttpClientFactory httpFactory, IConfiguration config)
        {
            _httpFactory = httpFactory;
            _config = config;
        }

        public async Task<InitPaymentResponseDto> CreatePaymentAsync(PaymentTransaction transaction, Guid bankMerchantId, CancellationToken ct)
        {

            var request = new InitPaymentRequestDto
            {
                Amount = transaction.Amount,
                Currency = transaction.Currency,
                Stan = transaction.Stan,
                MerchantId = bankMerchantId,
                PspTimestamp = transaction.PspTimestamp,
            };
            var bankApiBaseUrl = _config["Bank:ApiBaseUrl"]!;
            var pspId = Guid.Parse(_config["Psp:PspId"]!);
            var secret = _config["Psp:SharedSecret"]!;

            var _httpClient = _httpFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(bankApiBaseUrl);

            var isQrPayment = transaction.PaymentMethod.Equals(PaymentMethodType.QrCode);
            var timestamp = transaction.PspTimestamp;
            var payload = SignatureHelper.BuildPayload(request, timestamp);
            var signature = SignatureHelper.CreateSignature(payload, secret);

            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                "/api/payments/init")
            {
                Content = JsonContent.Create(request)
            };

            httpRequest.Headers.Add("PspID", pspId.ToString());
            httpRequest.Headers.Add("Signature", signature);
            httpRequest.Headers.Add("Timestamp", timestamp.ToString("O"));
            httpRequest.Headers.Add("IsQrPayment", isQrPayment.ToString().ToLowerInvariant());

            using var response = await _httpClient.SendAsync(httpRequest, ct);

            if (!response.IsSuccessStatusCode)
            {
                // TODO: Return transactionStatus.Error to webshop
                var error = await response.Content.ReadAsStringAsync(ct);
                throw new InvalidOperationException(
                    $"Bank payment creation failed: {error}");
            }

            var bankResponse = await response.Content
                .ReadFromJsonAsync<InitPaymentResponseDto>(ct);

            if (bankResponse == null ||
                bankResponse.PaymentRequestId == Guid.Empty ||
                string.IsNullOrWhiteSpace(bankResponse.PaymentRequestUrl))
            {
                // TODO: Return transactionStatus.Error to webshop
                throw new InvalidOperationException("Invalid response from bank.");
            }

            return bankResponse;
        }
    }
}
