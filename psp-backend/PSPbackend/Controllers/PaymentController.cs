using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PSPbackend.DTOs;

namespace PSPbackend.Controllers
{
    [Route("api/psp")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IConfiguration _config;

        public PaymentController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("payment/{method}")]
        public ActionResult<PaymentResponse> StartPayment(
            [FromRoute] string method,
            [FromBody] PaymentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.PurchaseId))
                return BadRequest("PurchaseId is required.");

            method = method.ToLower(); //izmena

            // base url from appsettings.json
            var baseUrl = _config[$"PaymentProviders:{method}"];

            if (string.IsNullOrWhiteSpace(baseUrl))
                return BadRequest("Unsupported payment method.");

            //napravi transaction id ovo izmena
            var transactionId = "12345";

           
            var redirectUrl = $"{baseUrl}?transactionId={transactionId}"; // iz responsa 
            //dodaj transakcije

            return Ok(new PaymentResponse
            {
                RedirectUrl = redirectUrl,
                TransactionId = transactionId
            });
        }
    }
}
