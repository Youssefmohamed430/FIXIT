
namespace FIXIT.Application.IServices;

public interface IAuthService
{
    //Task<AuthModel> Login(LoginDTO loginDTO);
    Task<(AuthModel? authmodel, string token, string? refreshtoken, DateTime RefreshTokenExpiration)> Login(LoginDTO loginDTO);
    Task<AuthModel> Register(RegisterDTO registermodel);
    Task<AuthModel> ForgotPassword(string Email);
    Task<AuthModel> ResetPassword(ResetPassModelDto resetPassModel);
    bool VerifyCode(string email, string submittedCode);
    Task<AuthModel> CreateUser(string email);
    Task<AuthModel> RefreshToken(string token);
    Task<bool> RevokeTokenAsync(string token);
    Task<string> ResendCode(string email);
}
