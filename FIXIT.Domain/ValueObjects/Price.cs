namespace FIXIT.Domain.ValueObjects;

public sealed class Price : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Price()
    {}

    private Price(decimal amount, string _currency)
    {
        Amount = amount;
        Currency = _currency;
    }

    public static Price Create(decimal amount, string currency = "EGP")
    {
        if (amount < 0)
            throw new ArgumentException("Price must be greater than zero.");

        return new Price(
            Math.Round(amount, 2),
            currency
        );
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public Price ApplyCommission(decimal percentage)
    {
        if (percentage < 0 || percentage > 100)
            throw new ArgumentException("Invalid commission.");

        var commissionAmount = Amount * (percentage / 100);
        return Create(commissionAmount, Currency);
    }

    public Price ApplyProviderAmount(decimal percentage)
    {
        if (percentage < 0 || percentage > 100)
            throw new ArgumentException("Invalid percentage.");

        var ProviderAmount = Amount * (percentage / 100);
        return Create(ProviderAmount, Currency);
    }

}
