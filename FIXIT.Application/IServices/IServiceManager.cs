
namespace FIXIT.Application.IServices;

public interface IServiceManager
{
    public IAuthService AuthService { get; }
    public IEmailService EmailService { get; }
    public IWallettService _walletService { get; }
    public IAccountService _accountService { get; }
    public IJobPostService jobPostService { get; }
}
