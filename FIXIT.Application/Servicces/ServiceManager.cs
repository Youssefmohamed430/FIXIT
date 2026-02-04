using FIXIT.Application.IServices;
using FIXIT.Domain.Abstractions;
using FIXIT.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIXIT.Application.Servicces;

public class ServiceManager
    (IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
    : IServiceManager
{

    private readonly Lazy<IAuthService> _authService;
    private readonly Lazy<IEmailService> _emailService;

    public IAuthService AuthService => _authService.Value;
    public IEmailService EmailService => _emailService.Value;
}
