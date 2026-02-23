
namespace FIXIT.Application.Handlers;

public class ProviderRoleHandler(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork, IWallettService walletService, ILogger<CustomerRoleHandler> logger) : IUserRoleHandler
{
    public UserRole Role => UserRole.Provider;
    public async Task HandleAsync(ApplicationUser user)
    {
        await userManager.AddToRoleAsync(user, "Provider");
        var entity = user.Adapt<ServiceProvider>();
        await unitOfWork.GetRepository<ServiceProvider>().AddAsync(entity);
        await unitOfWork.SaveAsync();
        await walletService.CreateWalletForUser(
            new WalletDTO
            {
                UserId = user.Id,
                ownerType = OwnerType.Provider
            });
        logger.LogInformation("Provider account created successfully for {Email}", user.Email);
    }
}
