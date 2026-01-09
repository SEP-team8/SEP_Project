using Microsoft.AspNetCore.Mvc;
using PSPbackend.DTOs;
using PSPbackend.DTOs.Bank;
using PSPbackend.Models;
using PSPbackend.Repos;
using PSPbackend.Services;

namespace PSPbackend.Controllers
{
    [Route("api/psp")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IBankClient _bank;
        private readonly IConfiguration _config;
        private readonly IPaymentTransactionRepository _txRepo;
        //private readonly IMerchantAuthService _merchantAuth; dodati
        public PaymentController(IBankClient bank, IConfiguration config, IPaymentTransactionRepository txRepo)
        {
            _bank = bank;
            _config = config;
            _txRepo = txRepo;
        }

        [HttpPost("startPayment")]
        public async Task<ActionResult<PaymentResponseDto>> StartPayment([FromBody] PaymentRequestDto req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.MerchantId) || string.IsNullOrWhiteSpace(req.MerchantPassword))
                return Unauthorized("Missing merchant credentials.");

            //dodati autentifikaciju merchanta
            //if (!await _merchantAuth.ValidateAsync(req.MerchantId, req.MerchantPassword, ct))
            //    return Unauthorized("Invalid merchant credentials.");

            if (req.Purchase.Amount <= 0)
                return BadRequest("purchase.amount must be > 0.");

            if (string.IsNullOrWhiteSpace(req.Purchase.Currency))
                return BadRequest("purchase.currency is required.");

            var method = req.PaymentMethod.Trim().ToLowerInvariant();
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

            // INSERT transakcije (pre poziva banci)
            var tx = new PaymentTransaction
            {
                TransactionId = Guid.NewGuid(),

                MerchantId = req.MerchantId,
                MerchantOrderId = req.Purchase.MerchantOrderId,
                MerchantTimestamp = req.MerchantTimestamp, //convert to timestamp

                Amount = req.Purchase.Amount,
                Currency = currencyEnum,

                Stan = stan,
                PspTimestamp = pspTimestamp,
                BankMerchantId = bankMerchantId,

                Status = TransactionStatus.Created,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            await _txRepo.CreateAsync(tx, ct);

            //Send bank request
            var bankRes = await _bank.InitAsync(bankBody, isQrPayment: isQr, ct); //dodati sta treba od podataka za banku

            // UPDATE status
            tx.BankPaymentRequestId = bankRes.PaymentRequestId;
            tx.Status = TransactionStatus.RedirectedToBank;
            await _txRepo.UpdateAsync(tx, ct);

            //sacuvaj transakciju u DB dodati 
            var transactionId = Guid.NewGuid();

            return Ok(new PaymentResponseDto(
                transactionId,
                bankRes.PaymentRequestId,
                bankRes.PaymentRequestUrl
            ));
        }


        // 2) Bank -> PSP status (korak 6 u specifikaciji)
        [HttpPost("payments/status")]
        public async Task<IActionResult> BankPaymentStatus(
            [FromBody] BankPaymentStatusDto dto,
            CancellationToken ct)
        {
            // Nadji transakciju po PaymentRequestId
            var tx = await _txRepo.GetByBankPaymentRequestIdAsync(dto.PaymentRequestId, ct);

            if (tx == null)
                return NotFound("Transaction not found.");

            // UPDATE statusa i vremena banke
            tx.AcquirerTimestamp = dto.AcquirerTimestamp;
            tx.Status = dto.Status; //error, success, failed

            await _txRepo.UpdateAsync(tx, ct);


            //za slanje treba mi za svakog merchanta url, smisliti kako ovo 

            //var http = _httpFactory.CreateClient();

            //var payload = new
            //{
            //    transactionId = tx.TransactionId,
            //    merchantId = tx.MerchantId,
            //    merchantOrderId = tx.MerchantOrderId,
            //    status = tx.Status.ToString(),
            //    stan = tx.Stan,
            //    bankPaymentRequestId = tx.BankPaymentRequestId,
            //    acquirerTimestamp = tx.AcquirerTimestamp
            //};

            //await http.PostAsJsonAsync(callbackUrl, payload, ct);

            return Ok();
        }

    }
}
