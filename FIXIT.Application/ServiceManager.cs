using FIXIT.Application.IServices;
using FIXIT.Application.Servicces;
using FIXIT.Domain;
using FIXIT.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIXIT.Application;

public class ServiceManager
    (IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
    : IServiceManager
{

    private readonly Lazy<IAuthService> _authService;

    public IAuthService AuthService => _authService.Value;
}
