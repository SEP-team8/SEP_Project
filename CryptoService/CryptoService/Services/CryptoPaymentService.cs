using System.Numerics;
using CryptoService.DTOs;
using CryptoService.Models;
using CryptoService.Models.WalletModels;
using CryptoService.Persistance;
using CryptoService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Nethereum.HdWallet;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Signer;
using QRCoder;

namespace CryptoService.Services
{
    using WalletModel = CryptoService.Models.WalletModels.Wallet;
    using WalletAddressModel = CryptoService.Models.WalletModels.WalletAddress;
    using NethereumWallet = Nethereum.HdWallet.Wallet;

    public class CryptoPaymentService : ICryptoPaymentService
    {
        private readonly CryptoDbContext _db;
        private readonly IConfiguration _config;
        private readonly string _rpcUrl;
        private readonly int _confirmations;

        public CryptoPaymentService(
            CryptoDbContext db,
            IConfiguration config)
        {
            _db = db;
            _config = config;

            _rpcUrl = config["Ethereum:RpcUrl"]
                ?? throw new InvalidOperationException("Ethereum RpcUrl missing");

            _confirmations = int.Parse(config["Ethereum:ConfirmationsRequired"] ?? "1");
        }

        public async Task<CreateCryptoPaymentResponse> CreatePaymentAsync(
            CreateCryptoPaymentRequest request,
            CancellationToken cancellationToken)
        {
            if (request.FiatAmount <= 0)
                throw new ArgumentException("Invalid amount");

            // DEV: fiksna cena (ili ubaci oracle kasnije)
            decimal ethPriceUsd = 3000m;
            decimal ethAmount = Math.Round(request.FiatAmount / ethPriceUsd, 18);

            BigInteger wei = UnitConversion.Convert.ToWei(ethAmount);

            string address = await GeneratePaymentAddress(cancellationToken);

            var payment = new CryptoPayment
            {
                Id = Guid.NewGuid(),
                OrderId = request.OrderId,
                FiatAmount = request.FiatAmount,
                FiatCurrency = request.FiatCurrency,
                EthAmount = ethAmount,
                AmountWei = wei.ToString(),
                EthAddress = address,
                Status = CryptoPaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(20)
            };

            _db.CryptoPayments.Add(payment);
            await _db.SaveChangesAsync(cancellationToken);

            return new CreateCryptoPaymentResponse(
                payment.Id,
                payment.EthAddress,
                payment.EthAmount,
                payment.ExpiresAt
            );
        }

        private async Task<string> GeneratePaymentAddress(CancellationToken ct)
        {
            var mnemonic = _config["Ethereum:WalletMnemonic"];
            if (string.IsNullOrWhiteSpace(mnemonic))
                return _config["Ethereum:ShopAddress"]!;

            var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.IsActive, ct)
                ?? new WalletModel
                {
                    Id = Guid.NewGuid(),
                    IsActive = true,
                    CurrentAddressIndex = 0,
                    CreatedAt = DateTime.UtcNow
                };

            wallet.CurrentAddressIndex++;
            _db.Wallets.Update(wallet);
            await _db.SaveChangesAsync(ct);

            var hd = new NethereumWallet(mnemonic, null);
            var account = hd.GetAccount(wallet.CurrentAddressIndex);

            _db.WalletAddresses.Add(new WalletAddressModel
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                Address = account.Address,
                DerivationIndex = wallet.CurrentAddressIndex,
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync(ct);

            return account.Address;
        }

        public async Task<CryptoPaymentStatusResponse?> CheckPaymentStatusAsync(
            Guid paymentId,
            CancellationToken ct)
        {
            var payment = await _db.CryptoPayments.FindAsync(paymentId);
            if (payment == null) return null;

            var web3 = new Web3(_rpcUrl);
            var balance = await web3.Eth.GetBalance.SendRequestAsync(payment.EthAddress);

            if (balance >= BigInteger.Parse(payment.AmountWei))
            {
                payment.Status = CryptoPaymentStatus.Detected;
                await _db.SaveChangesAsync(ct);
            }

            return new CryptoPaymentStatusResponse(
                payment.Id,
                payment.Status,
                payment.EthAmount,
                payment.TransactionHash,
                0
            );
        }

        public async Task<byte[]> GeneratePaymentQrCodeAsync(
            Guid paymentId,
            CancellationToken ct)
        {
            var payment = await _db.CryptoPayments.FindAsync(paymentId);
            if (payment == null)
                throw new Exception("Payment not found");

            string uri = $"ethereum:{payment.EthAddress}?value={payment.EthAmount}";

            using var gen = new QRCodeGenerator();
            using var data = gen.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q);
            using var qr = new PngByteQRCode(data);
            return qr.GetGraphic(20);
        }

        public Task<CryptoPaymentStatusResponse?> GetStatusAsync(Guid id, CancellationToken ct)
            => CheckPaymentStatusAsync(id, ct);

        public Task<GenerateWalletResponse> GenerateShopWalletAsync(CancellationToken ct)
        {
            var key = EthECKey.GenerateKey();
            return Task.FromResult(
                new GenerateWalletResponse(
                    key.GetPrivateKey(),
                    key.GetPublicAddress()
                )
            );
        }
    }
}
