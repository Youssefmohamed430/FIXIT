
namespace FIXIT.Application.Handlers;

public class CustomerRoleHandler
    (UserManager<ApplicationUser> userManager,IUnitOfWork unitOfWork,IWallettService walletService,ILogger<CustomerRoleHandler> logger) : IUserRoleHandler
{
    public UserRole Role => UserRole.Customer;

    public async Task HandleAsync(ApplicationUser user)
    {
        await userManager.AddToRoleAsync(user, "Customer");

        var entity = user.Adapt<Customer>();
        await unitOfWork.GetRepository<Customer>().AddAsync(entity);
        await unitOfWork.SaveAsync();

        await walletService.CreateWalletForUser(
            new WalletDTO
            {
                UserId = user.Id,
                ownerType = OwnerType.Customer
            });

        logger.LogInformation("Customer account created successfully for {Email}", user.Email);
    }
}
