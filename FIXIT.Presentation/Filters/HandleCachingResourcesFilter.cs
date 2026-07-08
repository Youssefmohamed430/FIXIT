
using System.Security.Claims;

namespace FIXIT.Presentation;

public class HandleCachingResourcesFilter(IMemoryCache cache, ILogger<HandleCachingResourcesFilter> logger) : IResourceFilter
{
    private const string CacheHitFlag = "X-CacheHit";
    private string GetCacheKey(HttpContext httpContext, CacheableAttribute cacheAttr)
    {
        var path = httpContext.Request.Path.ToString();
        var query = httpContext.Request.QueryString.ToString();
        return $"{cacheAttr.Key}:{path}{query}";
    }

    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        var endpoint = context.HttpContext.GetEndpoint();
        if (endpoint == null) return;

        var cacheAttr = endpoint.Metadata.GetMetadata<CacheableAttribute>();
        if (cacheAttr == null) return;

        var key = GetCacheKey(context.HttpContext, cacheAttr);

        if (cache.TryGetValue(key, out object cachedData))
        {
            context.Result = new OkObjectResult(cachedData);
            context.HttpContext.Items[CacheHitFlag] = true;
        }

        logger.LogInformation("Checked cache for key: {CacheKey}", key);
    }

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
        var endpoint = context.HttpContext.GetEndpoint();
        if (endpoint == null) return;

        var cacheAttr = endpoint.Metadata.GetMetadata<CacheableAttribute>();
        if (cacheAttr != null)
        {
            bool isCacheHit = context.HttpContext.Items.ContainsKey(CacheHitFlag);

            if (!isCacheHit && context.Result is OkObjectResult okResult)
            {
                var key = GetCacheKey(context.HttpContext, cacheAttr);
                cache.Set(key, okResult.Value, TimeSpan.FromMinutes(10));
                logger.LogInformation("Resource executed and cached with key: {CacheKey}", key);
            }
        }

        var invalidateAttrs = endpoint.Metadata
            .GetOrderedMetadata<InvalidatesCacheAttribute>();

        if (invalidateAttrs.Count > 0 && context.Result is OkObjectResult)
        {
            foreach (var attr in invalidateAttrs)
            {
                var userId = context.HttpContext.User
                    .FindFirstValue(attr.UserIdClaim);

                if (string.IsNullOrEmpty(userId)) continue;

                var keyToRemove = $"{attr.Key}:{attr.BasePath}/{userId}";
                cache.Remove(keyToRemove);
                logger.LogInformation("Cache invalidated for key: {CacheKey}", keyToRemove);
            }
        }

    }
}