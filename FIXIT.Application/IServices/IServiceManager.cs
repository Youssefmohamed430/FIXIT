using FIXIT.Application.Servicces;

namespace FIXIT.Application.IServices;

public interface IServiceManager
{
    public IAuthService AuthService { get; }
    public IEmailService EmailService { get; }
}
