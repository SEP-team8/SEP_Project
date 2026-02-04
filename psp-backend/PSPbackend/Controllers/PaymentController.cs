using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PSPbackend.Context;
using PSPbackend.DTOs;
using PSPbackend.DTOs.Bank;
using PSPbackend.DTOs.Crypto;
using PSPbackend.Helpers;
using PSPbackend.Models;
using PSPbackend.Models.Enums;
using PSPbackend.Services;
using System.Text.Json;

namespace PSPbackend.Controllers
{
    [ApiController]
    [Route("api/psp")]
    public class PaymentController : ControllerBase
    {
        public PspDbContext _pspDbContext;
        private readonly IBankClient _bank;

        // ADDED START: injektovani HttpClientFactory i Configuration
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _config;
        public PaymentController(IBankClient bank, PspDbContext context, IHttpClientFactory httpFactory, IConfiguration config)
        {
            _pspDbContext = context;
            _bank = bank;
            _httpFactory = httpFactory;
            _config = config;
        }

        /*
        // ORIGINAL CONSTRUCTOR (kept for reference) 
        public PaymentController(IBankClient bank, PspDbContext context)
        {
            _pspDbContext = context;
            _bank = bank;
        }
        */

        [HttpPost("initPayment")]
        [DisableCors]
        public async Task<IActionResult> InitPayment([FromBody] PaymentRequestDto req, CancellationToken ct)
        {
            if (req.MerchantId == Guid.Empty || string.IsNullOrWhiteSpace(req.MerchantPassword))
                return BadRequest("Missing merchant credentials.");

            var merchant = await _pspDbContext.Merchants
                .SingleOrDefaultAsync(m => m.MerchantId == req.MerchantId, ct);

            if (merchant == null || merchant.MerchantPassword != req.MerchantPassword)
                return Unauthorized("Invalid merchant credentials.");

            if (req.Amount <= 0)
                return BadRequest("Amount must be > 0.");

            if (!Enum.TryParse<Currency>(req.Currency, true, out var currency))
            {
                return BadRequest("Invalid or missing currency.");
            }

            var stan = StanGenerator.GenerateStan();
            var pspTimestamp = DateTime.UtcNow;

            var transaction = new PaymentTransaction
            {
                MerchantId = merchant.MerchantId,
                MerchantOrderId = req.MerchantOrderId,
                MerchantTimestamp = req.MerchantTimestamp,
                Amount = req.Amount,
                Currency = currency,

                Stan = stan,
                PspTimestamp = pspTimestamp,

                Status = TransactionStatus.Initialized,
            };

            _pspDbContext.PaymentTransactions.Add(transaction);
            await _pspDbContext.SaveChangesAsync(ct);

            var redirectUrl =
                $"http://localhost:5172/pay?" +
                $"merchantId={transaction.MerchantId}" +
                $"&stan={transaction.Stan}" +
                $"&pspTimestamp={transaction.PspTimestamp:o}";

            return Ok(new
            {
                redirectUrl
            });
        }

        [HttpGet("paymentMethods/{merchantId}")]
        public async Task<IActionResult> GetPaymentMethods([FromRoute] Guid merchantId, CancellationToken ct)
        {
            if (merchantId == Guid.Empty)
                return BadRequest("Invalid merchant id.");

            var merchantExists = await _pspDbContext.Merchants
                .AnyAsync(m => m.MerchantId == merchantId, ct);

            if (!merchantExists)
                return NotFound("Merchant not found.");

            var methods = await _pspDbContext.MerchantPaymentMethods
            .Where(mpm => mpm.MerchantId == merchantId)
            .Select(mpm => mpm.PaymentMethod)
            .ToListAsync(ct);

            if (!methods.Any())
                return NotFound("No payment methods configured for this merchant.");

            return Ok(methods);
        }

