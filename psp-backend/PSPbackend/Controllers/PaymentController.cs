using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PSPbackend.Context;
using PSPbackend.DTOs;
using PSPbackend.DTOs.Bank;
using PSPbackend.Helpers;
using PSPbackend.Models;
using PSPbackend.Models.Enums;
using PSPbackend.Services;

namespace PSPbackend.Controllers
{    
    [ApiController]
    [Route("api/psp")]
    public class PaymentController : ControllerBase
    {
        public PspDbContext _pspDbContext;
        private readonly IBankClient _bank;

        public PaymentController(IBankClient bank, PspDbContext context)
        {
            _pspDbContext = context;
            _bank = bank;

        }

        [HttpPost("initPayment")]
        [DisableCors]
        public async Task<IActionResult> InitPayment([FromBody] PaymentRequestDto req, CancellationToken ct)
        {
            if (req.MerchantId == Guid.Empty|| string.IsNullOrWhiteSpace(req.MerchantPassword))
                return BadRequest("Missing merchant credentials.");

            var merchant = await _pspDbContext.Merchants
                .SingleOrDefaultAsync(m => m.MerchantId == req.MerchantId, ct);

            if (merchant == null || merchant.MerchantPassword != req.MerchantPassword)
                return Unauthorized("Invalid merchant credentials.");

            if (req.Amount <= 0)
                return BadRequest("Amount must be > 0.");

            if (!Enum.TryParse<Currency>(req.Currency, true, out var currency))
            {
                return BadRequest("Invalid or missing currency.");
            }

            var stan = StanGenerator.GenerateStan();
            var pspTimestamp = DateTime.UtcNow;

            var transaction = new PaymentTransaction
            {
                MerchantId = merchant.MerchantId,
                MerchantOrderId = req.MerchantOrderId,
                MerchantTimestamp = req.MerchantTimestamp,
                Amount = req.Amount,
                Currency = currency,

                Stan = stan,
                PspTimestamp = pspTimestamp,

                Status = TransactionStatus.Initialized,
            };

            _pspDbContext.PaymentTransactions.Add(transaction);
            await _pspDbContext.SaveChangesAsync(ct);

            // TODO: Check this url and djust frontend route
            var redirectUrl =
                $"https://localhost:5173/pay?" +
                $"merchantId={transaction.MerchantId}" +
                $"&stan={transaction.Stan}" +
                $"&pspTimestamp={transaction.PspTimestamp:o}";

            return Redirect(redirectUrl);
        }

        // TODO:Ovo se desava u momentu kada smo dosli na url iz metode iznad i zelimo da dobavimo koje su moguce metode placanja za tog merchant-a
        [HttpGet("paymentMethods/{merchantId}")]
        public async Task<IActionResult> GetPaymentMethods([FromRoute] int merchantId, CancellationToken ct)
        {
            throw new NotImplementedException("");
        }

        [HttpPost("selectPaymentMethod")]
        public async Task<IActionResult> SelectPaymentMethod(
            [FromBody] SelectPaymentMethodRequestDto req,
            CancellationToken ct
        )
        {
            if (req.MerchantId == Guid.Empty ||
                string.IsNullOrWhiteSpace(req.Stan))
                return BadRequest("Missing identifiers.");

            var transaction = await _pspDbContext.PaymentTransactions
                .SingleOrDefaultAsync(t =>
                    t.MerchantId == req.MerchantId &&
                    t.Stan == req.Stan &&
                    t.PspTimestamp == req.PspTimestamp,
                    ct);

            if (transaction == null)
                return NotFound("Payment transaction not found.");

            transaction.PaymentMethod = req.PaymentMethod;
            await _pspDbContext.SaveChangesAsync(ct);

            var bankResponse = await _bank
                .CreatePaymentAsync(transaction, ct);
            // TODO: Add try catch here and if success redirect to bank payment url if error then redirect to webshop error url

            //transaction.Status = TransactionStatus.RedirectedToBank;

            return Redirect(bankResponse.PaymentRequestUrl);
        }

        [HttpPost("bank/callback")]
        [DisableCors]
        public async Task<IActionResult> BankCallback(
            [FromBody] BankPaymentStatusDto dto,
            CancellationToken ct
        )
        {
            if (dto.MerchantID == Guid.Empty ||
                string.IsNullOrWhiteSpace(dto.Stan))
            {
                return BadRequest("Missing transaction identifiers.");
            }

            var pspTimestamp = DateTime.SpecifyKind(dto.PspTimestamp, DateTimeKind.Utc);

            var transaction = await _pspDbContext.PaymentTransactions
                .Include(t => t.Merchant)
                .SingleOrDefaultAsync(t =>
                    t.MerchantId == dto.MerchantID &&
                    t.Stan == dto.Stan &&
                    t.PspTimestamp == pspTimestamp,
                    ct);

            if (transaction == null)
                return NotFound("Transaction not found.");

            transaction.Status = dto.Status;
            transaction.AcquirerTimestamp = DateTime.UtcNow;

            await _pspDbContext.SaveChangesAsync(ct);

            var redirectUrl = dto.Status == TransactionStatus.Success
                ? transaction.Merchant.SucessUrl
                : transaction.Merchant.FailedUrl;

            // TODO Add merhcnat order ID to url
            //redirectUrl +=
            //    $"?merchantId={transaction.MerchantId}";;

            return Redirect(redirectUrl);
        }
    }
}
