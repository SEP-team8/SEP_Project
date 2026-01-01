using Microsoft.AspNetCore.Mvc;
using webshop_back.Models;
using webshop_back.Service;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly PaymentService _paymentService;
    private readonly IRepository _repo;

    public PaymentsController(PaymentService paymentService, IRepository repo)
    {
        _paymentService = paymentService;
        _repo = repo;
    }

    [HttpPost("init")]
    public IActionResult Init([FromBody] PaymentInitRequest req)
    {
        // Basic validations per spec: require MERCHANT_ID, AMOUNT, SUCCESS/FAILED URLs
        if (string.IsNullOrEmpty(req.MERCHANT_ID) || req.AMOUNT <= 0) return BadRequest("Invalid request");
        var resp = _paymentService.InitializePayment(req);

        // create Order record in repo with status Initialized
        var order = new Order
        {
            OrderId = req.MERCHANT_ORDER_ID,
            Amount = req.AMOUNT,
            Currency = req.CURRENCY,
            Status = "Initialized"
        };
        _repo.AddOrder(order);

        return Ok(new
        {
            paymentId = resp.PaymentId,
            paymentUrl = resp.PaymentUrl,
            qrPayload = resp.QrPayload,
            amount = resp.Amount,
            currency = resp.Currency
        });
    }

    // endpoints that PSP/Bank would call back to — these are the SUCCESS/FAILED/ERROR endpoints the spec mentions
    [HttpPost("callback")]
    public IActionResult Callback([FromQuery] string orderId, [FromQuery] string status, [FromQuery] string globalTransactionId)
    {
        var order = _repo.GetOrder(orderId);
        if (order == null) return NotFound();

        order.Status = status;
        _repo.UpdateOrder(order);

        // In real app: notify the web shop via SUCCESS_URL / FAILED_URL redirect or server-to-server call.
        return Ok(new { message = "Order updated", orderId, status, globalTransactionId });
    }
}
