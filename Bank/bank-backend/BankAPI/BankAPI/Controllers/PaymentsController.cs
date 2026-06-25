using BankAPI.DTOs;
using BankAPI.Models;
using BankAPI.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace BankAPI.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentsController : ControllerBase
    {
        public IPaymentService _paymentService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            IPaymentService paymentService,
            ILogger<PaymentsController> logger
        )
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpPost("init")]
        [DisableCors]
        public async Task<IActionResult> InitPayment(
        [FromBody] InitPaymentRequestDto dto,
        [FromHeader(Name = "PspID")] Guid pspId,
        [FromHeader(Name = "Signature")] string signature,
        [FromHeader(Name = "Timestamp")] DateTime timestamp,
        [FromHeader(Name = "IsQrPayment")] bool isQrPayment)
        {
            if (Math.Abs((DateTime.UtcNow - timestamp).TotalMinutes) > 30)
            {
                // PCI DSS 10.2.4 — log all invalid logical access attempts
                _logger.LogWarning("InitPayment rejected — timestamp expired. PspId: {PspId}, MerchantId: {MerchantId}, RequestTimestamp: {Timestamp}, SourceIp: {Ip}",
                    pspId, dto.MerchantId, timestamp, HttpContext.Connection.RemoteIpAddress);
                return Unauthorized("Timestamp expired");
            }

            InitializePaymentServiceResult result = await _paymentService.InitializePayment(dto, pspId, signature, timestamp, isQrPayment);

            return result.Result switch
            {
                InitializePaymentResult.Success =>
                    Ok(result.Response),

                InitializePaymentResult.InvalidPsp =>
                    Unauthorized("Invalid PSP"),

                InitializePaymentResult.InvalidSignature =>
                    Unauthorized("Invalid signature"),

                InitializePaymentResult.InvalidMerchant =>
                    BadRequest("Invalid merchant"),

                _ => StatusCode(500)
            };
        }

        [HttpGet("{paymentRequestId:guid}")]
        public async Task<IActionResult> GetPaymentRequest(Guid paymentRequestId)
        {
            var paymentRequest = await _paymentService.GetPaymentRequest(paymentRequestId);

            return Ok(paymentRequest);
        }

        [HttpPost("{paymentRequestId:guid}/pay")]
        public async Task<IActionResult> ExecutePayment(
            Guid paymentRequestId,
            [FromBody] CardPaymentRequest request)
        {
            var redirectUrl = await _paymentService.ExecuteCardPayment(paymentRequestId, request);

            return Ok(redirectUrl);
        }

        [HttpPost("{paymentRequestId}/qr")]
        public async Task<ActionResult<QRPaymentResponseDto>> GenerateQrPayment(Guid paymentRequestId)
        {
            var result = await _paymentService.GenerateQrPayment(paymentRequestId);
            return Ok(result);
        }
    }
}
