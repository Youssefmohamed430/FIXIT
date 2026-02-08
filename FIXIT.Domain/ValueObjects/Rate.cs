namespace FIXIT.Domain.ValueObjects;

public sealed class Rate : ValueObject
{
    public decimal Value { get; }

    private Rate()
    {
        
    }
    private Rate(decimal value)
    {
        Value = value;
    }

    public static Rate Create(decimal value)
    {
        if (value < 1 || value > 5 || value % 0.5m != 0)
            throw new ArgumentException("Rate must be between 1 and 5 with 0.5 steps.");

        return new Rate(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static Rate Average(IEnumerable<Rate> rates)
    {
        var avg = rates.Average(r => r.Value);
        return Create((int)Math.Round(avg));
    }

}
