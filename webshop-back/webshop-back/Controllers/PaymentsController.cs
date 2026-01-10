using Microsoft.AspNetCore.Mvc;
using webshop_back.Data.Models;
using webshop_back.Service.Interfaces;
using webshop_back.Helpers;
using System.Text.Json;
using webshop_back.DTOs;

namespace webshop_back.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly IRepository _repo;
        private readonly IPaymentService _paymentService;

        public PaymentsController(IRepository repo, IPaymentService paymentService)
        {
            _repo = repo;
            _paymentService = paymentService;
        }

        // 1) Init payment
        [HttpPost("create")]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentInitRequest req)
        {
            if (req.MerchantOrderId == Guid.Empty || req.MerchantId == Guid.Empty)
            {
                return BadRequest("Missing MERCHANT_ORDER_ID or MERCHANT_ID.");
            }

            var order = _repo.GetOrderWithItems(req.MerchantOrderId);
            if (order == null)
            {
                return BadRequest("Order not found. Create order first via /orders endpoint.");
            }

            var resp = await _paymentService.InitializePaymentToAcquirerAsync(req, order);
            return Ok(resp);
        }

        // 2) PSP webhook callback
        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            Request.EnableBuffering();

            using var sr = new StreamReader(Request.Body, leaveOpen: true);
            var payload = await sr.ReadToEndAsync();
            Request.Body.Position = 0;

            var signatureHeader = Request.Headers["X-PSP-Signature"].FirstOrDefault();

            PaymentWebhookRequest? webhook;
            try
            {
                webhook = JsonSerializer.Deserialize<PaymentWebhookRequest>(
                    payload,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
            }
            catch
            {
                return BadRequest("Invalid payload");
            }

            if (webhook == null)
                return BadRequest("Empty payload");

            if (webhook.MerchantId == Guid.Empty || webhook.MerchantOrderId == Guid.Empty)
                return BadRequest("Invalid merchant or order id");

            // Resolve merchant directly
            var merchant = _repo.GetMerchantByMerchantId(webhook.MerchantId);
            if (merchant == null)
                return BadRequest("Merchant not found");

            // Verify signature
            var verified = SignatureHelper.VerifyHmacSha256(
                signatureHeader,
                payload,
                merchant.WebhookSecret
            );

            if (!verified)
            {
                return Unauthorized("Invalid signature");
            }

            // Resolve order
            var order = _repo.GetOrder(webhook.MerchantOrderId);
            if (order == null)
            {
                return NotFound("Order not found");
            }

            // Idempotent update
            order.UpdatedAt = DateTime.UtcNow;

            // TODO: map PSP status → internal status
            // order.Status = webhook.Status;

            _repo.UpdateOrder(order);

            return Ok();
        }

        // 3) Query order status
        [HttpGet("{orderId}/status")]
        public IActionResult GetStatus(Guid orderId)
        {
            if (orderId == Guid.Empty)
                return BadRequest("Invalid order id");

            var order = _repo.GetOrderWithItems(orderId);
            if (order == null)
                return NotFound();

            return Ok(new
            {
                order.OrderId,
                order.Status,
                order.ExpiresAt
            });
        }
    }
}
