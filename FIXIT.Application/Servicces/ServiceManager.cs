using FIXIT.Application.IServices;
using FIXIT.Domain.Abstractions;
using FIXIT.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace FIXIT.Application.Servicces;

public class ServiceManager : IServiceManager
{
    private readonly IServiceProvider _serviceProvider;

    public IUnitOfWork UnitOfWork { get; }
    public UserManager<ApplicationUser> Usermanager { get; set; }

    #region Lazy Services
    private readonly Lazy<IAuthService> _authService;
    private readonly Lazy<IEmailService> _emailService;
    private readonly Lazy<IWallettService> _walletService;
    private readonly Lazy<IAccountService> _accountService;
    #endregion

    #region Constructor
    public ServiceManager(
        IServiceProvider serviceProvider,
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager)
    {
        _serviceProvider = serviceProvider;
        Usermanager = userManager;
        UnitOfWork = unitOfWork;

        _authService = new Lazy<IAuthService>(
            () => _serviceProvider.GetRequiredService<IAuthService>()
        );

        _emailService = new Lazy<IEmailService>(
            () => _serviceProvider.GetRequiredService<IEmailService>()
        );

        _walletService = new Lazy<IWallettService>(
            () => _serviceProvider.GetRequiredService<IWallettService>()
        );

        _accountService = new Lazy<IAccountService>(
            () => _serviceProvider.GetRequiredService<IAccountService>()
        );
    }
    #endregion

    #region Service Properties
    public IAuthService AuthService => _authService.Value;
    public IEmailService EmailService => _emailService.Value;
    IAccountService IServiceManager._accountService => _accountService.Value;
    IWallettService IServiceManager._walletService => _walletService.Value;
    #endregion
}