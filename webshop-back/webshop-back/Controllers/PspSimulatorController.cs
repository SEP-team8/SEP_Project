using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("psp")]
public class PspSimulatorController : ControllerBase
{
    // GET /psp/simulate-payment
    [HttpGet("simulate-payment")]
    public IActionResult SimulatePayment([FromQuery] string paymentId, [FromQuery] string successUrl, [FromQuery] string failedUrl)
    {
        // Simple HTML page to simulate card entry
        var html = $@"
<html>
<body>
    <h2>Simulated Bank Payment Page</h2>
    <p>PaymentId: {paymentId}</p>
    <form method='post' action='/psp/complete-payment'>
        <input type='hidden' name='paymentId' value='{paymentId}'/>
        <input type='hidden' name='successUrl' value='{successUrl}'/>
        <input type='hidden' name='failedUrl' value='{failedUrl}'/>
        <label>Card PAN: <input name='pan' /></label><br/>
        <label>Expiry MM/YY: <input name='exp' /></label><br/>
        <label>CVV: <input name='cvv' /></label><br/>
        <button type='submit' name='action' value='ok'>Pay</button>
        <button type='submit' name='action' value='fail'>Fail</button>
    </form>
</body>
</html>";
        return new ContentResult { Content = html, ContentType = "text/html" };
    }

    // POST /psp/complete-payment
    [HttpPost("complete-payment")]
    public IActionResult CompletePayment([FromForm] string paymentId, [FromForm] string action, [FromForm] string successUrl, [FromForm] string failedUrl)
    {
        var globalId = $"GTID-{Guid.NewGuid():N}";
        var targetUrl = action == "ok"
            ? $"{successUrl}?paymentId={paymentId}&global={globalId}&status=OK"
            : $"{failedUrl}?paymentId={paymentId}&global={globalId}&status=FAILED";

        // HTML sa JS redirectom radi i sa React frontendom
        var html = $@"
<html>
<body>
    <h2>Payment Result</h2>
    <p>Redirecting to frontend...</p>
    <script>
        // replace = ne dodaje u history
        window.location.replace('{targetUrl}');
    </script>
</body>
</html>";
        return new ContentResult { Content = html, ContentType = "text/html" };
    }
}
