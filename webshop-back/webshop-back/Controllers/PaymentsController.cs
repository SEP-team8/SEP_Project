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

        [HttpPost("create")]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentInitRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.MerchantOrderId) || string.IsNullOrWhiteSpace(req.MerchantId))
                return BadRequest("Missing MERCHANT_ORDER_ID or MERCHANT_ID.");

            var order = _repo.GetOrderWithItems(req.MerchantOrderId);
            if (order == null)
                return BadRequest("Order not found. Create order first via /orders endpoint.");

            var resp = await _paymentService.InitializePaymentToAcquirerAsync(req, order);
            return Ok(resp);
        }

        // 2) Webhook endpoint - PSP calls this to notify final status
        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            // read raw body
            Request.EnableBuffering();
            using var sr = new StreamReader(Request.Body, leaveOpen: true);
            var payload = await sr.ReadToEndAsync();
            Request.Body.Position = 0; // rewind for further middleware if needed

            // read signature header (psp will send e.g. X-PSP-Signature)
            var signatureHeader = Request.Headers["X-PSP-Signature"].FirstOrDefault();

            // Try to parse payload to find merchant_id or merchant_order_id
            PaymentWebhookRequest? webhook;
            try
            {
                webhook = JsonSerializer.Deserialize<PaymentWebhookRequest>(payload, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                return BadRequest("Invalid payload");
            }

            if (webhook == null)
                return BadRequest("Empty payload");

            // Resolve merchant (by merchant id if present)
            Merchant? merchant = null;
            if (!string.IsNullOrEmpty(webhook.MerchantId))
            {
                merchant = _repo.GetMerchantByMerchantId(webhook.MerchantId);
            }
            // fallback: try to find order and its merchant
            if (merchant == null && !string.IsNullOrEmpty(webhook.MerchantOrderId))
            {
                var order = _repo.GetOrder(webhook.MerchantOrderId);
                if (order != null)
                    merchant = _repo.GetMerchantByMerchantId(order.MerchantId ?? "");
            }

            var secret = merchant?.WebhookSecret ?? ""; // consider fallback to app configuration

            // Verify signature
            var verified = SignatureHelper.VerifyHmacSha256(signatureHeader, payload, secret);
            if (!verified)
            {
                // return 401 so PSP can alert
                return Unauthorized("Invalid signature");
            }

            // Idempotent update: find order by merchant_order_id or payment_id
            Order? target = null;
            if (!string.IsNullOrEmpty(webhook.MerchantOrderId))
                target = _repo.GetOrder(webhook.MerchantOrderId);

            if (target == null && !string.IsNullOrEmpty(webhook.PaymentId))
            {
                // try by payment id
                target = _repo.GetOrder(webhook.PaymentId); // if your GetOrder supports this; otherwise implement GetOrderByPaymentId
            }

            if (target == null)
            {
                // optionally create record or log and return 404
                return NotFound("Order not found");
            }
            // Update fields idempotently
            target.Status = webhook.Status ?? target.Status;
            if (!string.IsNullOrEmpty(webhook.PaymentId))
                target.PaymentId = webhook.PaymentId;
            if (!string.IsNullOrEmpty(webhook.GlobalTransactionId))
                target.GlobalTransactionId = webhook.GlobalTransactionId;
            if (!string.IsNullOrEmpty(webhook.Stan))
                target.Stan = webhook.Stan;
            target.UpdatedAt = DateTime.UtcNow;

            _repo.UpdateOrder(target);

            // respond 200 quickly
            return Ok();
        }

        // 3) Query order status
        [HttpGet("{orderId}/status")]
        public IActionResult GetStatus(string orderId)
        {
            var order = _repo.GetOrderWithItems(orderId);
            if (order == null) return NotFound();
            return Ok(new
            {
                order.OrderId,
                order.Status,
                order.PaymentId,
                order.PaymentUrl,
                order.GlobalTransactionId,
                order.Stan,
                order.ExpiresAt
            });
        }
    }
}
