using CryptoService.DTOs;
using CryptoService.Models;
using CryptoService.Persistance;
using CryptoService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Nethereum.Signer;
using Nethereum.Util;
using Nethereum.Web3;
using QRCoder;
using System.Numerics;
using System.Text.Json;

namespace CryptoService.Services
{
    using NethereumWallet = Nethereum.HdWallet.Wallet;
    using WalletAddressModel = CryptoService.Models.WalletModels.WalletAddress;
    using WalletModel = CryptoService.Models.WalletModels.Wallet;

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

        public async Task<CreateCryptoPaymentResponse> CreatePaymentAsync(CreateCryptoPaymentRequest request, CancellationToken ct)
        {
            if (request.MerchantId == Guid.Empty)
                throw new ArgumentException("MerchantId is required");

            if (request.FiatAmount <= 0)
                throw new ArgumentException("Invalid fiat amount");

            if (string.IsNullOrWhiteSpace(request.Stan))
                throw new ArgumentException("Stan is required");

            if (!Enum.IsDefined(typeof(Currency), request.Currency))
                throw new ArgumentException("Invalid currency value");

            var currency = (Currency)request.Currency;

            decimal fallbackEthPriceUsd = 3000m;

            decimal? ethPriceUsd = null;
            decimal? ethPriceEur = null;
            decimal? ethPriceRsd = null;

            try
            {
                using var http = new HttpClient();
                var cgUrl = "https://api.coingecko.com/api/v3/simple/price?ids=ethereum&vs_currencies=usd,eur,rsd";
                using var cgResp = await http.GetAsync(cgUrl, ct);
                if (cgResp.IsSuccessStatusCode)
                {
                    using var stream = await cgResp.Content.ReadAsStreamAsync(ct);
                    using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
                    if (doc.RootElement.TryGetProperty("ethereum", out var ethEl))
                    {
                        if (ethEl.TryGetProperty("usd", out var u) && u.ValueKind != JsonValueKind.Null)
                            ethPriceUsd = u.GetDecimal();
                        if (ethEl.TryGetProperty("eur", out var e) && e.ValueKind != JsonValueKind.Null)
                            ethPriceEur = e.GetDecimal();
                        if (ethEl.TryGetProperty("rsd", out var r) && r.ValueKind != JsonValueKind.Null)
                            ethPriceRsd = r.GetDecimal();
                    }
                }
            }
            catch
            {
            }

            decimal fiatInUsd;
            if (currency == Currency.USD)
            {
                fiatInUsd = request.FiatAmount;
            }
            else if (currency == Currency.EUR)
            {
                if (ethPriceEur.HasValue && ethPriceUsd.HasValue)
                {
                    var rateEurToUsd = ethPriceUsd.Value / ethPriceEur.Value;
                    fiatInUsd = request.FiatAmount * rateEurToUsd;
                }
                else
                {
                    try
                    {
                        using var http = new HttpClient();
                        var convUrl = $"https://api.exchangerate.host/convert?from=EUR&to=USD&amount={request.FiatAmount}";
                        using var convResp = await http.GetAsync(convUrl, ct);
                        if (convResp.IsSuccessStatusCode)
                        {
                            using var stream = await convResp.Content.ReadAsStreamAsync(ct);
                            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
                            if (doc.RootElement.TryGetProperty("result", out var res) && res.ValueKind != JsonValueKind.Null)
                            {
                                fiatInUsd = res.GetDecimal();
                            }
                            else fiatInUsd = request.FiatAmount * 1.1m;
                        }
                        else fiatInUsd = request.FiatAmount * 1.1m;
                    }
                    catch
                    {
                        fiatInUsd = request.FiatAmount * 1.1m;
                    }
                }
            }
            else if (currency == Currency.RSD)
            {
                if (ethPriceRsd.HasValue)
                {
                    if (ethPriceUsd.HasValue)
                    {
                        var rateRsdToUsd = ethPriceUsd.Value / ethPriceRsd.Value;
                        fiatInUsd = request.FiatAmount * rateRsdToUsd;
                    }
                    else
                    {
                        try
                        {
                            using var http = new HttpClient();
                            var convUrl = $"https://api.exchangerate.host/convert?from=RSD&to=USD&amount={request.FiatAmount}";
                            using var convResp = await http.GetAsync(convUrl, ct);
                            if (convResp.IsSuccessStatusCode)
                            {
                                using var stream = await convResp.Content.ReadAsStreamAsync(ct);
                                using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
                                if (doc.RootElement.TryGetProperty("result", out var res) && res.ValueKind != JsonValueKind.Null)
                                {
                                    fiatInUsd = res.GetDecimal();
                                }
                                else fiatInUsd = request.FiatAmount / 110m; // legacy fallback
                            }
                            else fiatInUsd = request.FiatAmount / 110m;
                        }
                        catch
                        {
                            fiatInUsd = request.FiatAmount / 110m;
                        }
                    }
                }
                else
                {
                    try
                    {
                        using var http = new HttpClient();
                        var convUrl = $"https://api.exchangerate.host/convert?from=RSD&to=USD&amount={request.FiatAmount}";
                        using var convResp = await http.GetAsync(convUrl, ct);
                        if (convResp.IsSuccessStatusCode)
                        {
                            using var stream = await convResp.Content.ReadAsStreamAsync(ct);
                            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
                            if (doc.RootElement.TryGetProperty("result", out var res) && res.ValueKind != JsonValueKind.Null)
                            {
                                fiatInUsd = res.GetDecimal();
                            }
                            else fiatInUsd = request.FiatAmount / 110m;
                        }
                        else fiatInUsd = request.FiatAmount / 110m;
                    }
                    catch
                    {
                        fiatInUsd = request.FiatAmount / 110m;
                    }
                }
            }
            else
            {
                fiatInUsd = request.FiatAmount;
            }

            decimal ethAmount;
            try
            {
                var priceUsd = ethPriceUsd ?? fallbackEthPriceUsd;
                ethAmount = Math.Round(fiatInUsd / priceUsd, 18);
            }
            catch
            {
                ethAmount = 0m;
            }

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

            var chainId = int.Parse(_config["Ethereum:ChainId"] ?? "11155111");

            return new CreateCryptoPaymentResponse
            {
                PaymentId = payment.Id,
                EthAddress = payment.EthAddress,
                EthAmount = payment.EthAmount,
                ExpiresAt = payment.ExpiresAt,
                ChainId = chainId
            };
        }


