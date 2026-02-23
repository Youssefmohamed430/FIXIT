
namespace FIXIT.Domain.ValueObjects;

public sealed class ImgPath : ValueObject
{
    public string Value { get; }
    private ImgPath()
    {
        
    }
    private ImgPath(string value)
    {
        Value = value;
    }

    public static ImgPath Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Image path cannot be empty.");

        if (!IsValidImageExtension(value))
            throw new ArgumentException("Invalid image format.");

        return new ImgPath(value);
    }

    private static bool IsValidImageExtension(string path)
    {
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        return allowedExtensions.Any(ext =>
            path.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
