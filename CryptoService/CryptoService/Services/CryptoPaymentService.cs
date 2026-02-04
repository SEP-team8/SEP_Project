using System.Numerics;
using CryptoService.DTOs;
using CryptoService.Models;
using CryptoService.Persistance;
using CryptoService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
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
                ?? throw new InvalidOperationException("Ethereum:RpcUrl missing");

            _confirmations = int.Parse(
                config["Ethereum:ConfirmationsRequired"] ?? "1"
            );
        }

        public async Task<CreateCryptoPaymentResponse> CreatePaymentAsync(
            CreateCryptoPaymentRequest request,
            CancellationToken ct)
        {
            if (request.MerchantId == Guid.Empty)
                throw new ArgumentException("MerchantId is required");

            if (request.FiatAmount <= 0)
                throw new ArgumentException("Invalid fiat amount");

            if (string.IsNullOrWhiteSpace(request.Stan))
                throw new ArgumentException("Stan is required");

            // map numeric currency → enum
            if (!Enum.IsDefined(typeof(Currency), request.Currency))
                throw new ArgumentException("Invalid currency value");

            var currency = (Currency)request.Currency;

            // DEV: fiksna cena (kasnije oracle)
            decimal ethPriceUsd = 3000m;
            decimal fiatInUsd = request.FiatAmount;

            switch (currency)
            {
                case Currency.RSD:
                    fiatInUsd = request.FiatAmount / 110m; // fake kurs
                    break;

                case Currency.EUR:
                    fiatInUsd = request.FiatAmount * 1.1m;
                    break;

                case Currency.USD:
                    fiatInUsd = request.FiatAmount;
                    break;
            }

            decimal ethAmount = Math.Round(fiatInUsd / ethPriceUsd, 18);
            BigInteger wei = UnitConversion.Convert.ToWei(ethAmount);

            var ethAddress = await GeneratePaymentAddress(ct);

            var payment = new CryptoPayment
            {
                Id = Guid.NewGuid(),

                // === PSP correlation data ===
                MerchantId = request.MerchantId,
                Stan = request.Stan,
                PspTimestamp = DateTime.SpecifyKind(
                    request.PspTimestamp,
                    DateTimeKind.Utc
                ),

                FiatAmount = request.FiatAmount,
                FiatCurrency = currency,

                EthAmount = ethAmount,
                AmountWei = wei.ToString(),
                EthAddress = ethAddress,

                Status = CryptoPaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(20)
            };

            _db.CryptoPayments.Add(payment);
            await _db.SaveChangesAsync(ct);

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

            // fallback → statička adresa (DEV / demo)
            if (string.IsNullOrWhiteSpace(mnemonic))
                return _config["Ethereum:ShopAddress"]
                    ?? throw new InvalidOperationException("Shop address missing");

            var wallet = await _db.Wallets
                .FirstOrDefaultAsync(w => w.IsActive, ct);

            if (wallet == null)
            {
                wallet = new WalletModel
                {
                    Id = Guid.NewGuid(),
                    IsActive = true,
                    CurrentAddressIndex = 0,
                    CreatedAt = DateTime.UtcNow
                };

                _db.Wallets.Add(wallet);
                await _db.SaveChangesAsync(ct);
            }

            wallet.CurrentAddressIndex++;
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
            if (payment == null)
                return null;

            var web3 = new Web3(_rpcUrl);
            var balance = await web3.Eth.GetBalance
                .SendRequestAsync(payment.EthAddress);

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
                _confirmations
            );
        }

        public Task<CryptoPaymentStatusResponse?> GetStatusAsync(
            Guid id,
            CancellationToken ct)
            => CheckPaymentStatusAsync(id, ct);

        public async Task<byte[]> GeneratePaymentQrCodeAsync(
            Guid paymentId,
            CancellationToken ct)
        {
            var payment = await _db.CryptoPayments.FindAsync(paymentId);
            if (payment == null)
                throw new Exception("Payment not found");

            // EIP-681
            string uri =
                $"ethereum:{payment.EthAddress}?value={payment.AmountWei}";

            using var gen = new QRCodeGenerator();
            using var data = gen.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q);
            using var qr = new PngByteQRCode(data);
            return qr.GetGraphic(20);
        }

        public Task<GenerateWalletResponse> GenerateShopWalletAsync(
            CancellationToken ct)
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
