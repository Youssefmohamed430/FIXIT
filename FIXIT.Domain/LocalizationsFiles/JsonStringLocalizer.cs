namespace FIXIT.Domain.LocalizationsFiles;

public class JsonStringLocalizer : IStringLocalizer
{
    private readonly JsonSerializer _serializer = new();

    // نحسب المسار من مكان الـ Assembly مش من الـ working directory
    private static string GetFilePath(string cultureName)
    {
        var basePath = AppContext.BaseDirectory;
        return Path.Combine(basePath, "Resources", $"{cultureName}.json");
    }

    public LocalizedString this[string name]
    {
        get
        {
            var value = GetString(name);
            return new LocalizedString(name, value ?? name, value == null);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var actualValue = this[name];
            return !actualValue.ResourceNotFound
                ? new LocalizedString(name, string.Format(actualValue.Value, arguments))
                : actualValue;
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        var filePath = GetFilePath(Thread.CurrentThread.CurrentCulture.Name);

        if (!File.Exists(filePath))
            yield break;

        using var filestream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var streamReader = new StreamReader(filestream);
        using var reader = new JsonTextReader(streamReader);

        while (reader.Read())
        {
            if (reader.TokenType != JsonToken.PropertyName) continue;
            var key = reader.Value as string;
            reader.Read();
            var value = _serializer.Deserialize<string>(reader)!;
            yield return new LocalizedString(key!, value);
        }
    }

    private string? GetString(string key)
    {
        var cultureName = Thread.CurrentThread.CurrentCulture.Name;
        var filePath = GetFilePath(cultureName);

        // Fallback إلى en-US لو اللغة المطلوبة مش موجودة
        if (!File.Exists(filePath))
            filePath = GetFilePath("en-US");

        if (!File.Exists(filePath))
            return null;

        return GetValueFromJson(key, filePath);
    }

    private string? GetValueFromJson(string propertyName, string filepath)
    {
        if (string.IsNullOrEmpty(filepath) || string.IsNullOrEmpty(propertyName))
            return null;

        using var filestream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
        using var streamReader = new StreamReader(filestream);
        using var reader = new JsonTextReader(streamReader);

        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.PropertyName
                && reader.Value as string == propertyName)
            {
                reader.Read();
                return _serializer.Deserialize<string>(reader);
            }
        }

        return null;
    }
}