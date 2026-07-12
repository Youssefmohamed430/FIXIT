
namespace FIXIT.Presentation.Controllers;

[Route("[controller]")]
[ApiController]
[EnableRateLimiting("AuthPolicy")]

public class AuthController
    (IServiceManager serviceManager,AppDbContext dbContext) : ControllerBase
{
    [HttpPost("LogIn")]
    public async Task<IActionResult> LogInAsync(LoginDTO model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        (var result , string token , string RefreshToken , DateTime RefreshTokenExpiration) = await serviceManager?.AuthService?.Login(model)!;

        if (!string.IsNullOrEmpty(RefreshToken))
            SetRefreshTokenInCookie(RefreshToken, RefreshTokenExpiration);

        return result == null ? Ok(new { Token = token }) : BadRequest(result);
    }
    [HttpPost("Register")]
    public async Task<IActionResult> Register(RegisterDTO model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await serviceManager.AuthService.Register(model);

        return result.IsAuthenticated ? Ok(result) : BadRequest(result);
    }
    [HttpPost("ForgetPassword/{Email}")]
    public async Task<IActionResult> ForgetPassword(string Email)
    {
        var result = await serviceManager.AuthService.ForgotPassword(Email);

        return result.IsAuthenticated ? Ok(result) : BadRequest(result);
    }
    [HttpPost("ResetPassword")]
    public async Task<IActionResult> ResetPassword(ResetPassModelDto resetPassModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await serviceManager.AuthService.ResetPassword(resetPassModel);

        return result.IsAuthenticated ? Ok(result) : BadRequest(result);
    }
    [HttpPost("VerifyCode/{submittedCode}")]
    public async Task<IActionResult> VerifyCode([FromQuery] string email, string submittedCode)
    {
        var result = serviceManager.AuthService.VerifyCode(email, submittedCode);

        if (result)
        {
            var Userresult = await serviceManager.AuthService.CreateUser(email);

            if(Userresult.IsAuthenticated == false)
                return BadRequest(Userresult); 

            SetRefreshTokenInCookie(Userresult.RefreshToken, Userresult.RefreshTokenExpiration);

            return Ok(Userresult);
        }
        else
            return BadRequest(Result<object>.Failure(new Error("Invalid verification code.")));
    }

    [HttpPost("RefreshToken")]
    [EnableRateLimiting("GeneralPolicy")]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];

        var result = await serviceManager.AuthService.RefreshToken(refreshToken);

        if (!result.IsAuthenticated)
            return BadRequest(result);

        if (!string.IsNullOrEmpty(result.RefreshToken))
            SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

        return result.IsAuthenticated ? Ok(result) : Unauthorized(result);

    }
    [HttpPost("revokeToken")]
    [DisableRateLimiting]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeToken model)
    {
        var token = model.Token ?? Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(token))
            return BadRequest("Token is required!");

        var result = await serviceManager.AuthService.RevokeTokenAsync(token);

        if (!result)
            return BadRequest("Token is invalid!");

        return Ok();
    }

    [HttpGet("ResendCode")]
    public async Task<IActionResult> ResendCode([FromQuery] string email)
    { 
        var result = await serviceManager.AuthService.ResendCode(email);
        return result != "" ? Ok(result) : BadRequest("Failed to resend code!");
    }


    private void SetRefreshTokenInCookie(string refreshToken, DateTime expires)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = expires.ToLocalTime(),
            Secure = true,
            IsEssential = true,
            SameSite = SameSiteMode.None
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }
}