        [HttpPost("selectPaymentMethod")]
        public async Task<IActionResult> SelectPaymentMethod(
            [FromBody] SelectPaymentMethodRequestDto req,
            CancellationToken ct
        )
        {
            if (req.MerchantId == Guid.Empty ||
                string.IsNullOrWhiteSpace(req.Stan))
                return BadRequest("Missing identifiers.");

            var transaction = await _pspDbContext.PaymentTransactions
                .SingleOrDefaultAsync(t =>
                    t.MerchantId == req.MerchantId &&
                    t.Stan == req.Stan &&
                    t.PspTimestamp == req.PspTimestamp,
                    ct);

            if (transaction == null)
                return NotFound("Payment transaction not found.");

            var merchant = await _pspDbContext.Merchants.SingleOrDefaultAsync(m => m.MerchantId == req.MerchantId, ct);

            if (merchant == null)
                return NotFound("Merchant not found.");

            transaction.PaymentMethod = req.PaymentMethod;
            await _pspDbContext.SaveChangesAsync(ct);

            // Branch for Crypto payments — modular, does NOT touch bank flow
            if (req.PaymentMethod == PaymentMethodType.Crypto)
            {
                try
                {
                    var cryptoBase = _config["CryptoService:BaseUrl"]?.TrimEnd('/');
                    if (string.IsNullOrWhiteSpace(cryptoBase))
                    {
                        transaction.Status = TransactionStatus.Error;
                        await _pspDbContext.SaveChangesAsync(ct);
                        return StatusCode(500, "CryptoService not configured");
                    }

                    var client = _httpFactory.CreateClient();
                    client.BaseAddress = new Uri(cryptoBase);

                    var cryptoReq = new
                    {
                        merchantId = transaction.MerchantId,
                        FiatAmount = transaction.Amount,
                        currency = (int)transaction.Currency, // fiat currency enum
                        stan = transaction.Stan,
                        pspTimestamp = transaction.PspTimestamp
                    };

                    using var httpReq = new HttpRequestMessage(HttpMethod.Post, "/crypto/payments")
                    {
                        Content = JsonContent.Create(cryptoReq)
                    };
                    var resp = await client.SendAsync(httpReq, ct);

                    if (!resp.IsSuccessStatusCode)
                    {
                        transaction.Status = TransactionStatus.Error;
                        await _pspDbContext.SaveChangesAsync(ct);
                        return StatusCode(502, "CryptoService error");
                    }

                    var body = await resp.Content.ReadFromJsonAsync<CreateCryptoPaymentResponse>(cancellationToken: ct);
                    if (body == null)
                    {
                        transaction.Status = TransactionStatus.Error;
                        await _pspDbContext.SaveChangesAsync(ct);
                        return StatusCode(502, "CryptoService invalid response");
                    }

                    // VAŽNO: sačuvaj ETH iznos i postavi currency na ETH
                    transaction.CryptoPaymentId = body.PaymentId;
                    transaction.CryptoAddress = body.EthAddress;
                    transaction.CryptoAmount = body.EthAmount; // NOVO
                    transaction.Currency = Currency.ETH;       // NOVO: sada je 3 (ETH)
                    transaction.Status = TransactionStatus.Initialized; // opcionalno, reset status

                    // **SaveChanges** nakon što su svi crypto field-ovi postavljeni
                    await _pspDbContext.SaveChangesAsync(ct);

                    var frontendBase = _config["PspFrontend:BaseUrl"]?.TrimEnd('/');
                    if (string.IsNullOrWhiteSpace(frontendBase))
                    {
                        transaction.Status = TransactionStatus.Error;
                        await _pspDbContext.SaveChangesAsync(ct);
                        return StatusCode(500, "PSP frontend not configured");
                    }

                    var paymentPage =
                        $"{frontendBase}/payCrypto" +
                        $"?paymentId={body.PaymentId}" +
                        $"&merchantId={transaction.MerchantId}" +
                        $"&stan={transaction.Stan}";

                    return Ok(new StartPaymentResponseDto(paymentPage));
                }
                catch (Exception ex)
                {
                    transaction.Status = TransactionStatus.Error;
                    await _pspDbContext.SaveChangesAsync(ct);
                    return StatusCode(500, "Crypto flow failed: " + ex.Message);
                }
            }


            //Bank flow
            try
            {
                var bankResponse = await _bank
                    .CreatePaymentAsync(transaction, merchant.BankMerchantId, ct);

                return Ok(bankResponse.PaymentRequestUrl);
            }
            catch (Exception)
            {
                transaction.Status = TransactionStatus.Error;
                await _pspDbContext.SaveChangesAsync(ct);
                return Ok(merchant.ErrorUrl);
            }
        }

