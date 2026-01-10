using webshop_back.Data.Models;

namespace webshop_back.Helpers
{
    public interface ITenantProvider
    {
        Merchant? Current { get; }
        Guid? CurrentMerchantId { get; }
    }
}
