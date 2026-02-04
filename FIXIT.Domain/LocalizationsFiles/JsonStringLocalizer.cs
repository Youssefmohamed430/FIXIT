namespace FIXIT.Domain.LocalizationsFiles;

public class JsonStringLocalizer : IStringLocalizer
{
    private readonly JsonSerializer _serializer = new();
    public LocalizedString this[string name]
    {
        get
        {
            var value = GetString(name);
            return new LocalizedString(name, value);
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
        var filePath = Path.GetFullPath($"Resources/{Thread.CurrentThread.CurrentCulture.Name}.json");

        using FileStream filestream = new(filePath, FileMode.Open, FileAccess.Read);
        using StreamReader streamReader = new(filestream);
        using JsonTextReader reader = new(streamReader);

        while (reader.Read())
        {
            if (reader.TokenType != JsonToken.PropertyName)
                continue;

            var key = reader.Value as string;

            reader.Read();

            var value = _serializer.Deserialize<string>(reader)!;

            yield return new LocalizedString(key!, value);
        }
    }

    private string GetString(string Key)
    {
        var filePath = Path.GetFullPath($"Resources/{Thread.CurrentThread.CurrentCulture.Name}.json");
        
        return File.Exists(filePath) ? GetValueFromJson(Key,filePath) : string.Empty;
    }

    private string GetValueFromJson(string PropertyName,string filepath)
    {
        if(string.IsNullOrEmpty(filepath) || string.IsNullOrEmpty(PropertyName))
            return string.Empty;

        using FileStream filestream = new(filepath, FileMode.Open, FileAccess.Read);
        using StreamReader streamReader = new(filestream);
        using JsonTextReader reader = new(streamReader);


        while(reader.Read())
        {
            if(reader.TokenType == JsonToken.PropertyName && reader.Value as string == PropertyName)
            {
                reader.Read();
                return _serializer.Deserialize<string>(reader)!;
            }
        }

        return string.Empty;
    }
}