        [HttpPost("bank/callback")]
        [DisableCors]
        public async Task<IActionResult> BankCallback(
            [FromBody] BankPaymentStatusDto dto,
            CancellationToken ct
        )
        {
            // TODO: Validate this request
            if (dto.MerchantID == Guid.Empty ||
                string.IsNullOrWhiteSpace(dto.Stan))
            {
                return BadRequest("Missing transaction identifiers.");
            }

            var pspTimestamp = DateTime.SpecifyKind(dto.PspTimestamp, DateTimeKind.Utc);

            var merchantId = await _pspDbContext.Merchants
                .Where(bm => bm.BankMerchantId == dto.MerchantID)
                .Select(bm => bm.MerchantId)
                .SingleOrDefaultAsync(ct);

            if (merchantId == Guid.Empty)
            {
                return BadRequest("Unknown bank merchant.");
            }

            var transaction = await _pspDbContext.PaymentTransactions
                .Include(t => t.Merchant)
                .SingleOrDefaultAsync(t =>
                    t.MerchantId == merchantId &&
                    t.Stan == dto.Stan &&
                    t.PspTimestamp == pspTimestamp,
                    ct);

            if (transaction == null)
                return NotFound("Transaction not found.");

            transaction.Status = dto.Status;
            transaction.AcquirerTimestamp = DateTime.UtcNow;

            await _pspDbContext.SaveChangesAsync(ct);

            var redirectUrl = dto.Status == TransactionStatus.Success
                ? transaction.Merchant.SucessUrl
                : transaction.Merchant.FailedUrl;

            var separator = "?";
            redirectUrl = $"{redirectUrl}{separator}merchantOrderId={transaction.MerchantOrderId}";

            return Ok(redirectUrl);
        }

        // Separate endpoint for Crypto callbacks from CryptoService (HMAC verification)
        [HttpPost("crypto/callback")]
        [DisableCors]
        public async Task<IActionResult> CryptoCallback(
            [FromBody] CryptoPaymentNotificationDto dto,
            CancellationToken ct)
        {
            // Validate headers (Signature) and HMAC computed over serialized DTO
            if (!Request.Headers.TryGetValue("Signature", out var signatureHeader))
                return BadRequest("Missing signature header.");

            var secret = _config["Psp:SharedSecret"] ?? throw new InvalidOperationException("Psp:SharedSecret missing");

            // Serialize DTO the same way CryptoService does when signing (JSON)
            var serialized = JsonSerializer.Serialize(dto);
            var expected = SignatureHelper.CreateSignature(serialized, secret);

            if (!string.Equals(expected, signatureHeader.ToString(), StringComparison.OrdinalIgnoreCase))
                return Unauthorized("Invalid signature.");

            // Map payment by bankMerchantId -> merchantId
            var merchantId = await _pspDbContext.Merchants
                .Where(bm => bm.BankMerchantId == dto.MerchantID)
                .Select(bm => bm.MerchantId)
                .SingleOrDefaultAsync(ct);

            if (merchantId == Guid.Empty)
                return BadRequest("Unknown bank merchant.");

            var pspTs = DateTime.SpecifyKind(dto.PspTimestamp, DateTimeKind.Utc);

            var transaction = await _pspDbContext.PaymentTransactions
                .Include(t => t.Merchant)
                .SingleOrDefaultAsync(t =>
                    t.MerchantId == merchantId &&
                    t.Stan == dto.Stan &&
                    t.PspTimestamp == pspTs,
                    ct);

            if (transaction == null) return NotFound("Transaction not found.");

            transaction.Status = dto.Status;
            transaction.AcquirerTimestamp = DateTime.UtcNow;

            // If CryptoService included a tx hash, persist it
            if (!string.IsNullOrWhiteSpace(dto.TransactionHash))
                transaction.TransactionHash = dto.TransactionHash;

            await _pspDbContext.SaveChangesAsync(ct);

            var redirectUrl = dto.Status == TransactionStatus.Success
                ? transaction.Merchant.SucessUrl
                : transaction.Merchant.FailedUrl;

            redirectUrl = $"{redirectUrl}?merchantOrderId={transaction.MerchantOrderId}";

            return Ok(redirectUrl);
        }
        // ADDED END

