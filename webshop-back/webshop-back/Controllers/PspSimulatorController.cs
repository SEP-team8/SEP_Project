using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("psp")]
public class PspSimulatorController : ControllerBase
{
    // This controller simulates bank payment page for demonstration/testing.
    [HttpGet("simulate-payment")]
    public IActionResult SimulatePayment([FromQuery] string paymentId, [FromQuery] string successUrl, [FromQuery] string failedUrl)
    {
        // Render a simple HTML page that simulates card entry and redirects with result
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

    [HttpPost("complete-payment")]
    public IActionResult CompletePayment([FromForm] string paymentId, [FromForm] string action, [FromForm] string successUrl, [FromForm] string failedUrl)
    {
        // If action == ok -> redirect to successUrl with query params (simulate STAN, global id)
        var globalId = $"GTID-{Guid.NewGuid():N}";
        if (action == "ok")
        {
            // append query to successUrl
            var url = $"{successUrl}?paymentId={paymentId}&global={globalId}&status=OK";
            return Redirect(url);
        }
        else
        {
            var url = $"{failedUrl}?paymentId={paymentId}&global={globalId}&status=FAILED";
            return Redirect(url);
        }
    }
}
