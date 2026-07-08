
namespace FIXIT.Presentation.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class InvalidatesCacheAttribute(string key, string basePath, string userIdClaim = "uid")
    : Attribute
{
    public string Key { get; } = key;

    public string BasePath { get; } = basePath;

    public string UserIdClaim { get; } = userIdClaim;
}