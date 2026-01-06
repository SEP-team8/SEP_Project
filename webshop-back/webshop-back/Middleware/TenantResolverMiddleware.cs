using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using webshop_back.Data.Models;
using webshop_back.Service.Interfaces;

public class TenantResolverMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolverMiddleware> _logger;
    private readonly IConfiguration _config;

    public TenantResolverMiddleware(RequestDelegate next,
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
            Merchant? m = null;

            // 1) Try header (dev / explicit)
            var header = ctx.Request.Headers["X-Merchant-Id"].FirstOrDefault();
            if (!string.IsNullOrEmpty(header))
            {
                _logger.LogDebug("TenantResolver: X-Merchant-Id header present: {Header}", header);
                m = repo.GetMerchantByMerchantId(header);
                if (m != null)
                {
                    _logger.LogInformation("TenantResolver: resolved merchant by header X-Merchant-Id={MerchantId}", m.MerchantId);
                }
                else
                {
                    _logger.LogWarning("TenantResolver: X-Merchant-Id header provided but no merchant found: {Header}", header);
                }
            }

            // 2) Try host (api.{shop}.localhost or shop domain)
            if (m == null)
            {
                var host = ctx.Request.Host.Host ?? string.Empty;
                host = host.Trim().ToLowerInvariant(); // normalize

                _logger.LogDebug("TenantResolver: resolving by host {Host}", host);

                // attempt exact domain match
                m = repo.GetMerchantByDomain(host);
                if (m != null)
                {
                    _logger.LogInformation("TenantResolver: resolved merchant by host {Host} -> {MerchantId}", host, m.MerchantId);
                }
                else
                {
                    _logger.LogDebug("TenantResolver: no merchant found for host {Host}", host);
                }
            }

            // 3) Fallback: use configured default merchant for dev/localhost (if present)
            if (m == null)
            {
                var host = ctx.Request.Host.Host ?? string.Empty;
                if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                    host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase))
                {
                    var defaultMerchant = _config["Dev:DefaultMerchantId"];
                    if (!string.IsNullOrEmpty(defaultMerchant))
                    {
                        _logger.LogDebug("TenantResolver: attempting fallback to Dev:DefaultMerchantId={DefaultMerchant}", defaultMerchant);
                        m = repo.GetMerchantByMerchantId(defaultMerchant);
                        if (m != null)
                        {
                            _logger.LogInformation("TenantResolver: resolved merchant by Dev:DefaultMerchantId -> {MerchantId}", m.MerchantId);
                        }
                        else
                        {
                            _logger.LogWarning("TenantResolver: Dev:DefaultMerchantId provided but merchant not found: {DefaultMerchant}", defaultMerchant);
                        }
                    }
                }
            }

            // 4) Attach to context (or leave null)
            if (m != null)
            {
                ctx.Items["Merchant"] = m;
            }
            else
            {
                _logger.LogWarning("TenantResolver: no merchant resolved for request {Method} {Path} (Host: {Host})",
                    ctx.Request.Method, ctx.Request.Path, ctx.Request.Host);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TenantResolver: unexpected error while resolving tenant");
            // do not throw — let request continue (controllers can handle missing merchant)
        }

        await _next(ctx);
    }
}
