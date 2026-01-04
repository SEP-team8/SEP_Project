using webshop_back.Data.Models;
using webshop_back.Service.Interfaces;

namespace webshop_back.Middleware
{
    public class TenantResolverMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantResolverMiddleware> _logger;

        public TenantResolverMiddleware(RequestDelegate next, ILogger<TenantResolverMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext ctx, IRepository repo)
        {
            Merchant? m = null;

            // 1) try header X-Merchant-Id
            var header = ctx.Request.Headers["X-Merchant-Id"].FirstOrDefault();
            if (!string.IsNullOrEmpty(header))
            {
                m = repo.GetMerchantByMerchantId(header);
                if (m == null)
                {
                    _logger.LogDebug("TenantResolver: header X-Merchant-Id provided but merchant not found or inactive: {Header}", header);
                }
            }

            // 2) fallback: resolve by host/subdomain (if stored Domain)
            if (m == null)
            {
                var host = ctx.Request.Host.Host; // e.g. shop1.myapp.local
                if (!string.IsNullOrEmpty(host))
                {
                    m = repo.GetMerchantByDomain(host);
                    if (m == null)
                    {
                        _logger.LogDebug("TenantResolver: no merchant matched by domain {Host}", host);
                    }
                }
            }

            if (m != null)
            {
                ctx.Items["Merchant"] = m;
                _logger.LogDebug("TenantResolver: resolved merchant {MerchantId} for request", m.MerchantId);
            }
            else
            {
                // No merchant resolved — that's OK for many endpoints, but you may require merchant later.
                ctx.Items["Merchant"] = null;
            }

            await _next(ctx);
        }
    }
}
