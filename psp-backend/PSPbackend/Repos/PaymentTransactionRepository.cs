using Microsoft.EntityFrameworkCore;
using PSPbackend.Context;
using PSPbackend.Models;

namespace PSPbackend.Repos
{
    public class PaymentTransactionRepository : IPaymentTransactionRepository
    {
        private readonly PspDbContext _db;
        public PaymentTransactionRepository(PspDbContext db)
        {
            _db = db;
        }

        //public async Task CreateAsync(PaymentTransaction tx, CancellationToken ct)
        //{
        //    _db.PaymentTransactions.Add(tx);
        //    await _db.SaveChangesAsync(ct);
        //}

        //public Task<PaymentTransaction?> GetByBankPaymentRequestIdAsync(Guid paymentRequestId, CancellationToken ct)
        //{
        //    return _db.PaymentTransactions
        //        .FirstOrDefaultAsync(x => x.BankPaymentRequestId == paymentRequestId, ct);
        //}

        //public async Task UpdateAsync(PaymentTransaction tx, CancellationToken ct)
        //{
        //    tx.UpdatedAtUtc = DateTime.UtcNow;
        //    _db.PaymentTransactions.Update(tx);
        //    await _db.SaveChangesAsync(ct);
        //}
    }
}
