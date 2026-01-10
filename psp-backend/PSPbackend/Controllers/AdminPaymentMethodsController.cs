using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PSPbackend.Context;
using PSPbackend.Models.Enums;
using PSPbackend.Models;
using PSPbackend.DTOs;

namespace PSPbackend.Controllers
{
    [ApiController]
    [Route("api/admin/paymentMethods")]
    public class AdminPaymentMethodsController : ControllerBase
    {
        private readonly PspDbContext _db;

        public AdminPaymentMethodsController(PspDbContext db)
        {
            _db = db;
        }

        [HttpGet("getAllrows")]
        public async Task<ActionResult<List<MerchantMethodRowDto>>> GetAllMerchantsPaymentMethods(CancellationToken ct)
        {
            var rows = await _db.MerchantPaymentMethods
               .Include(x => x.PaymentMethod)
               .OrderBy(x => x.MerchantId)
               .ThenBy(x => x.PaymentMethod.PaymentMethodType) 
               .Select(x => new MerchantMethodRowDto
               {
                   MerchantId = x.MerchantId,
                   PaymentMethodType = x.PaymentMethod.PaymentMethodType.ToString() 
               })
               .ToListAsync(ct);

            return Ok(rows);
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] AddMerchantPaymentMethodDto dto, CancellationToken ct)
        {
            if (dto.MerchantId == Guid.Empty)
                return BadRequest("MerchantId is required.");

            if (string.IsNullOrWhiteSpace(dto.PaymentMethodType))
                return BadRequest("PaymentMethodType is required.");

            var merchantExists = await _db.Merchants
                .AnyAsync(m => m.MerchantId == dto.MerchantId, ct);

            if (!merchantExists)
                return NotFound("Merchant not found.");

            // parse enum
            if (!Enum.TryParse<PaymentMethodType>(dto.PaymentMethodType, true, out var methodType))
                return BadRequest("Invalid PaymentMethodType.");

            var method = await _db.PaymentMethods
                .SingleOrDefaultAsync(pm => pm.PaymentMethodType == methodType, ct);

            if (method == null)
                return NotFound("Payment method is not configured in PaymentMethods table.");

            var alreadyExists = await _db.MerchantPaymentMethods.AnyAsync(x =>
                x.MerchantId == dto.MerchantId && x.PaymentMethodId == method.PaymentMethodId, ct);

            if (alreadyExists)
                return Conflict("This payment method is already assigned to the merchant.");

            _db.MerchantPaymentMethods.Add(new MerchantPaymentMethods
            {
                MerchantId = dto.MerchantId,
                PaymentMethodId = method.PaymentMethodId
            });

            await _db.SaveChangesAsync(ct);

            return Ok("Added.");
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromQuery] Guid merchantId, [FromQuery] string paymentMethodType, CancellationToken ct)
        {
            if (merchantId == Guid.Empty)
                return BadRequest("MerchantId is required.");

            if (string.IsNullOrWhiteSpace(paymentMethodType))
                return BadRequest("PaymentMethodType is required.");

            var merchantExists = await _db.Merchants.AnyAsync(m => m.MerchantId == merchantId, ct);
            if (!merchantExists)
                return NotFound("Merchant not found.");

            // parse enum
            if (!Enum.TryParse<PaymentMethodType>(paymentMethodType, true, out var methodType))
                return BadRequest("Invalid PaymentMethodType.");

            var method = await _db.PaymentMethods
                .SingleOrDefaultAsync(pm => pm.PaymentMethodType == methodType, ct);

            if (method == null)
                return NotFound("Payment method is not configured in PaymentMethods table.");

            var row = await _db.MerchantPaymentMethods
                .SingleOrDefaultAsync(x => x.MerchantId == merchantId && x.PaymentMethodId == method.PaymentMethodId, ct);

            if (row == null)
                return NotFound("Merchant does not have this payment method assigned.");

            // PROVERA: mora ostati bar 1 metoda
            var count = await _db.MerchantPaymentMethods
                .CountAsync(x => x.MerchantId == merchantId, ct);

            if (count <= 1)
                return BadRequest("Cannot delete the last active payment method for this merchant.");

            _db.MerchantPaymentMethods.Remove(row);
            await _db.SaveChangesAsync(ct);

            return Ok("Deleted.");
        }
    }
}
