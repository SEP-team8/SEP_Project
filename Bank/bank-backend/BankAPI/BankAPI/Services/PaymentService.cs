using BankAPI.Context;
using BankAPI.DTOs;
using BankAPI.Helpers;
using BankAPI.Helpers.HmacValidator;
using BankAPI.Models;
using BankAPI.Services.CardProtector;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace BankAPI.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly BankingDbContext _context;
        private readonly IHmacValidator _hmacValidator;
        private readonly IPspClient _pspClient;
        private readonly ICardProtector _cardProtector;
        private readonly Microsoft.Extensions.Logging.ILogger<PaymentService> _logger;

        public PaymentService(
            BankingDbContext context,
            IHmacValidator hmacValidator,
            IPspClient pspClient,
            ICardProtector cardProtector,
            Microsoft.Extensions.Logging.ILogger<PaymentService> logger
        )
        {
            _context = context;
            _hmacValidator = hmacValidator;
            _pspClient = pspClient;
            _cardProtector = cardProtector;
            _logger = logger;
        }

        private bool IsCardExpired(string expiry)
        {
            if (!DateTime.TryParseExact(
                expiry,
                "MM/yy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var expiryDate))
            {
                return true;
            }

            var lastDayOfMonth = new DateTime(
                expiryDate.Year,
                expiryDate.Month,
                DateTime.DaysInMonth(expiryDate.Year, expiryDate.Month)
            );

            return lastDayOfMonth < DateTime.UtcNow.Date;
        }

        public async Task<string> ExecuteCardPayment(Guid paymentRequestId, CardPaymentRequest request)
        {
            _logger.LogInformation("ExecuteCardPayment started for paymentRequestId {PaymentRequestId}", paymentRequestId);

            using var dbTransaction = await _context.Database.BeginTransactionAsync();

            var paymentRequest = await _context.PaymentRequests
                .FirstOrDefaultAsync(p => p.PaymentRequestId == paymentRequestId);

            if (paymentRequest == null)
            {
                _logger.LogWarning("PaymentRequest not found for paymentRequestId {PaymentRequestId}", paymentRequestId);
                return await NotifyFailure(paymentRequestId, TransactionStatus.Failed);
            }

            if (paymentRequest.Status != PaymentRequestStatus.Pending)
            {
                _logger.LogWarning("PaymentRequest status is not Pending for paymentRequestId {PaymentRequestId} - status: {Status}", paymentRequestId, paymentRequest.Status);
                return await NotifyFailure(paymentRequestId, TransactionStatus.Failed);
            }

            if (paymentRequest.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("PaymentRequest expired for paymentRequestId {PaymentRequestId}", paymentRequestId);
                paymentRequest.Status = PaymentRequestStatus.Expired;
                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();
                return await NotifyFailure(paymentRequestId, TransactionStatus.Failed);
            }

            if (string.IsNullOrWhiteSpace(request.Cvv)
                || request.Cvv.Length != 3
                || !request.Cvv.All(char.IsDigit))
            {
                _logger.LogWarning("Invalid CVV for paymentRequestId {PaymentRequestId}", paymentRequestId);
                paymentRequest.Status = PaymentRequestStatus.Failed;
                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return await NotifyFailure(paymentRequestId, TransactionStatus.Failed);
            }

            // Card validation
            if (!LuhnFormulaChecker.IsValidLuhn(request.CardNumber))
            {
                _logger.LogWarning("Invalid card number (Luhn check failed) for paymentRequestId {PaymentRequestId}", paymentRequestId);
                paymentRequest.Status = PaymentRequestStatus.Failed;
                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return await NotifyFailure(paymentRequestId, TransactionStatus.Failed);
            }

            // Compute PAN hash and lookup by hash to avoid storing/ comparing raw PAN values.
            var panHash = _cardProtector.ComputePanHash(request.CardNumber);

            var card = await _context.Cards
                .Include(c => c.BankAccount)
                .FirstOrDefaultAsync(c => c.PanHash == panHash);

            if (card == null)
            {
                _logger.LogWarning("Card not found for paymentRequestId {PaymentRequestId}", paymentRequestId);
                return await NotifyFailure(paymentRequestId, TransactionStatus.Failed);
            }

            if (IsCardExpired(card.ExpiryMmYy))
            {
                _logger.LogWarning("Card expired for paymentRequestId {PaymentRequestId}", paymentRequestId);
                paymentRequest.Status = PaymentRequestStatus.Failed;
                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return await NotifyFailure(paymentRequestId, TransactionStatus.Failed);
            }

            // Unprotect stored CVV and compare. Handle legacy plaintext values by attempting
            // to unprotect and falling back to plaintext comparison. If plaintext is detected,
            // re-protect it and persist the encrypted value.
            string storedCvv;
            bool wasPlaintext = false;
            try
            {
                storedCvv = _cardProtector.UnprotectCvv(card.EncryptedCvv);
            }
            catch
            {
                // Assume stored value is plaintext CVV from pre-migration state
                storedCvv = card.EncryptedCvv;
                wasPlaintext = true;
            }

            if (storedCvv != request.Cvv)
            {
                paymentRequest.Status = PaymentRequestStatus.Failed;
                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                _logger.LogWarning("Payment failed validation for paymentRequestId {PaymentRequestId} - card last4 {PanLast4}", paymentRequestId, card.PanLast4);

                return await NotifyFailure(paymentRequestId, TransactionStatus.Failed);
            }

            if (wasPlaintext)
            {
                // Protect and persist the CVV
                card.EncryptedCvv = _cardProtector.ProtectCvv(storedCvv);
                _context.Cards.Update(card);
                await _context.SaveChangesAsync();
            }

            if (card.BankAccount.Balance < paymentRequest.Amount)
            {
                _logger.LogWarning("Insufficient balance for paymentRequestId {PaymentRequestId} - required: {Amount}, balance: {Balance}", paymentRequestId, paymentRequest.Amount, card.BankAccount.Balance);
                paymentRequest.Status = PaymentRequestStatus.Failed;
                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return await NotifyFailure(paymentRequestId, TransactionStatus.Failed);
            }

            var merchant = await _context.Merchants
                .Include(c => c.BankAccount)
                .FirstOrDefaultAsync(b => b.Id == paymentRequest.MerchantId);


            if (merchant.BankAccount == null)
            {
                _logger.LogWarning("Merchant bank account not found for paymentRequestId {PaymentRequestId}", paymentRequestId);
                paymentRequest.Status = PaymentRequestStatus.Failed;
                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return await NotifyFailure(paymentRequestId, TransactionStatus.Failed);
            }

            try
            {
                card.BankAccount.Balance -= paymentRequest.Amount;
                merchant.BankAccount.Balance += paymentRequest.Amount;

                var globalTransactionId = Guid.NewGuid();
                var acquirerTimestamp = DateTime.UtcNow;

                _context.Transactions.Add(new Transaction
                {
                    PaymentRequestId = paymentRequestId,
                    GlobalTransactionId = globalTransactionId,
                    AcquirerTimestamp = acquirerTimestamp,
                    Status = TransactionStatus.Successfull
                });

                paymentRequest.Status = PaymentRequestStatus.Success;

                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                _logger.LogInformation("Payment successful for paymentRequestId {PaymentRequestId} - amount {Amount} {Currency} - card last4 {PanLast4}", paymentRequestId, paymentRequest.Amount, paymentRequest.Currency, card.PanLast4);

                var redirectUrl = await _pspClient.NotifyPaymentStatusAsync(new PspPaymentStatusDto
                {
                    PaymentRequestId = paymentRequestId,
                    Stan = paymentRequest.Stan,
                    GlobalTransactionId = globalTransactionId,
                    AcquirerTimestamp = acquirerTimestamp,
                    Status = TransactionStatus.Successfull,
                    MerchantID = merchant.Id,
                    PspTimestamp = paymentRequest.PspTimestamp,
                });

                return redirectUrl;
            }
            catch
            {
                await dbTransaction.RollbackAsync();

                var globalTransactionId = Guid.NewGuid();
                var acquirerTimestamp = DateTime.UtcNow;

                _context.Transactions.Add(new Transaction
                {
                    PaymentRequestId = paymentRequestId,
                    GlobalTransactionId = globalTransactionId,
                    AcquirerTimestamp = acquirerTimestamp,
                    Status = TransactionStatus.Failed
                });

                await _context.SaveChangesAsync();

                return await NotifyFailure(paymentRequestId, TransactionStatus.Failed);
            }
        }


        private async Task<string> NotifyFailure(Guid paymentRequestId, TransactionStatus status)
        {
            var paymentRequest = await _context.PaymentRequests.FindAsync(paymentRequestId);

            var redirectUrl = await _pspClient.NotifyPaymentStatusAsync(new PspPaymentStatusDto
            {
                PaymentRequestId = paymentRequestId,
                Stan = paymentRequest?.Stan!,
                GlobalTransactionId = Guid.NewGuid(),
                AcquirerTimestamp = DateTime.UtcNow,
                Status = status,
                MerchantID = paymentRequest?.MerchantId ?? Guid.Empty,
                PspTimestamp = paymentRequest?.PspTimestamp ?? DateTime.UtcNow
            });

            return redirectUrl;
        }

        public async Task<PaymentRequestDto> GetPaymentRequest(Guid paymentRequestId)
        {
            var paymentRequest = await _context.PaymentRequests
                .Where(p => p.PaymentRequestId == paymentRequestId)
                .Select(p => new PaymentRequestDto()
                {
                    PaymentRequestId = p.PaymentRequestId,
                    Amount = p.Amount,
                    Currency = p.Currency.ToString(),
                    ExpiresAt = p.ExpiresAt,
                    Status = p.Status
                })
                .FirstOrDefaultAsync();

            if (paymentRequest == null)
            {
                throw new Exception("Payment request not found.");
            }

            if (paymentRequest.Status != PaymentRequestStatus.Pending)
            {
                throw new Exception("Payment request is not valid.");
            }

            if (paymentRequest.ExpiresAt < DateTime.UtcNow)
            {
                throw new Exception("Payment request expired.");
            }

            return paymentRequest;
        }

        public async Task<InitializePaymentServiceResult> InitializePayment(InitPaymentRequestDto dto, Guid pspId, string signature, DateTime timestamp, bool isQrPayment)
        {
            _logger.LogInformation("InitializePayment started - PspId: {PspId}, MerchantId: {MerchantId}, Amount: {Amount}", pspId, dto.MerchantId, dto.Amount);

            var psp = await _context.Psps.FindAsync(pspId);
            if (psp == null)
            {
                _logger.LogWarning("Invalid PSP - PspId: {PspId}", pspId);
                return new InitializePaymentServiceResult
                {
                    Result = InitializePaymentResult.InvalidPsp
                };
            }
               
            var payload =
                $"merchantId={dto.MerchantId}&amount={dto.Amount}" +
                $"&currency={(int)dto.Currency}&stan={dto.Stan}" +
                $"&timestamp={timestamp:o}";

            if (!_hmacValidator.Validate(payload, signature, psp.HMACKey))
            {
                _logger.LogWarning("Invalid signature for InitializePayment - MerchantId: {MerchantId}", dto.MerchantId);
                return new InitializePaymentServiceResult
                {
                    Result = InitializePaymentResult.InvalidSignature
                };
            }                

            var merchant = await _context.Merchants.FindAsync(dto.MerchantId);
            if (merchant == null)
            {
                _logger.LogWarning("Merchant not found - MerchantId: {MerchantId}", dto.MerchantId);
                return new InitializePaymentServiceResult
                {
                    Result = InitializePaymentResult.InvalidMerchant
                };
            }                

            var paymentRequest = new PaymentRequest
            {
                PaymentRequestId = Guid.NewGuid(),
                MerchantId = dto.MerchantId,
                PspId = pspId,
                Amount = dto.Amount,
                Currency = dto.Currency,
                Stan = dto.Stan,
                PspTimestamp = dto.PspTimestamp,
                Status = PaymentRequestStatus.Pending,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };

            _context.PaymentRequests.Add(paymentRequest);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Payment request initialized successfully - PaymentRequestId: {PaymentRequestId}, Amount: {Amount} {Currency}", paymentRequest.PaymentRequestId, dto.Amount, dto.Currency);

            return new InitializePaymentServiceResult
            {
                Result = InitializePaymentResult.Success,
                Response = new InitPaymentResponseDto
                {
                    PaymentRequestId = paymentRequest.PaymentRequestId,
                    PaymentRequestUrl = isQrPayment ?
                        $"http://localhost:3000/payQr/{paymentRequest.PaymentRequestId}" :
                        $"http://localhost:3000/payCard/{paymentRequest.PaymentRequestId}"
                }
            };
        }

        public async Task<QRPaymentResponseDto> GenerateQrPayment(Guid paymentRequestId)
        {
            var paymentRequest = await _context.PaymentRequests
                .Include(p => p.Merchant)
                .ThenInclude(m => m.BankAccount)
                .FirstOrDefaultAsync(p => p.PaymentRequestId == paymentRequestId);

            if (paymentRequest == null)
                throw new Exception("Payment request not found");

            if (paymentRequest.Status != PaymentRequestStatus.Pending)
                throw new Exception("Payment request not valid");

            if (paymentRequest.ExpiresAt < DateTime.UtcNow)
                throw new Exception("Payment request expired");

            var ipsData = new IpsQrData
            {
                Amount = paymentRequest.Amount,
                Currency = paymentRequest.Currency.ToString(),
                MerchantName = paymentRequest.Merchant.Name,
                MerchantAccount = paymentRequest.Merchant.BankAccount.AccountNumber.Replace("-", ""),
                Purpose = "Placanje robe"
            };

            var payload = IpsQrPayloadGenerator.Generate(ipsData);
            var qrBase64 = QrImageGenerator.GenerateBase64(payload);

            return new QRPaymentResponseDto
            {
                PaymentRequestId = paymentRequestId,
                QrCodeBase64 = qrBase64
            };
        }
    }
}
