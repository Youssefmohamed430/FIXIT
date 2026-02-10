namespace FIXIT.Blazor.Models;

public class AuthResult
{
    public bool IsAuthenticated { get; set; }
    public string Token { get; set; }
    public string RefreshToken { get; set; }
    public string Message { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
}
