using webshop_back.Data.Models;
using webshop_back.Service.Interfaces;

public class TenantResolverMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolverMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext ctx, IRepository repo)
    {
        var header = ctx.Request.Headers["X-Merchant-Id"].FirstOrDefault();
        Merchant? m = null;

        if (!string.IsNullOrEmpty(header))
        {
            m = repo.GetMerchantByMerchantId(header);
        }

        if (m == null)
        {
            var host = ctx.Request.Host.Host; // ex: shop1.myapp.local
            m = repo.GetMerchantByDomain(host);
        }

        if (m != null)
        {
            ctx.Items["Merchant"] = m;
        }

        await _next(ctx);
    }
}
