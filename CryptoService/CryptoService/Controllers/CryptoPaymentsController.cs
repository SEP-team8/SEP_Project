using CryptoService.DTOs;
using CryptoService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CryptoService.Controllers;

[ApiController]
[Route("crypto/payments")]
public class CryptoPaymentsController : ControllerBase
{
    private readonly ICryptoPaymentService _cryptoPaymentService;

    public CryptoPaymentsController(ICryptoPaymentService cryptoPaymentService)
    {
        _cryptoPaymentService = cryptoPaymentService;
    }

    [HttpPost]
    public async Task<ActionResult<CreateCryptoPaymentResponse>> CreatePayment(
        [FromBody] CreateCryptoPaymentRequest request,
        CancellationToken cancellationToken)
    {
        if (request.MerchantId == Guid.Empty)
            return BadRequest("MerchantId is required.");

        if (request.FiatAmount <= 0)
            return BadRequest("FiatAmount must be greater than zero.");

        if (string.IsNullOrWhiteSpace(request.Stan))
            return BadRequest("Stan is required.");

        try
        {
            var response = await _cryptoPaymentService
                .CreatePaymentAsync(request, cancellationToken);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Error = "Crypto payment creation failed",
                Details = ex.Message
            });
        }
    }

    [HttpGet("{paymentId:guid}")]
    public async Task<ActionResult<CryptoPaymentStatusResponse>> GetPaymentStatus(
        Guid paymentId,
        CancellationToken cancellationToken)
    {
        var status = await _cryptoPaymentService
            .GetStatusAsync(paymentId, cancellationToken);

        if (status == null)
            return NotFound();

        return Ok(status);
    }

    [HttpPost("{paymentId:guid}/check")]
    public async Task<ActionResult<CryptoPaymentStatusResponse>> CheckPayment(
        Guid paymentId,
        CancellationToken cancellationToken)
    {
        var status = await _cryptoPaymentService
            .CheckPaymentStatusAsync(paymentId, cancellationToken);

        if (status == null)
            return NotFound();

        return Ok(status);
    }

    [HttpGet("{paymentId:guid}/qrcode")]
    public async Task<IActionResult> GetPaymentQrCode(
        Guid paymentId,
        CancellationToken cancellationToken)
    {
        try
        {
            var qrCode = await _cryptoPaymentService
                .GeneratePaymentQrCodeAsync(paymentId, cancellationToken);

            return File(qrCode, "image/png");
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("{paymentId:guid}/submitTx")]
    public async Task<ActionResult<CryptoPaymentStatusResponse>> SubmitTransaction(
    Guid paymentId,
    [FromBody] SubmitCryptoTxDto dto,
    CancellationToken cancellationToken)
    {
        if (paymentId == Guid.Empty || dto == null) return BadRequest("Missing data");
        if (paymentId != dto.PaymentId) return BadRequest("PaymentId mismatch");

        try
        {
            var result = await _cryptoPaymentService.SubmitTransactionAsync(dto, cancellationToken);
            if (result == null) return NotFound();
            return Ok(result);
        }
        catch (ArgumentException aex)
        {
            return BadRequest(aex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "SubmitTx failed", Details = ex.Message });
        }
    }

}
