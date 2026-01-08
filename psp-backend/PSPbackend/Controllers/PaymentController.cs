using Microsoft.AspNetCore.Mvc;
using PSPbackend.DTOs;
using PSPbackend.DTOs.Bank;
using PSPbackend.Models;
using PSPbackend.Services;

namespace PSPbackend.Controllers
{
    [Route("api/psp")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IBankClient _bank;
        private readonly IConfiguration _config;


        public PaymentController(IBankClient bank, IConfiguration config)
        {
            _bank = bank;
            _config = config;
        }

        [HttpPost("startPayment")]
        public async Task<ActionResult<PaymentResponseDto>> StartPayment( [FromRoute] string method, [FromBody] PaymentRequestDto req, CancellationToken ct)
        {
            if (req?.Purchase is null || string.IsNullOrWhiteSpace(req.Purchase.Id))
                return BadRequest("purchase.id is required.");

            if (req.Purchase.Amount <= 0)
                return BadRequest("purchase.amount must be > 0.");

            if (string.IsNullOrWhiteSpace(req.Purchase.Currency))
                return BadRequest("purchase.currency is required.");

            method = (method ?? req.PaymentMethod ?? "").Trim().ToLowerInvariant();
            var isCard = method == "card";
            var isQr = method == "qr";
            if (!isCard && !isQr)
                return BadRequest("Only 'card' and 'qr' supported currently");

            //Stan -> mozda se drugacije generise videti
            var stan = Random.Shared.Next(100000, 999999).ToString(); // 6 cifara
            var pspTimestamp = DateTime.UtcNow;

            var currencyEnum = req.Purchase.Currency.ToUpperInvariant() switch
            {
                "RSD" => Currency.RSD,
                "EUR" => Currency.EUR,
                "USD" => Currency.USD,
                _ => throw new InvalidOperationException("Unsupported currency")
            };

            //Merchand id psp dobije od banke
            Guid bankMerchantId = Guid.Parse(_config["Bank:MerchantId"]!); 
            var bankBody = new InitPaymentRequestDto((float)req.Purchase.Amount, currencyEnum, bankMerchantId, stan, pspTimestamp);

            //Send bank request
            var bankRes = await _bank.InitAsync(bankBody, isQrPayment: isQr, ct);

            //sacuvaj transakciju u DB dodati 
            var transactionId = Guid.NewGuid(); 

            return Ok(new PaymentResponseDto(
                transactionId,
                bankRes.PaymentRequestId,
                bankRes.PaymentRequestUrl
            ));
        }

    }
}
