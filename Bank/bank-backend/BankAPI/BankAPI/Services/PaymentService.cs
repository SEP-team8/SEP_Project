using BankAPI.Context;
using BankAPI.DTOs;
using BankAPI.Helpers;
using BankAPI.Helpers.HmacValidator;
using BankAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace BankAPI.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly BankingDbContext _context;
        private readonly IHmacValidator _hmacValidator;
        public IPspClient _pspClient;

        public PaymentService(
            BankingDbContext context,
            IHmacValidator hmacValidator,
            IPspClient pspClient
        )
        {
            _context = context;
            _hmacValidator = hmacValidator;
            _pspClient = pspClient;
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

        public async Task<PaymentExecutionResult>   ExecuteCardPayment(Guid paymentRequestId, CardPaymentRequest request)
        {
            using var dbTransaction = await _context.Database.BeginTransactionAsync();

            var paymentRequest = await _context.PaymentRequests
                .FirstOrDefaultAsync(p => p.PaymentRequestId == paymentRequestId);

            try
            {
                if (paymentRequest == null)
                    return PaymentExecutionResult.NotFound;

                if (paymentRequest.Status != PaymentRequestStatus.Pending)
                    return PaymentExecutionResult.InvalidState;

                if (paymentRequest.ExpiresAt < DateTime.UtcNow)
                {
                    paymentRequest.Status = PaymentRequestStatus.Expired;
                    await _context.SaveChangesAsync();
                    return PaymentExecutionResult.Expired;
                }

                // Card validation
                if (!LuhnFormulaChecker.IsValidLuhn(request.CardNumber))
                {
                    paymentRequest.Status = PaymentRequestStatus.Failed;
                    await _context.SaveChangesAsync();

                    return PaymentExecutionResult.InvalidCard;
                }

                var card = await _context.Cards
                    .Include(c => c.BankAccount)
                    .FirstOrDefaultAsync(c => c.PAN == request.CardNumber);

                if (card == null)
                    return PaymentExecutionResult.InvalidCard;

                if (IsCardExpired(card.ExpiryMmYy))
                {
                    paymentRequest.Status = PaymentRequestStatus.Failed;
                    await _context.SaveChangesAsync();

                    return PaymentExecutionResult.InvalidCard;
                }

                if (card.BankAccount.Balance < paymentRequest.Amount)
                    return PaymentExecutionResult.InsufficientFunds;

                var merchant = await _context.Merchants
                    .Include(c => c.BankAccount)
                    .FirstOrDefaultAsync(b => b.Id == paymentRequest.MerchantId);

                if (merchant.BankAccount == null)
                    return PaymentExecutionResult.InvalidState;

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

                await _pspClient.NotifyPaymentStatusAsync(new PspPaymentStatusDto
                {
                    PaymentRequestId = paymentRequestId,
                    Stan = paymentRequest.Stan,
                    GlobalTransactionId = globalTransactionId,
                    AcquirerTimestamp = acquirerTimestamp,
                    Status = TransactionStatus.Successfull
                });

                return PaymentExecutionResult.Success;
            }
            catch
            {
                await dbTransaction.RollbackAsync();

                await _pspClient.NotifyPaymentStatusAsync(new PspPaymentStatusDto
                {
                    PaymentRequestId = paymentRequestId,
                    Stan = paymentRequest.Stan,
                    GlobalTransactionId = null,
                    AcquirerTimestamp = DateTime.UtcNow,
                    Status = TransactionStatus.Failed
                });

                return PaymentExecutionResult.InvalidCard;
            }
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
            var psp = await _context.Psps.FindAsync(pspId);
            if (psp == null)
            {
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
                return new InitializePaymentServiceResult
                {
                    Result = InitializePaymentResult.InvalidSignature
                };
            }                

            var merchant = await _context.Merchants.FindAsync(dto.MerchantId);
            if (merchant == null)
            {
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

            return new InitializePaymentServiceResult
            {
                Result = InitializePaymentResult.Success,
                Response = new InitPaymentResponseDto
                {
                    PaymentRequestId = paymentRequest.PaymentRequestId,
                    PaymentRequestUrl = isQrPayment ?
                        $"http://localhost:3000/payCard/{paymentRequest.PaymentRequestId}" :
                        $"http://localhost:3000/payQr/{paymentRequest.PaymentRequestId}"
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
                MerchantAccount = paymentRequest.Merchant.BankAccount.AccountNumber,
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
