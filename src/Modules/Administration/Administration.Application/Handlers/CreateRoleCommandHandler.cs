namespace Administration.Application.Handlers;

using Administration.Application.Commands;
using Administration.Application.Ports;
using Shared.Application.Cqrs;
using Shared.Application.Time;
using Shared.ErrorHandling;

internal sealed class CreateRoleCommandHandler(IAdminRbacRepository repository, ISystemClock clock)
    : ICommandHandler<CreateRoleCommand, AdminRoleDetails>
{
    public async Task<Result<AdminRoleDetails>> HandleAsync(CreateRoleCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return Result.Failure<AdminRoleDetails>(AdministrationApplicationErrors.RoleNameRequired);
        }

        if (!AdminRoleName.TryNormalize(command.Name, out string? roleName))
        {
            return Result.Failure<AdminRoleDetails>(AdministrationApplicationErrors.RoleNameInvalid);
        }

        if (await repository.RoleExistsAsync(roleName, cancellationToken).ConfigureAwait(false))
        {
            return Result.Failure<AdminRoleDetails>(AdministrationApplicationErrors.RoleAlreadyExists);
        }

        AdminRoleDetails role = await repository.CreateRoleAsync(roleName, clock.UtcNow, cancellationToken).ConfigureAwait(false);

        return Result.Success(role);
    }
}
