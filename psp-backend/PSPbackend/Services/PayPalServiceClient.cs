using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PSPbackend.Models;
using PSPbackend.Models.Enums;

namespace PSPbackend.Services
{
    public class PayPalServiceClient : IPayPalServiceClient
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _config;

        public PayPalServiceClient(IHttpClientFactory httpFactory, IConfiguration config)
        {
            _httpFactory = httpFactory;
            _config = config;
        }

        private HttpClient CreateClient()
        {
            var client = _httpFactory.CreateClient();
            client.BaseAddress = new Uri(_config["PayPal:BaseUrl"]!);
            return client;
        }

        private async Task<string> GetAccessTokenAsync(HttpClient client, CancellationToken ct)
        {
            var clientId = _config["PayPal:ClientId"]!;
            var clientSecret = _config["PayPal:ClientSecret"]!;
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

            var request = new HttpRequestMessage(HttpMethod.Post, "/v1/oauth2/token");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            });

            var response = await client.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<JsonDocument>(ct);
            return json!.RootElement.GetProperty("access_token").GetString()!;
        }

        public async Task<string> CreateOrderAsync(PaymentTransaction transaction, string returnUrl, string cancelUrl, CancellationToken ct)
        {
            var client = CreateClient();
            var token = await GetAccessTokenAsync(client, ct);

            var currencyCode = transaction.Currency switch
            {
                Currency.EUR => "EUR",
                Currency.USD => "USD",
                _ => "EUR"  // RSD not supported by PayPal — fallback to EUR
            };

            var amount = transaction.Amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

            var body = new
            {
                intent = "CAPTURE",
                purchase_units = new[]
                {
                    new
                    {
                        amount = new
                        {
                            currency_code = currencyCode,
                            value = amount
                        }
                    }
                },
                payment_source = new
                {
                    paypal = new
                    {
                        experience_context = new
                        {
                            return_url = returnUrl,
                            cancel_url = cancelUrl,
                            user_action = "PAY_NOW"
                        }
                    }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "/v2/checkout/orders");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = JsonContent.Create(body);

            var response = await client.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<JsonDocument>(ct);

            foreach (var link in json!.RootElement.GetProperty("links").EnumerateArray())
            {
                if (link.GetProperty("rel").GetString() == "payer-action")
                    return link.GetProperty("href").GetString()!;
            }

            throw new InvalidOperationException("PayPal did not return an approval URL.");
        }

        public async Task CaptureOrderAsync(string paypalOrderId, CancellationToken ct)
        {
            var client = CreateClient();
            var token = await GetAccessTokenAsync(client, ct);

            var request = new HttpRequestMessage(HttpMethod.Post, $"/v2/checkout/orders/{paypalOrderId}/capture");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = new StringContent("{}", Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();
        }
    }
}
