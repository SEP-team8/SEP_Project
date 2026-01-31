using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using webshop_back.Data.Models;
using webshop_back.Service.Interfaces;

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


        [HttpGet("{orderId}/success")]
        public IActionResult PaymentSuccess(Guid orderId)
        {
            var order = _repo.GetOrder(orderId);
            if (order == null)
                return NotFound("Order not found");

            if (order.Status != OrderStatus.Success)
            {
                order.Status = OrderStatus.Success;
                order.UpdatedAt = DateTime.UtcNow;
                _repo.UpdateOrder(order);
            }

            return Ok(new
            {
                orderId = order.OrderId,
                status = order.Status.ToString()
            });
        }

        
        [HttpGet("{orderId}/failed")]
        public IActionResult PaymentFailed(Guid orderId)
        {
            var order = _repo.GetOrder(orderId);
            if (order == null)
                return NotFound("Order not found");

            if (order.Status != OrderStatus.Failed)
            {
                order.Status = OrderStatus.Failed;
                order.UpdatedAt = DateTime.UtcNow;
                _repo.UpdateOrder(order);
            }

            return Ok(new
            {
                orderId = order.OrderId,
                status = order.Status.ToString()
            });
        }


        [HttpGet("{orderId}/cancelled")]
        public IActionResult PaymentCancelled(Guid orderId)
        {
            var order = _repo.GetOrder(orderId);
            if (order == null)
                return NotFound("Order not found");

            if (order.Status != OrderStatus.Cancelled)
            {
                order.Status = OrderStatus.Cancelled;
                order.UpdatedAt = DateTime.UtcNow;
                _repo.UpdateOrder(order);
            }

            return Ok(new
            {
                orderId = order.OrderId,
                status = order.Status.ToString()
            });
        }

        [HttpGet("{orderId}/status")]
        public IActionResult GetStatus(Guid orderId)
        {
            var order = _repo.GetOrder(orderId);
            if (order == null)
                return NotFound();

            return Ok(new
            {
                order.OrderId,
                status = order.Status.ToString(),
                order.ExpiresAt
            });
        }

        [HttpPost("auto-cancel-pending")]
        [Authorize]
        public IActionResult AutoCancelPendingForCurrentUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("sub")?.Value
                              ?? User.FindFirst("userId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Forbid("User id not available in token claims.");
            }

            var cutoff = DateTime.UtcNow.AddMinutes(-15);

            var userOrders = _repo.GetOrdersForUser(userId);

            if (userOrders == null)
            {
                return Ok(new { cancelled = 0 });
            }

            var toCancel = userOrders
                .Where(o => o.Status == OrderStatus.Pending &&
                            (o.UpdatedAt ?? o.CreatedAt) <= cutoff)
                .ToList();

            foreach (var order in toCancel)
            {
                order.Status = OrderStatus.Cancelled;
                order.UpdatedAt = DateTime.UtcNow;
                _repo.UpdateOrder(order);
            }

            return Ok(new
            {
                cancelled = toCancel.Count,
                cancelledIds = toCancel.Select(o => o.OrderId).ToArray()
            });
        }
    }
}
