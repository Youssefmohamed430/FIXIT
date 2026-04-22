namespace FIXIT.Presentation.ServiceRegistration;

public static class RateLimiterServices
{
    public static IServiceCollection AddRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // ── Policy 1: Auth Endpoints (Login / Register / ForgotPassword)
            options.AddFixedWindowLimiter("AuthPolicy", opt =>
            {
                opt.PermitLimit = 5;               // 5 طلبات بس
                opt.Window = TimeSpan.FromMinutes(1); // كل دقيقة
                opt.QueueLimit = 0;                // مفيش قائمة انتظار
            });

            // ── Policy 2: General API Endpoints
            options.AddFixedWindowLimiter("GeneralPolicy", opt =>
            {
                opt.PermitLimit = 30;
                opt.Window = TimeSpan.FromMinutes(1);
                opt.QueueLimit = 5;
            });

            // ── Policy 3: Payment Endpoints (حساسة جداً)
            options.AddFixedWindowLimiter("PaymentPolicy", opt =>
            {
                opt.PermitLimit = 3;
                opt.Window = TimeSpan.FromMinutes(5);
                opt.QueueLimit = 0;
            });

            // ── الـ Response لما يتجاوز الـ Limit
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    await context.HttpContext.Response.WriteAsync(
                        $"Too many requests. Retry after {retryAfter.TotalSeconds} seconds.",
                        cancellationToken);
                }
                else
                {
                    await context.HttpContext.Response.WriteAsync(
                        "Too many requests. Please try again later.",
                        cancellationToken);
                }
            };
        });

        return services;
    }
}
