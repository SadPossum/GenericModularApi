namespace Administration.Application.Validation;

using Administration.Application.Commands;
using Shared.Administration;
using Shared.Application.Cqrs;
using Shared.Domain;

internal sealed class AssignRoleCommandValidator : ICommandValidator<AssignRoleCommand>
{
    public IEnumerable<string> Validate(AssignRoleCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.ActorId))
        {
            yield return "Admin actor id is required.";
        }
        else if (!AdminActor.TrySystem(command.ActorId, out _))
        {
            yield return "Admin actor id is invalid.";
        }

        if (string.IsNullOrWhiteSpace(command.RoleName))
        {
            yield return "Admin role name is required.";
        }
        else if (!AdminRoleName.TryNormalize(command.RoleName, out _))
        {
            yield return "Admin role name is invalid.";
        }

        if (!string.IsNullOrWhiteSpace(command.TenantId) &&
            !TenantIds.TryNormalize(command.TenantId, out _))
        {
            yield return "Tenant id is invalid.";
        }
    }
}
