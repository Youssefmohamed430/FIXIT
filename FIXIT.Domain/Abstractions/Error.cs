namespace FIXIT.Domain.Abstractions;
public sealed record Error(string code, string Descriprion = null!)
{
    public static readonly Error None = new(string.Empty);
}


