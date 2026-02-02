using System.Numerics;
using CryptoService.Persistance;
using Microsoft.EntityFrameworkCore;
using Nethereum.Web3;

namespace CryptoService.HostedServices
{
    public class PaymentWatcher : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<PaymentWatcher> _logger;
        private readonly string _rpcUrl;

        public PaymentWatcher(
            IServiceProvider services,
            IConfiguration config,
            ILogger<PaymentWatcher> logger)
        {
            _services = services;
            _logger = logger;
            _rpcUrl = config["Ethereum:RpcUrl"]!;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            _logger.LogInformation("PaymentWatcher started");

            while (!token.IsCancellationRequested)
            {
                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<CryptoDbContext>();
                var web3 = new Web3(_rpcUrl);

                var pending = await db.CryptoPayments
                    .Where(p => p.Status == Models.CryptoPaymentStatus.Pending)
                    .ToListAsync(token);

                foreach (var p in pending)
                {
                    try
                    {
                        var balance = await web3.Eth.GetBalance.SendRequestAsync(p.EthAddress);
                        if (balance >= BigInteger.Parse(p.AmountWei))
                        {
                            p.Status = Models.CryptoPaymentStatus.Detected;
                            _logger.LogInformation("ETH received for {id}", p.Id);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning(e, "Watcher error");
                    }
                }

                await db.SaveChangesAsync(token);
                await Task.Delay(30_000, token);
            }
        }
    }
}
