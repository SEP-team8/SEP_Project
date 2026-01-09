using Microsoft.AspNetCore.Mvc;
using PSPbackend.DTOs;
using PSPbackend.DTOs.Bank;
using PSPbackend.Models;
using PSPbackend.Repos;
using PSPbackend.Services;
using System.Reflection.Metadata.Ecma335;

namespace PSPbackend.Controllers
{
    [Route("api/psp")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IBankClient _bank;
        private readonly IConfiguration _config;
        private readonly IPaymentTransactionRepository _txRepo;

        public PaymentController(IBankClient bank, IConfiguration config, IPaymentTransactionRepository txRepo)
        {
            _bank = bank;
            _config = config;
            _txRepo = txRepo;
        }

        [HttpPost("initPayment")]
        public ActionResult<PaymentResponseDto> InitPayment([FromBody] PaymentRequestDto req, CancellationToken ct)
        {
            if (req.MerchantId == Guid.Empty|| string.IsNullOrWhiteSpace(req.MerchantPassword))
                return BadRequest("Missing merchant credentials.");

            //Unauthorized provera za merchant id iz tabele

            if (req.Amount <= 0)
                return BadRequest("Amount must be > 0.");

            if (!Enum.TryParse<Currency>(req.Currency, true, out _))
            {
                return BadRequest("Invalid or missing currency.");
            }

            //Sta ovde cuvam?

            return Ok(new PaymentResponseDto
            {
                MerchantId = req.MerchantId,
                Amount = req.Amount,
                Currency = req.Currency,
                PaymentMethods = new List<PaymentMethod>(2) { PaymentMethod.Card, PaymentMethod.QrCode } //izmena
            });
        }

        [HttpPost("startPayment")]
        public async Task<ActionResult<StartPaymentResponseDto>> StartPayment([FromBody] StartPaymentRequestDto req, CancellationToken ct)
        {

            if (!Enum.IsDefined(typeof(PaymentMethod), req.PaymentMethod))
            {
                return BadRequest("Invalid payment method.");
            }

            var stan = Random.Shared.Next(100000, 999999).ToString(); // 6 cifara
            var pspTimestamp = DateTime.UtcNow;

            Guid bankMerchantId = Guid.Parse(_config["Bank:MerchantId"]!);
            //bankBody - uzmi iz baze new InitPaymentRequestDto()
            var bankBody = new InitPaymentRequestDto();

            // INSERT transakcije (pre poziva banci)
            //var tx = new PaymentTransaction
            //{
            //    MerchantId = req.MerchantId,
            //    MerchantOrderId = req.Purchase.MerchantOrderId,
            //    MerchantTimestamp = req.MerchantTimestamp,

            //    Amount = req.Purchase.Amount,
            //    Currency = currencyEnum,

            //    Stan = stan,
            //    PspTimestamp = pspTimestamp,
            //    Status = TransactionStatus.Created,
            //};

            //await _txRepo.CreateAsync(tx, ct);

            //Send bank request
            var bankRes = await _bank.InitAsync(bankBody, req.PaymentMethod, ct); //dodati sta treba od podataka za banku
            //error redirect na web shop

            // UPDATE status
            //tx.BankPaymentRequestId = bankRes.PaymentRequestId;
            //tx.Status = TransactionStatus.RedirectedToBank;
            //await _txRepo.UpdateAsync(tx, ct);

            //sacuvaj transakciju u DB dodati 
            var transactionId = Guid.NewGuid();

            return Ok(new StartPaymentResponseDto(
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
            //var tx = await _txRepo.GetByBankPaymentRequestIdAsync(dto.PaymentRequestId, ct);

            //if (tx == null)
            //    return NotFound("Transaction not found.");

            //// UPDATE statusa i vremena banke
            //tx.AcquirerTimestamp = dto.AcquirerTimestamp;
            //tx.Status = dto.Status; //error, success, failed

            //await _txRepo.UpdateAsync(tx, ct);


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
