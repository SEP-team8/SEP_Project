using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using webshop_back.Data.Models;
using webshop_back.DTOs;
using webshop_back.Helpers;
using webshop_back.Service.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IRepository _repo;
    private readonly ILogger<PaymentsController> _logger;
    private readonly IConfiguration _config;

    public PaymentsController(IPaymentService paymentService, IRepository repo, ILogger<PaymentsController> logger, IConfiguration config)
    {
        _paymentService = paymentService;
        _repo = repo;
        _logger = logger;
        _config = config;
    }

    [HttpPost("init")]
    public IActionResult Init([FromBody] PaymentInitRequest req)
    {
        if (string.IsNullOrEmpty(req.MERCHANT_ID) || req.AMOUNT <= 0 || string.IsNullOrEmpty(req.MERCHANT_ORDER_ID))
            return BadRequest("Invalid request: missing merchant/order/amount.");

        // 1) validate merchant
        var merchant = _repo.GetMerchant(req.MERCHANT_ID);
        if (merchant == null || !merchant.IsActive)
        {
            _logger.LogWarning("Payment init: unknown or inactive merchant {MerchantId}", req.MERCHANT_ID);
            return Unauthorized("Unknown merchant");
        }

        // Optionally validate MERCHANT_PASSWORD by comparing hashed secret (if provided)
        // For now we skip password check or implement VerifyMerchantSecret(merchant, req.MERCHANT_PASSWORD)

        // 2) idempotency: if same MERCHANT_ORDER_ID already exists -> return existing
        var existing = _repo.GetOrder(req.MERCHANT_ORDER_ID);
        if (existing != null)
        {
            _logger.LogInformation("Payment init: returning existing order for {OrderId}", req.MERCHANT_ORDER_ID);
            return Ok(new
            {
                paymentId = existing.PaymentId,
                paymentUrl = existing.PaymentUrl,
                amount = existing.Amount,
                currency = existing.Currency,
                status = existing.Status
            });
        }

        int? userId = null;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim != null)
            userId = int.Parse(userIdClaim);

        // 3) create Order record (initialized)
        var order = new Order
        {
            OrderId = req.MERCHANT_ORDER_ID,
            MerchantId = merchant.MerchantId,
            UserId = userId,
            Amount = req.AMOUNT,
            Currency = req.CURRENCY,
            Status = "Initialized",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15) // example expiry
        };

        _repo.AddOrder(order);

        // 4) call payment service to initialize on PSP (returns PaymentId/paymentUrl/qr)
        var pspResp = _paymentService.InitializePaymentToAcquirer(req, order);

        // 5) update order with PSP response
        order.PaymentId = pspResp.PaymentId;
        order.PaymentUrl = pspResp.PaymentUrl;
        order.Stan = order.Stan ?? (pspResp.PaymentId?.Substring(0, 6) ?? null); // rough
        order.UpdatedAt = DateTime.UtcNow;

        _repo.UpdateOrder(order);

        return Ok(new
        {
            paymentId = pspResp.PaymentId,
            paymentUrl = pspResp.PaymentUrl,
            qrPayload = pspResp.QrPayload,
            amount = pspResp.Amount,
            currency = pspResp.Currency
        });
    }

    // PSP will call this endpoint with signature header for verification
    [HttpPost("callback")]
    public async Task<IActionResult> Callback()
    {
        // read raw body
        var body = await new StreamReader(Request.Body).ReadToEndAsync();

        // verify signature header
        var signature = Request.Headers["X-Signature"].FirstOrDefault();
        // choose secret: either merchant's secret (if callback contains merchantId) or global PSP secret from config
        var pspSecret = _config["Psp:SharedSecret"] ?? "";

        if (!SignatureHelper.VerifyHmacSha256(signature, body, pspSecret))
        {
            _logger.LogWarning("Callback signature verification failed");
            return Unauthorized("Invalid signature");
        }

        // parse body as CallbackDto
        CallbackDto payload;
        try
        {
            payload = System.Text.Json.JsonSerializer.Deserialize<CallbackDto>(body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse callback payload");
            return BadRequest("Invalid payload");
        }

        var order = _repo.GetOrder(payload.OrderId);
        if (order == null) return NotFound();

        // idempotent update: if same global id & status already applied -> ok
        if (!string.IsNullOrEmpty(payload.GlobalTransactionId) &&
            payload.GlobalTransactionId == order.GlobalTransactionId &&
            payload.Status == order.Status)
        {
            return Ok(new { message = "Already processed" });
        }

        // optional: validate amount matches
        if (payload.Amount.HasValue && payload.Amount.Value != order.Amount)
        {
            // log & optionally flag for manual review
            _logger.LogWarning("Callback amount mismatch for {OrderId} payload {PayloadAmount} expected {OrderAmount}", order.OrderId, payload.Amount, order.Amount);
        }

        order.Status = payload.Status;
        order.GlobalTransactionId = payload.GlobalTransactionId;
        order.UpdatedAt = DateTime.UtcNow;

        _repo.UpdateOrder(order);

        // optionally notify webshop (server-to-server) or websocket to storefront

        return Ok(new { message = "Order updated", orderId = order.OrderId, status = order.Status });
    }
}
