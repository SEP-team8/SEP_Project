using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PSPbackend.Context;
using PSPbackend.Models.Enums;
using PSPbackend.Services;

namespace PSPbackend.Controllers
{
    [ApiController]
    [Route("api/paypal")]
    public class PayPalCheckoutController : ControllerBase
    {
        private readonly IPayPalServiceClient _paypal;
        private readonly PspDbContext _db;

        public PayPalCheckoutController(IPayPalServiceClient paypal, PspDbContext db)
        {
            _paypal = paypal;
            _db = db;
        }

        [HttpGet("capture")]
        public async Task<IActionResult> Capture(
            [FromQuery] Guid merchantId,
            [FromQuery] string stan,
            [FromQuery] string pspTimestamp,
            [FromQuery] string token,
            CancellationToken ct)
        {
            var pspTsUtc = DateTime.SpecifyKind(DateTime.Parse(pspTimestamp), DateTimeKind.Utc);

            var transaction = await _db.PaymentTransactions
                .Include(t => t.Merchant)
                .SingleOrDefaultAsync(t =>
                    t.MerchantId == merchantId &&
                    t.Stan == stan &&
                    t.PspTimestamp == pspTsUtc, ct);

            if (transaction == null)
                return NotFound("Transaction not found.");

            try
            {
                await _paypal.CaptureOrderAsync(token, ct);

                transaction.Status = TransactionStatus.Success;
                transaction.AcquirerTimestamp = DateTime.UtcNow;
                await _db.SaveChangesAsync(ct);

                return Redirect($"{transaction.Merchant.SucessUrl}?merchantOrderId={transaction.MerchantOrderId}");
            }
            catch
            {
                transaction.Status = TransactionStatus.Error;
                await _db.SaveChangesAsync(ct);

                return Redirect($"{transaction.Merchant.FailedUrl}?merchantOrderId={transaction.MerchantOrderId}");
            }
        }

        // PayPal redirects the user here if they click "Cancel" on the PayPal page.
        [HttpGet("cancel")]
        public async Task<IActionResult> Cancel(
            [FromQuery] Guid merchantId,
            [FromQuery] string stan,
            [FromQuery] string pspTimestamp,
            CancellationToken ct)
        {
            var pspTsUtc = DateTime.SpecifyKind(DateTime.Parse(pspTimestamp), DateTimeKind.Utc);

            var transaction = await _db.PaymentTransactions
                .Include(t => t.Merchant)
                .SingleOrDefaultAsync(t =>
                    t.MerchantId == merchantId &&
                    t.Stan == stan &&
                    t.PspTimestamp == pspTsUtc, ct);

            if (transaction == null)
                return NotFound("Transaction not found.");

            transaction.Status = TransactionStatus.Failed;
            await _db.SaveChangesAsync(ct);

            return Redirect($"{transaction.Merchant.FailedUrl}?merchantOrderId={transaction.MerchantOrderId}");
        }
    }
}
