using FIXIT.Application.IServices;
using FIXIT.Domain.Abstractions;
using FIXIT.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIXIT.Application.Servicces;

public class ServiceManager: IServiceManager
{
    public IUnitOfWork UnitOfWork { get; }
    public UserManager<ApplicationUser> Usermanager { get; set; }

    private readonly Lazy<IAuthService> _authService;
    private readonly Lazy<IEmailService> _emailService;


    public ServiceManager(IServiceProvider serviceProvider, IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
    {
        Usermanager = userManager;
        UnitOfWork = unitOfWork;

        _authService = new Lazy<IAuthService>(
            () => serviceProvider.GetRequiredService<IAuthService>()
        );
        _emailService = new Lazy<IEmailService>(
            () => serviceProvider.GetRequiredService<IEmailService>()
        );
    }

    public IAuthService AuthService => _authService.Value;
    public IEmailService EmailService => _emailService.Value;
}
