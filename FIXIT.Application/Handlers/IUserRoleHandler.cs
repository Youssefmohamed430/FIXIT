
namespace FIXIT.Application.Handlers;

public interface IUserRoleHandler
{
    UserRole Role { get; }
    Task HandleAsync(ApplicationUser user);
}
