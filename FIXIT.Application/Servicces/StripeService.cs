using FIXIT.Application.IServices;
using FIXIT.Domain.Abstractions;
using FIXIT.Application.DTOs;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;

namespace FIXIT.Application.Services;

public class StripeService : IPaymentGateway
{
    private readonly ILogger<StripeService> _logger;
    private readonly string _secretKey;
    private readonly string _webhookSecret;
    private readonly string _successUrl;
    private readonly string _cancelUrl;
    public PaymentWay paymentWay => PaymentWay.Stripe;


    public StripeService(ILogger<StripeService> logger)
    {
        _logger = logger;
        _secretKey      = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY")!;
        _webhookSecret  = Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET")!;
        _successUrl     = Environment.GetEnvironmentVariable("STRIPE_SUCCESS_URL") ?? "https://yourdomain.com/payment/success";
        _cancelUrl      = Environment.GetEnvironmentVariable("STRIPE_CANCEL_URL")  ?? "https://yourdomain.com/payment/cancel";

        StripeConfiguration.ApiKey = _secretKey;
    }

    /// <summary>
    /// Creates a Stripe Checkout Session and returns the hosted URL.
    /// amountCents is in EGP cents (e.g. 5000 = 50 EGP).
    /// passengerid = userId stored in metadata so we know who paid.
    /// </summary>
    public async Task<string> Pay(int amountCents, string passengerid)
    {
        try
        {
            _logger.LogInformation("Creating Stripe Checkout Session for user {UserId}, amount {Amount} cents", passengerid, amountCents);

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency    = "egp",
                            UnitAmount  = amountCents * 100L, // Stripe expects smallest unit (piastres)
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "FIXIT Wallet Charge"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode       = "payment",
                SuccessUrl = $"{_successUrl}?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl  = _cancelUrl,
                Metadata   = new Dictionary<string, string>
                {
                    { "userId", passengerid }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            _logger.LogInformation("Stripe session created: {SessionId}", session.Id);
            return session.Url; // redirect the user here
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error while creating session");
            throw;
        }
    }

    /// <summary>
    /// Validates the Stripe webhook signature and checks if payment succeeded.
    /// payload  = raw request body (string cast to object from controller)
    /// hmacHeader = value of "Stripe-Signature" header
    /// </summary>
    public Task<bool> RecieveCallback(object payload, string hmacHeader)
    {
        try
        {
            var json  = payload as string
                        ?? throw new ArgumentException("Payload must be the raw JSON string");

            var stripeEvent = EventUtility.ConstructEvent(json, hmacHeader, _webhookSecret);

            _logger.LogInformation("Stripe webhook event received: {EventType}", stripeEvent.Type);

            //if (stripeEvent.Type == Events.PaymentIntentSucceeded)
            //    return Task.FromResult(true);

            _logger.LogWarning("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
            return Task.FromResult(false);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe webhook signature validation failed");
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Extracts the amount in EGP from the Stripe event payload (raw JSON string).
    /// </summary>
    public Task<decimal> ExtractAmountAsync(object payload)
    {
        var json         = payload as string ?? throw new ArgumentException("Payload must be the raw JSON string");
        var stripeEvent  = EventUtility.ConstructEvent(json, null!, _webhookSecret,
                               throwOnApiVersionMismatch: false);

        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
        if (paymentIntent is null)
            throw new InvalidOperationException("Could not extract PaymentIntent from Stripe event");

        // Stripe stores amount in smallest unit (piastres), convert back to EGP
        var amountEgp = paymentIntent.Amount / 100m;
        return Task.FromResult(amountEgp);
    }

    /// <summary>
    /// Extracts the userId stored in PaymentIntent metadata.
    /// </summary>
    public Task<string> ExtractCustomerIdAsync(object payload)
    {
        var json        = payload as string ?? throw new ArgumentException("Payload must be the raw JSON string");
        var stripeEvent = EventUtility.ConstructEvent(json, null!, _webhookSecret,
                              throwOnApiVersionMismatch: false);

        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
        if (paymentIntent is null)
            throw new InvalidOperationException("Could not extract PaymentIntent from Stripe event");

        if (!paymentIntent.Metadata.TryGetValue("userId", out var userId))
            throw new InvalidOperationException("userId not found in Stripe PaymentIntent metadata");

        return Task.FromResult(userId);
    }
}
