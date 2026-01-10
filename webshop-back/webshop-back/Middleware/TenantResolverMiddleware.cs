using webshop_back.Data.Models;
using webshop_back.Service.Interfaces;

public class TenantResolverMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolverMiddleware> _logger;
    private readonly IConfiguration _config;

    public TenantResolverMiddleware(
        RequestDelegate next,
        ILogger<TenantResolverMiddleware> logger,
        IConfiguration config)
    {
        _next = next;
        _logger = logger;
        _config = config;
    }

    public async Task Invoke(HttpContext ctx, IRepository repo)
    {
        try
        {
            Merchant? merchant = null;

            // 1) Resolve by X-Merchant-Id header (GUID)
            var header = ctx.Request.Headers["X-Merchant-Id"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(header))
            {
                if (Guid.TryParse(header, out var merchantId))
                {
                    _logger.LogDebug(
                        "TenantResolver: X-Merchant-Id header present: {MerchantId}",
                        merchantId);

                    merchant = repo.GetMerchantByMerchantId(merchantId);

                    if (merchant != null)
                    {
                        _logger.LogInformation(
                            "TenantResolver: resolved merchant by header X-Merchant-Id={MerchantId}",
                            merchant.MerchantId);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "TenantResolver: X-Merchant-Id provided but merchant not found: {MerchantId}",
                            merchantId);
                    }
                }
                else
                {
                    _logger.LogWarning(
                        "TenantResolver: invalid X-Merchant-Id header (not a GUID): {Header}",
                        header);
                    Console.WriteLine("______________________________________TenantResolver: invalid X-Merchant-Id header (not a GUID): {Header}______________________",
                        header);
                }
            }

            // 2) Resolve by host/domain
            if (merchant == null)
            {
                var host = (ctx.Request.Host.Host ?? string.Empty)
                    .Trim()
                    .ToLowerInvariant();

                _logger.LogDebug("TenantResolver: resolving by host {Host}", host);

                merchant = repo.GetMerchantByDomain(host);

                if (merchant != null)
                {
                    _logger.LogInformation(
                        "TenantResolver: resolved merchant by host {Host} -> {MerchantId}",
                        host,
                        merchant.MerchantId);
                }
                else
                {
                    _logger.LogDebug(
                        "TenantResolver: no merchant found for host {Host}",
                        host);
                }
            }

            // 3) Dev fallback (localhost only)
            if (merchant == null)
            {
                var host = ctx.Request.Host.Host ?? string.Empty;

                if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                    host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase))
                {
                    var defaultMerchantValue = _config["Dev:DefaultMerchantId"];

                    if (!string.IsNullOrWhiteSpace(defaultMerchantValue) &&
                        Guid.TryParse(defaultMerchantValue, out var defaultMerchantId))
                    {
                        _logger.LogDebug(
                            "TenantResolver: attempting fallback to Dev:DefaultMerchantId={MerchantId}",
                            defaultMerchantId);

                        merchant = repo.GetMerchantByMerchantId(defaultMerchantId);

                        if (merchant != null)
                        {
                            _logger.LogInformation(
                                "TenantResolver: resolved merchant by Dev:DefaultMerchantId -> {MerchantId}",
                                merchant.MerchantId);
                        }
                        else
                        {
                            _logger.LogWarning(
                                "TenantResolver: Dev:DefaultMerchantId provided but merchant not found: {MerchantId}",
                                defaultMerchantId);
                        }
                    }
                }
            }

            if (merchant != null)
            {
                ctx.Items["Merchant"] = merchant;
            }
            else
            {
                _logger.LogWarning(
                    "TenantResolver: no merchant resolved for request {Method} {Path} (Host: {Host})",
                    ctx.Request.Method,
                    ctx.Request.Path,
                    ctx.Request.Host);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TenantResolver: unexpected error while resolving tenant");
        }

        await _next(ctx);
    }
}
