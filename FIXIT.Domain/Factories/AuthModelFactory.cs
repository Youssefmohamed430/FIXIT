using FIXIT.Domain.Abstractions;

namespace FIXIT.Domain.Factories
{
    public class AuthModelFactory
    {
        public AuthModel CreateAuthModel(string id, string username, string email, DateTime expiresOn,List<string> roles, string JWTSecurityToken,string refreshToken,DateTime refreshTokenExpiration, string Message = "")
        {
            return new AuthModel()
            {
                Id = id,
                Username = username,
                Email = email,
                IsAuthenticated = true,
                //ExpiresOn = expiresOn,
                Roles = roles,
                Token = JWTSecurityToken,
                RefreshToken = refreshToken,
                RefreshTokenExpiration = EgyptTimeHelper.ConvertFromUtc(refreshTokenExpiration),
            };
        }
    }
}