        [HttpGet("orderData")]
        public async Task<IActionResult> GetOrderData(
        [FromQuery] Guid merchantId,
        [FromQuery] string stan,
        [FromQuery] DateTime pspTimestamp,
        CancellationToken ct)
        {
            if (merchantId == Guid.Empty || string.IsNullOrWhiteSpace(stan))
                return BadRequest("Missing identifiers.");

            var pspTsUtc = DateTime.SpecifyKind(pspTimestamp, DateTimeKind.Utc);

            var tx = await _pspDbContext.PaymentTransactions
                .SingleOrDefaultAsync(t =>
                    t.MerchantId == merchantId &&
                    t.Stan == stan &&
                    t.PspTimestamp == pspTsUtc,
                    ct);

            if (tx == null)
                return NotFound("Payment transaction not found.");

            var dto = new OrderResponseDto
            {
                Amount = tx.Amount,
                Currency = tx.Currency
            };

            return Ok(dto);
        }

        [HttpGet("crypto/paymentInfo")]
        public async Task<IActionResult> GetCryptoPaymentInfo([FromQuery] Guid paymentId, CancellationToken ct)
        {
            if (paymentId == Guid.Empty) return BadRequest("paymentId required");

            var tx = await _pspDbContext.PaymentTransactions
                .SingleOrDefaultAsync(t => t.CryptoPaymentId == paymentId, ct);

            if (tx != null)
            {
                return Ok(new
                {
                    paymentId = tx.CryptoPaymentId,
                    ethAddress = tx.CryptoAddress,
                    ethAmount = tx.CryptoAmount,
                    currency = (int)tx.Currency,
                    chainId = 1 // mainnet
                });
            }

            var cryptoBase = _config["CryptoService:BaseUrl"]?.TrimEnd('/');
            var client = _httpFactory.CreateClient();
            client.BaseAddress = new Uri(cryptoBase);
            var resp = await client.GetAsync($"/crypto/payments/{paymentId}", ct);
            if (!resp.IsSuccessStatusCode) return StatusCode((int)resp.StatusCode, "CryptoService error");
            var body = await resp.Content.ReadFromJsonAsync<CryptoPaymentStatusResponse>(cancellationToken: ct);
            return Ok(new
            {
                paymentId = paymentId,
                ethAddress = body.EthAddress,
                ethAmount = body.EthAmount,
                currency = 3, // ETH
                chainId = body.ChainId
            });
        }


        [HttpPost("crypto/submitTx")]
        public async Task<IActionResult> SubmitCryptoTx([FromBody] SubmitCryptoTxDto dto, CancellationToken ct)
        {
            if (dto.PaymentId == Guid.Empty || string.IsNullOrWhiteSpace(dto.TxHash))
                return BadRequest("Missing data");

            var cryptoBase = _config["CryptoService:BaseUrl"]?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(cryptoBase)) return StatusCode(500, "CryptoService not configured");

            var client = _httpFactory.CreateClient();
            client.BaseAddress = new Uri(cryptoBase);

            var resp = await client.PostAsJsonAsync($"/crypto/payments/{dto.PaymentId}/submitTx", dto, ct);
            if (!resp.IsSuccessStatusCode)
                return StatusCode((int)resp.StatusCode, await resp.Content.ReadAsStringAsync(ct));

            // opcionalno: fetch status i vrati klijentu
            var check = await client.PostAsync($"/crypto/payments/{dto.PaymentId}/check", null, ct);
            var status = await check.Content.ReadFromJsonAsync<CryptoPaymentStatusResponse>(cancellationToken: ct);
            return Ok(status);
        }
    }
}
