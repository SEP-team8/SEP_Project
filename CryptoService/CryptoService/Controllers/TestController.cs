using CryptoService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CryptoService.Controllers;

[ApiController]
[Route("crypto/[controller]")]
public class TestController : ControllerBase
{
    private readonly ICryptoPaymentService _cryptoPaymentService;

    public TestController(ICryptoPaymentService cryptoPaymentService)
    {
        _cryptoPaymentService = cryptoPaymentService;
    }

    [HttpGet("ping")]
    public IActionResult Ping() => Ok("CryptoService is alive");

    [HttpPost("setup/generate-shop-wallet")]
    public async Task<ActionResult> GenerateShopWallet(CancellationToken cancellationToken)
    {
        var (wif, address) = await _cryptoPaymentService.GenerateShopWalletAsync(cancellationToken);
        return Ok(new { WIF = wif, Address = address });
    }
}
