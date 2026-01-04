using BankAPI.Context;
using BankAPI.DTOs;
using BankAPI.Helpers.HmacValidator;
using BankAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankAPI.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly BankingDbContext _context;
        private readonly IHmacValidator _hmacValidator;

        public PaymentsController(
        BankingDbContext context,
        IHmacValidator hmacValidator)
        {
            _context = context;
            _hmacValidator = hmacValidator;
        }

        [HttpPost("init")]
        public async Task<IActionResult> InitPayment(
        [FromBody] InitPaymentRequestDto dto,
        [FromHeader(Name = "PspID")] Guid pspId,
        [FromHeader(Name = "Signature")] string signature,
        [FromHeader(Name = "Timestamp")] DateTime timestamp)
        {
            var psp = await _context.Psps.FindAsync(pspId);
            if (psp == null)
                return Unauthorized("Invalid PSP");

            if (Math.Abs((DateTime.UtcNow - timestamp).TotalMinutes) > 5)
                return Unauthorized("Timestamp expired");

            var payload =
                $"merchantId={dto.MerchantId}&amount={dto.Amount}" +
                $"&currency={(int)dto.Currency}&stan={dto.Stan}" +
                $"&timestamp={timestamp:o}";

            if (!_hmacValidator.Validate(payload, signature, psp.HMACKey))
                return Unauthorized("Invalid signature");

            var merchant = await _context.Merchants.FindAsync(dto.MerchantId);
            if (merchant == null)
                return BadRequest("Invalid merchant");

            var paymentRequest = new PaymentRequest
            {
                PaymentRequestId = Guid.NewGuid(),
                MerchantId = dto.MerchantId,
                PspId = pspId,
                Amount = dto.Amount,
                Currency = dto.Currency,
                Stan = dto.Stan,
                PspTimestamp = dto.PspTimestamp,
                Status = PaymentRequestStatus.Pending,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };

            _context.PaymentRequests.Add(paymentRequest);
            await _context.SaveChangesAsync();

            var response = new InitPaymentResponseDto
            {
                PaymentRequestId = paymentRequest.PaymentRequestId,
                PaymentReqyestUrl =
                    $"http://localhost:3000/pay/{paymentRequest.PaymentRequestId}"
            };

            return Ok(response);
        }

        [HttpGet("{paymentRequestId:guid}")]
        public async Task<IActionResult> GetPaymentRequest(Guid paymentRequestId)
        {
            var paymentRequest = await _context.PaymentRequests
                .Where(p => p.PaymentRequestId == paymentRequestId)
                .Select(p => new
                {
                    p.PaymentRequestId,
                    p.Amount,
                    Currency = p.Currency.ToString(),
                    p.ExpiresAt,
                    p.Status
                })
                .FirstOrDefaultAsync();

            if (paymentRequest == null)
            {
                return NotFound("Payment request not found.");
            }

            if (paymentRequest.Status != PaymentRequestStatus.Pending)
            {
                return BadRequest("Payment request is not valid.");
            }

            if (paymentRequest.ExpiresAt < DateTime.UtcNow)
            {
                return BadRequest("Payment request expired.");
            }

            return Ok(paymentRequest);
        }

    }
}
