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

        if (!string.IsNullOrEmpty(order.PaymentId))
        {
            return Ok(new
            {
                paymentId = order.PaymentId,
                paymentUrl = order.PaymentUrl,
                amount = order.Amount,
                currency = order.Currency
            });
        }

        var pspResp = _paymentService.InitializePaymentToAcquirer(req, order);

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
            payload = System.Text.Json.JsonSerializer.Deserialize<CallbackDto>(body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
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
            _logger.LogWarning("Callback amount mismatch for {OrderId} payload {PayloadAmount} expected {OrderAmount}", order.OrderId, payload.Amount, order.Amount);
        }

        order.Status = payload.Status;
        order.GlobalTransactionId = payload.GlobalTransactionId;
        order.UpdatedAt = DateTime.UtcNow;

        _repo.UpdateOrder(order);


        return Ok(new { message = "Order updated", orderId = order.OrderId, status = order.Status });
    }
}
