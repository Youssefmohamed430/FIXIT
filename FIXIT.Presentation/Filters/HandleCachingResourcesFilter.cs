
namespace FIXIT.Presentation;

public class HandleCachingResourcesFilter(IMemoryCache cache, ILogger<HandleCachingResourcesFilter> logger) : IResourceFilter
{
    // استخرج الـ key بناءً على الـ attribute + الـ request path
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
        }

        logger.LogInformation("Checked cache for key: {CacheKey}", key);
    }

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
        var endpoint = context.HttpContext.GetEndpoint();
        if (endpoint == null) return;

        var cacheAttr = endpoint.Metadata.GetMetadata<CacheableAttribute>();
        if (cacheAttr == null) return;

        var key = GetCacheKey(context.HttpContext, cacheAttr);

        if (context.Result is OkObjectResult okResult)
        {
            cache.Set(key, okResult.Value, TimeSpan.FromMinutes(10));
        }

        logger.LogInformation("Resource executed and cached with key: {CacheKey}", key);
    }
}