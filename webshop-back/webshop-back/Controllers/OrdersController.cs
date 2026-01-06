using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using webshop_back.Data.Models;
using webshop_back.DTOs;
using webshop_back.Service.Interfaces;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IRepository _repo;
    private readonly IConfiguration _config;

    public OrdersController(IRepository repo, IConfiguration config)
    {
        _repo = repo;
        _config = config;
    }

    private int GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("id");
        if (claim == null || !int.TryParse(claim.Value, out var id))
            throw new UnauthorizedAccessException();
        return id;
    }

    [HttpPost]
    public IActionResult CreateOrder([FromBody] CreateOrderRequest req)
    {
        var userId = GetUserId();
        var merchant = HttpContext.Items["Merchant"] as Merchant;
        if (merchant == null) return Unauthorized();

        if (req.Items == null || !req.Items.Any())
            return BadRequest("Order must contain at least one item.");

        var now = DateTime.UtcNow;
        var orderId = Guid.NewGuid().ToString("N");

        // privremeni payment id (kasnije PSP može da ga zameni)
        var paymentId = "P-" + Guid.NewGuid().ToString("N");

        // STAN – uzimamo prvih 6 karaktera paymentId
        var stan = paymentId.Substring(0, 6);

        // 1️⃣ Uzimamo frontend URL iz Merchant-a
        var allowedReturnUrls = System.Text.Json.JsonSerializer
            .Deserialize<string[]>(merchant.AllowedReturnUrls)!;
        var returnUrl = allowedReturnUrls.First(); // možeš dodati logiku za odabir po portu ili domenu

        // 2️⃣ Uzimamo backend HTTPS port iz appsettings
        var backendPort = _config["Dev:HttpsPort"] ?? "7171";

        // 3️⃣ Generišemo payment URL koji simulator koristi
        var paymentUrl =
            $"https://localhost:{backendPort}/psp/simulate-payment" +
            $"?paymentId={Uri.EscapeDataString(paymentId)}" +
            $"&successUrl={Uri.EscapeDataString(returnUrl)}" +
            $"&failedUrl={Uri.EscapeDataString(returnUrl)}";

        var order = new Order
        {
            OrderId = orderId,
            UserId = userId,
            MerchantId = merchant.MerchantId,

            Status = "Initialized",
            CreatedAt = now,
            UpdatedAt = now,
            ExpiresAt = now.AddMinutes(15),

            Currency = "EUR",
            Amount = 0m,

            // payment-related
            PaymentId = paymentId,
            PaymentUrl = paymentUrl,

            Stan = stan,
            GlobalTransactionId = null, // PSP callback će popuniti

            Items = new List<OrderItem>()
        };

        decimal total = 0;

        foreach (var item in req.Items)
        {
            var vehicle = _repo.GetVehicle(item.VehicleId);
            if (vehicle == null || vehicle.MerchantId != merchant.MerchantId)
                return BadRequest($"Invalid vehicle {item.VehicleId}");

            var oi = new OrderItem
            {
                OrderId = orderId,
                VehicleId = vehicle.Id,
                VehicleName = $"{vehicle.Make} {vehicle.Model}",
                PricePerDay = vehicle.Price,
                Days = item.Days
            };

            total += oi.PricePerDay * oi.Days;
            order.Items.Add(oi);
        }

        order.Amount = total;

        _repo.AddOrder(order);

        return Ok(new
        {
            orderId = order.OrderId,
            paymentId = order.PaymentId,
            paymentUrl = order.PaymentUrl,
            amount = order.Amount,
            currency = order.Currency,
            expiresAt = order.ExpiresAt
        });
    }

    [HttpGet]
    public IActionResult GetMyOrders()
    {
        var userId = GetUserId();

        var orders = _repo.GetOrdersForUser(userId);

        var dto = orders.Select(o => new OrderDto
        {
            OrderId = o.OrderId,
            Amount = o.Amount,
            Currency = o.Currency,
            Status = o.Status,
            CreatedAt = o.CreatedAt,
            PaymentId = o.PaymentId,
            MerchantId = o.MerchantId
        });

        return Ok(dto);
    }

    [HttpGet("{orderId}")]
    public IActionResult GetOrder(string orderId)
    {
        var userId = GetUserId();

        var order = _repo.GetOrderWithItems(orderId);
        if (order == null) return NotFound();
        if (order.UserId != userId) return Forbid();

        var dto = new OrderDto
        {
            OrderId = order.OrderId,
            Amount = order.Amount,
            Currency = order.Currency,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            PaymentId = order.PaymentId,
            MerchantId = order.MerchantId,
            Items = order.Items.Select(i => new OrderItemDto
            {
                VehicleId = i.VehicleId,
                VehicleName = i.VehicleName,
                PricePerDay = i.PricePerDay,
                Days = i.Days,
                Total = i.PricePerDay * i.Days
            }).ToList()
        };

        return Ok(dto);
    }
}
