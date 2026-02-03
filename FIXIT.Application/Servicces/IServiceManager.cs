using FIXIT.Application.IServices;

namespace FIXIT.Application.Servicces;

public interface IServiceManager
{
    public IAuthService AuthService { get; }
}
