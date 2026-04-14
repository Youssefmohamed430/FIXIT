
using Microsoft.Extensions.Logging;

namespace FIXIT.Domain.Helpers;

public class JWTService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JWT _jwt;
    private readonly ILogger<JWTService> _logger;
    public JWTService(UserManager<ApplicationUser> userManager, IOptions<JWT> jwt, ILogger<JWTService> logger)
    {
        this._userManager = userManager;
        this._jwt = jwt.Value;
        this._logger = logger;
    }
    public async Task<JwtSecurityToken> CreateJwtToken(ApplicationUser user)
    {
        var userClaims = await _userManager.GetClaimsAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        var roleClaims = new List<Claim>();

        foreach (var role in roles)
            roleClaims.Add(new Claim("roles", role));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("uid", user.Id)
        }
        .Union(userClaims)
        .Union(roleClaims);

        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8
            .GetBytes(Environment.GetEnvironmentVariable("JWTKey")!));
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        _logger.LogInformation("Creating JWT token for user {UserId} with claims: {Claims} ,Expiration Time For JwtToken is {expireson}", user.Id, claims.Select(c => new { c.Type, c.Value }),_jwt.DurationInSeconds);

        var jwtSecurityToken = new JwtSecurityToken
            (
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddSeconds(_jwt.DurationInSeconds),
                signingCredentials: signingCredentials
            );

        return jwtSecurityToken;
    }
}
