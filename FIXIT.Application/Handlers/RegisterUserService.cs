using FIXIT.Application.Handlers;
using FIXIT.Application.Servicces;
using FIXIT.Domain.Entities;

namespace FIXIT.Application.Handlers;

public class RegisterUserService
{
    private readonly Dictionary<UserRole, IUserRoleHandler> _handlers;

    public RegisterUserService(IEnumerable<IUserRoleHandler> handlers)
    {
        _handlers = handlers.ToDictionary(h => h.Role);
    }

    public async Task HandleAsync(UserRole role, ApplicationUser user)
    {
        if (!_handlers.TryGetValue(role, out var handler))
            throw new InvalidOperationException($"No handler registered for role {role}");

        await handler.HandleAsync(user);
    }
}
