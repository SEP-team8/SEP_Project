using CryptoService.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CryptoService.Controllers
{
    [Route("crypto/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ICryptoPaymentService _cryptoPaymentService;

        public TestController(ICryptoPaymentService cryptoPaymentService)
        {
            _cryptoPaymentService = cryptoPaymentService;
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("CryptoService is alive");
        }

        [HttpPost("setup/generate-shop-wallet")]
        public async Task<ActionResult> GenerateShopWallet()
        {
            var (wif, address) = await _cryptoPaymentService.GenerateShopWalletAsync();

            return Ok(new
            {
                WIF = wif,
                Address = address,
                Instructions = "1. Copy WIF to appsettings.json, 2. You don't need to fund this address (customers pay TO it)"
            });
        }
    }
}
