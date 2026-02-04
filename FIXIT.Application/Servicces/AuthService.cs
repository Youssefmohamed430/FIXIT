using Data_Access_Layer.Helpers;
using FIXIT.Application.DTOs;
using FIXIT.Application.IServices;
using FIXIT.Domain;
using FIXIT.Domain.Entities;
using FIXIT.Domain.Helpers;
using FIXIT.Presentation.Controllers;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;

namespace FIXIT.Application.Servicces;

public class AuthService(UserManager<ApplicationUser> _userManager,IWallettService walletService,
        JWTService _jwtservice, SignInManager<ApplicationUser> _signInManager,IConfiguration _configuration,
        IMemoryCache _cache, ILogger<AuthService> logger, IUnitOfWork unitOfWork,IEmailService emailService)
        : IAuthService
{
    #region Login
    public async Task<AuthModel> Login(LoginDTO loginDTO)
    {
        logger.LogInformation("Login attempt for user {UserName}", loginDTO.UserName);

        var result = await _signInManager.PasswordSignInAsync(
              loginDTO.UserName, loginDTO.Password, isPersistent: false, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            var user = await _userManager.FindByNameAsync(loginDTO.UserName);

            var token = await _jwtservice.CreateJwtToken(user);

            var refreshtoken = await HandleRefreshToken(user, token);

            return new AuthModelFactory()
                .CreateAuthModel(user.Id, user.UserName, user.Email, token.ValidTo,
                    token.Claims.Where(c => c.Type == "roles").Select(c => c.Value).ToList(),
                    new JwtSecurityTokenHandler().WriteToken(token), refreshtoken.Token, EgyptTimeHelper.ConvertFromUtc(refreshtoken.ExpiresOn));
        }
        else if (result.IsLockedOut)
        {
            logger.LogWarning("User {UserName} account locked out", loginDTO.UserName);

            return new AuthModel { Message = "Account locked due to multiple invalid attempts.", IsAuthenticated = false };
        }
        else
        {
            logger.LogWarning("Invalid login for {UserName}", loginDTO.UserName);

            return new AuthModel { Message = "Invalid username or password", IsAuthenticated = false };
        }
    }
    #endregion

    #region Register
    public async Task<AuthModel> Register(RegisterDTO registermodel)
    {
        if (await _userManager.FindByNameAsync(registermodel.Username) is not null)
            return new AuthModel() { Message = "User Name Is Already Registerd" };

        if (await _userManager.FindByEmailAsync(registermodel.Email) is not null)
            return new AuthModel() { Message = "Email Is Already Registerd" };

        await VerificationAccount(registermodel.Email);

        var cacheKey = $"User:{registermodel.Email}";
        _cache.Set(cacheKey, registermodel, TimeSpan.FromMinutes(10));

        return new AuthModel { Message = "Verification code sent to email.", IsAuthenticated = true };
    }
    public async Task<AuthModel> CreateUser(string email)
    {
        try
        {
            unitOfWork.BeginTransaction();

            if (!_cache.TryGetValue($"User:{email}", out RegisterDTO? model))
                return new AuthModel() { Message = "Verification code expired or invalid." };

            var user = model.Adapt<ApplicationUser>();

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = "";

                foreach (var error in result.Errors)
                    errors += $"{error.Description}, ";

                logger.LogError("Failed to create user {Email}. Errors: {Errors}", email, errors);

                return new AuthModel() { Message = errors };
            }

            if(model.Role == "Customer")
            {
                await _userManager.AddToRoleAsync(user, "Customer");
                var customer = user.Adapt<Customer>();
                await unitOfWork.GetRepository<Customer>().AddAsync(customer);
                await unitOfWork.SaveAsync();
                logger.LogInformation("Customer account created successfully for {Email}", email);
                await walletService.CreateWalletForUser
                        (new WalletDTO { UserId = user.Id, ownerType = OwnerType.Customer });
            }
            else
            {
                await _userManager.AddToRoleAsync(user, "Provider");
                var provider = user.Adapt<ServiceProvider>();
                await unitOfWork.GetRepository<ServiceProvider>().AddAsync(provider);
                await unitOfWork.SaveAsync();
                logger.LogInformation("Service Provider account creatted successfully for {Email}",email);
                await walletService.CreateWalletForUser
                                        (new WalletDTO { UserId = user.Id, ownerType = OwnerType.Provider });
            }

            var JWTSecurityToken = await _jwtservice.CreateJwtToken(user);

            user.EmailConfirmed = true;

            var refreshToken = await HandleRefreshToken(user, JWTSecurityToken);

            unitOfWork.Commit();

            return new AuthModelFactory()
                .CreateAuthModel(user.Id, model.Username, model.Email, JWTSecurityToken.ValidTo, new List<string> { "Passenger" }, new JwtSecurityTokenHandler().WriteToken(JWTSecurityToken), refreshToken.Token, refreshToken.ExpiresOn, "Code Verfied successfully!");
        }
        catch (Exception ex)
        {
            unitOfWork.Rollback();
            logger.LogError(ex, "Error creating user for {Email}", email);
            return new AuthModel { Message = "An error occurred while creating the user." };
        }
    }
    #endregion

    #region Forgot And Reset Password
    public async Task<AuthModel> ForgotPassword(string Email)
    {
        var user = await _userManager.FindByEmailAsync(Email);

        if (user == null)
            return new AuthModel { Message = "If this email address is registered with us, password reset instructions will be sent to it." };

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        string htmlBody = HandleForgotEmailBody(Email, token);

        await emailService.SendEmailAsync(user.Email, "Reset Password", htmlBody);

        return new AuthModel { Message = "Reset password link has been sent.", IsAuthenticated = true };
    }

    private string HandleForgotEmailBody(string Email, string token)
    {
        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Waselny.jpg");
        var imageBytes = File.ReadAllBytes(imagePath);
        var base64Image = Convert.ToBase64String(imageBytes);
        var imageDataUrl = $"data:image/jpeg;base64,{base64Image}";


        var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:44382";
        var resetLink = $"{baseUrl}/Auth/ResetPassword?email={Uri.EscapeDataString(Email)}&token={Uri.EscapeDataString(token)}";

        var htmlPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "FogotPasswordEmailTemplate.html");
        var htmlBody = File.ReadAllText(htmlPath);
        htmlBody = htmlBody.Replace("{{LogoImage}}", imageDataUrl);
        htmlBody = htmlBody.Replace("{{ResetLink}}", resetLink);
        return htmlBody;
    }
    public async Task<AuthModel> ResetPassword(ResetPassModelDto resetPassModel)
    {
        var user = await _userManager.FindByEmailAsync(resetPassModel.Email);

        if (user == null)
            return new AuthModel { Message = "Invalid request." };

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetPassModel.token));

        var result = await _userManager.ResetPasswordAsync(user, decodedToken, resetPassModel.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return new AuthModel { Message = errors };
        }

        return new AuthModel { IsAuthenticated = true, Message = "Password reset successfully." };
    }
    #endregion

    #region Verification Code
    private async Task<string?> VerificationAccount(string email)
    {
        var verificationCode = Random.Shared.Next(100000, 999999).ToString();

        var htmlPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "VerificationCodeEmail.html");
        var htmlBody = File.ReadAllText(htmlPath);

        htmlBody = htmlBody.Replace("{{CODE}}", verificationCode);
        htmlBody = htmlBody.Replace("{{DATE}}", DateTime.Now.ToString("yyyy"));

        try
        {
            await emailService.SendEmailAsync(
                email,
                "Verification Account",
                htmlBody
            );

            _cache.Set(email, verificationCode, TimeSpan.FromMinutes(10));

            return verificationCode;
        }
        catch
        {
            return null;
        }
    }
    public bool VerifyCode(string email, string submittedCode)
    {
        if (_cache.TryGetValue(email, out string? code))
        {
            return code == submittedCode;
        }
        return false;
    }
    #endregion

    #region Handle Refresh Token
    private async Task<RefreshToken> HandleRefreshToken(ApplicationUser user, JwtSecurityToken token)
    {
        RefreshToken refreshtoken = null;

        if (user.refreshTokens.Any(t => t.IsActive))
            refreshtoken = user.refreshTokens.FirstOrDefault(t => t.IsActive)!;
        else
        {
            refreshtoken = GenerateRefreshToken();
            user.refreshTokens.Add(refreshtoken);
            await _userManager.UpdateAsync(user);
        }

        return refreshtoken;
    }
    private RefreshToken GenerateRefreshToken()
    {
        var randomNumber = new byte[32];

        RandomNumberGenerator.Fill(randomNumber);

        return new RefreshToken
        {
            Token = Convert.ToBase64String(randomNumber),
            ExpiresOn = EgyptTimeHelper.ConvertToUtc(EgyptTimeHelper.Now).AddDays(10),
            CreatedOn = EgyptTimeHelper.ConvertToUtc(EgyptTimeHelper.Now)
        };
    }
    public async Task<AuthModel> RefreshToken(string token)
    {
        var user = await _userManager.Users
                .Include(u => u.refreshTokens)
                .SingleOrDefaultAsync(u => u.refreshTokens.Any(t => t.Token == token));

        if (user == null)
            return new AuthModel { Message = "Invalid token." };

        var refreshToken = user.refreshTokens.Single(t => t.Token == token);

        if (!refreshToken.IsActive)
            return new AuthModel { Message = "InActive token." };

        refreshToken.RevokedOn = EgyptTimeHelper.ConvertToUtc(EgyptTimeHelper.Now);

        var newRefreshToken = GenerateRefreshToken();
        user.refreshTokens.Add(newRefreshToken);
        await _userManager.UpdateAsync(user);

        var JWTSecurityToken = await _jwtservice.CreateJwtToken(user);

        return new AuthModelFactory()
            .CreateAuthModel(user.Id, user.UserName, user.Email, JWTSecurityToken.ValidTo,
                JWTSecurityToken.Claims.Where(c => c.Type == "roles").Select(c => c.Value).ToList(),
                new JwtSecurityTokenHandler().WriteToken(JWTSecurityToken), newRefreshToken.Token, newRefreshToken.ExpiresOn);
    }
    public async Task<bool> RevokeTokenAsync(string token)
    {
        var user = await _userManager.Users.SingleOrDefaultAsync(u => u.refreshTokens.Any(t => t.Token == token));

        if (user == null)
            return false;

        var refreshToken = user.refreshTokens.Single(t => t.Token == token);

        if (!refreshToken.IsActive)
            return false;

        refreshToken.RevokedOn = DateTime.UtcNow;

        await _userManager.UpdateAsync(user);

        return true;
    }
    #endregion
}
