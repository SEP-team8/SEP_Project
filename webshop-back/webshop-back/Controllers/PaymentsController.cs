using Microsoft.AspNetCore.Mvc;
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

    public PaymentsController(
        IPaymentService paymentService,
        IRepository repo,
        ILogger<PaymentsController> logger,
        IConfiguration config)
    {
        _paymentService = paymentService;
        _repo = repo;
        _logger = logger;
        _config = config;
    }

    [HttpPost("init")]
    public IActionResult Init([FromBody] PaymentInitRequest req)
    {
        if (req.AMOUNT <= 0 || string.IsNullOrEmpty(req.MERCHANT_ORDER_ID))
            return BadRequest("Invalid request");

        var merchant = HttpContext.Items["Merchant"] as Merchant;
        if (merchant == null || !merchant.IsActive)
            return Unauthorized();

        var order = _repo.GetOrder(req.MERCHANT_ORDER_ID);
        if (order == null)
            return BadRequest("Order not found. Create order first.");

        if (order.MerchantId != merchant.MerchantId)
            return Forbid();

        // 1️⃣ Uzimamo frontend URL iz Merchant-a
        var allowedReturnUrls = System.Text.Json.JsonSerializer
            .Deserialize<string[]>(merchant.AllowedReturnUrls)!;
        var returnUrl = allowedReturnUrls.First(); // uzimamo prvi URL

        // 2️⃣ Uzimamo backend HTTPS port iz appsettings
        var backendPort = _config["Dev:HttpsPort"] ?? "7171";

        // 3️⃣ Ako payment već postoji, vratimo ga sa ispravnim URL-om
        if (!string.IsNullOrEmpty(order.PaymentId))
        {
            // osvežimo paymentUrl u slučaju da se port promenio ili returnUrl nije isti
            order.PaymentUrl =
                $"https://localhost:{backendPort}/psp/simulate-payment" +
                $"?paymentId={Uri.EscapeDataString(order.PaymentId)}" +
                $"&successUrl={Uri.EscapeDataString(returnUrl)}" +
                $"&failedUrl={Uri.EscapeDataString(returnUrl)}";

            _repo.UpdateOrder(order);

            return Ok(new
            {
                paymentId = order.PaymentId,
                paymentUrl = order.PaymentUrl,
                amount = order.Amount,
                currency = order.Currency
            });
        }

        // 4️⃣ Kreiramo novi payment
        var pspResp = _paymentService.InitializePaymentToAcquirer(req, order);

        // override paymentUrl da koristi pravi frontend
        pspResp.PaymentUrl =
            $"https://localhost:{backendPort}/psp/simulate-payment" +
            $"?paymentId={Uri.EscapeDataString(pspResp.PaymentId)}" +
            $"&successUrl={Uri.EscapeDataString(returnUrl)}" +
            $"&failedUrl={Uri.EscapeDataString(returnUrl)}";

        order.PaymentId = pspResp.PaymentId;
        order.PaymentUrl = pspResp.PaymentUrl;
        order.Stan = pspResp.PaymentId?.Substring(0, 6);
        order.UpdatedAt = DateTime.UtcNow;

        _repo.UpdateOrder(order);

        return Ok(new
        {
            paymentId = pspResp.PaymentId,
            paymentUrl = pspResp.PaymentUrl,
            amount = pspResp.Amount,
            currency = pspResp.Currency
        });
    }

    [HttpPost("callback")]
    public async Task<IActionResult> Callback()
    {
        var body = await new StreamReader(Request.Body).ReadToEndAsync();

        var signature = Request.Headers["X-Signature"].FirstOrDefault();
        var pspSecret = _config["Psp:SharedSecret"] ?? "";

        if (!SignatureHelper.VerifyHmacSha256(signature, body, pspSecret))
        {
            _logger.LogWarning("Callback signature verification failed");
            return Unauthorized("Invalid signature");
        }

        CallbackDto payload;
        try
        {
            payload = System.Text.Json.JsonSerializer.Deserialize<CallbackDto>(body,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse callback payload");
            return BadRequest("Invalid payload");
        }

        var order = _repo.GetOrder(payload.OrderId);
        if (order == null) return NotFound();

        if (!string.IsNullOrEmpty(payload.GlobalTransactionId) &&
            payload.GlobalTransactionId == order.GlobalTransactionId &&
            payload.Status == order.Status)
        {
            return Ok(new { message = "Already processed" });
        }

        if (payload.Amount.HasValue && payload.Amount.Value != order.Amount)
        {
            _logger.LogWarning(
                "Callback amount mismatch for {OrderId} payload {PayloadAmount} expected {OrderAmount}",
                order.OrderId, payload.Amount, order.Amount);
        }

        order.Status = payload.Status;
        order.GlobalTransactionId = payload.GlobalTransactionId;
        order.UpdatedAt = DateTime.UtcNow;

        _repo.UpdateOrder(order);

        return Ok(new { message = "Order updated", orderId = order.OrderId, status = order.Status });
    }
}
