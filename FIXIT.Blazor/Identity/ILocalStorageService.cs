namespace FIXIT.Blazor.Identity;

public interface ILocalStorageService
{
    Task<T> GetItemAsync<T>(string key);
    Task SetItemAsync(string key, string value);
    Task RemoveItemAsync(string key);
}
