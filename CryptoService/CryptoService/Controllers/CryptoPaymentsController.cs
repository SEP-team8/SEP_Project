using CryptoService.DTOs;
using CryptoService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CryptoService.Controllers
{
    [Route("crypto/Payments")]
    [ApiController]
    public class CryptoPaymentsController : ControllerBase
    {
        private readonly ICryptoPaymentService _cryptoPaymentService;

        public CryptoPaymentsController(ICryptoPaymentService cryptoPaymentService)
        {
            _cryptoPaymentService = cryptoPaymentService;
        }

        [HttpPost]
        public async Task<ActionResult<CreateCryptoPaymentResponse>> CreatePayment([FromBody] CreateCryptoPaymentRequest request, CancellationToken cancellationToken)
        {
            if (request.FiatAmount <= 0)
            {
                return BadRequest("Fiat amount must be greater than zero.");
            }

            var response = await _cryptoPaymentService.CreatePaymentAsync(request, cancellationToken);
            return Ok(response);
        }

        [HttpGet("{paymentId:guid}")]
        public async Task<ActionResult<CryptoPaymentStatusResponse>> GetPaymentStatus(Guid paymentId, CancellationToken cancellationToken)
        {
            var status = await _cryptoPaymentService.GetStatusAsync(paymentId, cancellationToken);
            if (status is null)
                return NotFound();

            return Ok(status);
        }

        [HttpPost("{paymentId:guid}/check")]
        public async Task<ActionResult<CryptoPaymentStatusResponse>> CheckPayment(Guid paymentId, CancellationToken cancellationToken)
        {
            var status = await _cryptoPaymentService.CheckPaymentStatusAsync(paymentId, cancellationToken);
            if (status is null)
                return NotFound();

            return Ok(status);
        }

        [HttpGet("{paymentId:guid}/qrcode")]
        public async Task<IActionResult> GetPaymentQrCode(Guid paymentId, CancellationToken cancellationToken)
        {
            try
            {
                byte[] qrCode = await _cryptoPaymentService.GeneratePaymentQrCodeAsync(paymentId, cancellationToken);
                return File(qrCode, "image/png");
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}