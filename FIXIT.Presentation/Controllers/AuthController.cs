using FIXIT.Application.DTOs;
using FIXIT.Application.IServices;
using FIXIT.Application.Servicces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FIXIT.Presentation.Controllers;

[Route("[controller]")]
[ApiController]
public class AuthController
    (IServiceManager serviceManager,AppDbContext dbContext) : ControllerBase
{
    [HttpPost("LogIn")]
    public async Task<IActionResult> LogInAsync(LoginDTO model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await serviceManager.AuthService.Login(model);

        if (!string.IsNullOrEmpty(result.RefreshToken))
            SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

        return result.IsAuthenticated ? Ok(result) : BadRequest(result.Message);
    }
    [HttpPost("Register")]
    public async Task<IActionResult> Register(RegisterDTO model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await serviceManager.AuthService.Register(model);

        return result.IsAuthenticated ? Ok(result) : BadRequest(result.Message);
    }
    [HttpPost("ForgetPassword/{Email}")]
    public async Task<IActionResult> ForgetPassword(string Email)
    {
        var result = await serviceManager.AuthService.ForgotPassword(Email);

        return result.IsAuthenticated ? Ok(result) : BadRequest(result.Message);
    }
    [HttpPost("ResetPassword")]
    public async Task<IActionResult> ResetPassword(ResetPassModelDto resetPassModel)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await serviceManager.AuthService.ResetPassword(resetPassModel);

        return result.IsAuthenticated ? Ok(result.Message) : BadRequest(result.Message);
    }
    [HttpPost("VerifyCode/{submittedCode}")]
    public async Task<IActionResult> VerifyCode([FromQuery] string email, string submittedCode)
    {
        var result = serviceManager.AuthService.VerifyCode(email, submittedCode);

        if (result)
        {
            var Userresult = await serviceManager.AuthService.CreateUser(email);

            if(Userresult.IsAuthenticated == false)
                return BadRequest(Userresult.Message); 

            SetRefreshTokenInCookie(Userresult.RefreshToken, Userresult.RefreshTokenExpiration);

            return Ok(Userresult);
        }
        else
            return BadRequest(new { Message = "Invalid verification code." });
    }

    [HttpPost("RefreshToken")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrEmpty(request.Token))
            return Unauthorized(new { Message = "No refresh token provided." });

        var result = await serviceManager.AuthService.RefreshToken(request.Token);

        if (!result.IsAuthenticated)
            return BadRequest(result.Message);

        if (!string.IsNullOrEmpty(result.RefreshToken))
            SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

        return result.IsAuthenticated ? Ok(result) : Unauthorized(result.Message);

    }
    [HttpPost("revokeToken")]
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
