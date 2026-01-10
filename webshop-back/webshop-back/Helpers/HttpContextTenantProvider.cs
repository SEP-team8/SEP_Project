using webshop_back.Data.Models;

namespace webshop_back.Helpers
{
    public class HttpContextTenantProvider : ITenantProvider
    {
        private readonly IHttpContextAccessor _ctx;
        public HttpContextTenantProvider(IHttpContextAccessor ctx) { _ctx = ctx; }
        public Merchant? Current => _ctx.HttpContext?.Items["Merchant"] as Merchant;
        public Guid? CurrentMerchantId => Current?.MerchantId;
    }
}
