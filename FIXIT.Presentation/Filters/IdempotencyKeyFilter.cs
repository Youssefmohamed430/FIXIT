namespace FIXIT.Presentation.Filters;

public class IdempotencyKeyFilter(IMemoryCache cache) : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var keys = cache.GetOrCreate("IdempotencyKeys", entry =>
        {
            return new HashSet<string>();
        });

        var key = context.HttpContext.Request.Headers["Idempotency-Key"].ToString();

        if(string.IsNullOrEmpty(key))
        {
            context.Result = new BadRequestObjectResult("Idempotency-Key header is required.");
            return;
        }

        if (keys.Contains(key))
        {
            context.Result = new BadRequestObjectResult("Duplicate request detected. This operation has already been performed.");
            return;
        }
        else
        {
            keys.Add(key);
            cache.Set("IdempotencyKeys", keys, TimeSpan.FromHours(1));
        }
    }
}