        private async Task<string> GeneratePaymentAddress(CancellationToken ct)
        {
            var mnemonic = _config["Ethereum:WalletMnemonic"];

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

        public async Task<CryptoPaymentStatusResponse?> SubmitTransactionAsync(SubmitCryptoTxDto dto, CancellationToken ct)
        {
            if (dto == null) throw new ArgumentException("dto required");
            if (dto.PaymentId == Guid.Empty || string.IsNullOrWhiteSpace(dto.TxHash))
                throw new ArgumentException("Missing paymentId or txHash");

            var payment = await _db.CryptoPayments.FindAsync(new object[] { dto.PaymentId }, ct);
            if (payment == null) return null;

            var web3 = new Web3(_rpcUrl);

            var txHash = dto.TxHash.StartsWith("0x") ? dto.TxHash : "0x" + dto.TxHash;

            payment.TransactionHash = txHash;
            await _db.SaveChangesAsync(ct);

            try
            {
                var tx = await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(txHash);

                var receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txHash);

                if (tx == null && receipt == null)
                {
                    return new CryptoPaymentStatusResponse(
                        payment.Id,
                        payment.Status,
                        payment.EthAmount,
                        payment.TransactionHash,
                        _confirmations
                    );
                }

                long confirmations = 0;
                if (receipt != null && receipt.BlockNumber != null)
                {
                    var latest = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
                    confirmations = (long)(latest.Value - receipt.BlockNumber.Value) + 1;
                }

                var toAddress = (tx?.To ?? string.Empty).ToLowerInvariant();
                var expectedTo = (payment.EthAddress ?? string.Empty).ToLowerInvariant();
                var txValue = tx?.Value ?? BigInteger.Zero;
                var expected = BigInteger.Parse(payment.AmountWei ?? "0");

                var validTo = toAddress == expectedTo;
                var validValue = txValue >= expected;

                if (validTo && validValue && confirmations >= _confirmations)
                {
                    payment.Status = CryptoPaymentStatus.Confirmed;
                    payment.TransactionHash = txHash;
                    payment.PspTimestamp = DateTime.UtcNow;
                    await _db.SaveChangesAsync(ct);
                }
                else if (validTo && validValue)
                {
                    payment.Status = CryptoPaymentStatus.Detected;
                    payment.TransactionHash = txHash;
                    await _db.SaveChangesAsync(ct);
                }
                else
                {
                    payment.TransactionHash = txHash;
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
            catch (Exception)
            {
                await _db.SaveChangesAsync(ct);
                throw;
            }
        }

    }
}
