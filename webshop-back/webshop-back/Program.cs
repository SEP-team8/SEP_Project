using webshop_back.Models;
using webshop_back.Service;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var config = builder.Configuration;
var frontendUrl = config.GetValue<string>("FrontendUrl") ?? "http://localhost:5173";
var baseUrl = config.GetValue<string>("BaseUrl") ?? "http://localhost:5000";

// Services
builder.Services.AddSingleton<DataStore>();
builder.Services.AddSingleton<PspStubService>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(frontendUrl).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors();
app.UseRouting();

// --- Public API: GET /api/vehicles
app.MapGet("/api/vehicles", (DataStore ds) =>
{
    var vehicles = ds.GetVehicles();
    return Results.Ok(vehicles);
});

// --- Register simple user (no password hashing, demo only)
app.MapPost("/api/auth/register", async (HttpContext http, DataStore ds) =>
{
    var req = await http.Request.ReadFromJsonAsync<User>();
    if (req == null || string.IsNullOrEmpty(req.Email)) return Results.BadRequest(new { error = "email required" });
    var users = ds.GetUsers();
    if (users.Any(u => u.Email == req.Email)) return Results.BadRequest(new { error = "Email exists" });
    var u = new User { Id = Guid.NewGuid().ToString(), Email = req.Email, Name = req.Name };
    ds.AddUser(u);
    return Results.Ok(new { user = u });
});

// --- Create order and initiate payment (PSP stub)
app.MapPost("/api/orders", async (HttpContext http, DataStore ds, PspStubService psp) =>
{
    var body = await http.Request.ReadFromJsonAsync<Order>();
    if (body == null) return Results.BadRequest(new { error = "invalid body" });

    // calculate total
    var total = body.Items?.Sum(i => i.Price * i.Quantity) ?? 0m;
    var order = new Order
    {
        Id = Guid.NewGuid().ToString(),
        MerchantOrderId = "ORD-" + Guid.NewGuid().ToString(),
        UserId = body.UserId,
        Amount = total,
        Currency = body.Currency ?? "EUR",
        Status = "CREATED",
        Items = body.Items ?? new List<OrderItem>(),
        CreatedAt = DateTime.UtcNow
    };

    ds.AddOrder(order);

    var pspResp = psp.InitiatePayment(baseUrl, order.Amount, order.Currency, order.MerchantOrderId);

    var payment = new Payment
    {
        Id = Guid.NewGuid().ToString(),
        OrderId = order.Id,
        PspPaymentId = pspResp.paymentId,
        PaymentUrl = pspResp.paymentUrl,
        Stan = pspResp.stan,
        Status = "INIT",
        CreatedAt = DateTime.UtcNow
    };

    ds.AddPayment(payment);

    return Results.Ok(new { order, payment });
});

// --- Get order + payments
app.MapGet("/api/orders/{orderId}", (string orderId, DataStore ds) =>
{
    var orders = ds.GetOrders();
    var order = orders.FirstOrDefault(o => o.Id == orderId);
    if (order == null) return Results.NotFound(new { error = "order not found" });
    var payments = ds.GetPayments().Where(p => p.OrderId == order.Id).ToList();
    return Results.Ok(new { order, payments });
});

// --- PSP mock UI (GET) - shows simple HTML with buttons
app.MapGet("/psp/mock-pay", (HttpRequest req) =>
{
    var paymentId = req.Query["payment_id"].ToString();
    if (string.IsNullOrEmpty(paymentId)) return Results.BadRequest("payment_id missing");

    var html = $@"
    <html>
      <body>
        <h2>PSP Mock Payment</h2>
        <p>Payment ID: {paymentId}</p>
        <form method='post' action='/psp/mock-pay/complete'>
          <input type='hidden' name='payment_id' value='{paymentId}' />
          <button name='result' value='success' type='submit'>Simulate Success</button>
          <button name='result' value='failed' type='submit'>Simulate Failure</button>
        </form>
      </body>
    </html>";
    return Results.Content(html, "text/html");
});

// --- PSP posts the result here (form POST)
app.MapPost("/psp/mock-pay/complete", async (HttpRequest req, DataStore ds) =>
{
    // read form-encoded body
    if (!req.HasFormContentType) return Results.BadRequest("invalid form");
    var form = await req.ReadFormAsync();
    var paymentId = form["payment_id"].ToString();
    var result = form["result"].ToString();

    var payments = ds.GetPayments();
    var payment = payments.FirstOrDefault(p => p.PspPaymentId == paymentId);
    if (payment == null)
    {
        var notFoundHtml = $"<p>Payment {paymentId} not found in backend datastore.</p>";
        return Results.Content(notFoundHtml, "text/html");
    }

    payment.Status = result == "success" ? "SUCCESS" : "FAILED";
    ds.UpdatePayment(payment);

    // update corresponding order
    var orders = ds.GetOrders();
    var order = orders.FirstOrDefault(o => o.Id == payment.OrderId);
    if (order != null)
    {
        order.Status = payment.Status == "SUCCESS" ? "PAID" : "FAILED";
        ds.UpdateOrder(order);
    }

    // redirect back to frontend with orderId query param
    var frontendUrl = req.HttpContext.RequestServices.GetRequiredService<IConfiguration>().GetValue<string>("FrontendUrl") ?? "http://localhost:5173";
    var redirect = $"{frontendUrl}/?orderId={order?.Id}";
    return Results.Redirect(redirect);
});

app.Run();