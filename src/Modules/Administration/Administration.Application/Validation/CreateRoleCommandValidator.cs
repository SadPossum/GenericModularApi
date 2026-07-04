namespace Administration.Application.Validation;

using Administration.Application.Commands;
using Shared.Cqrs;

internal sealed class CreateRoleCommandValidator : ICommandValidator<CreateRoleCommand>
{
    public IEnumerable<string> Validate(CreateRoleCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            yield return "Admin role name is required.";
        }
        else if (!AdminRoleName.TryNormalize(command.Name, out _))
        {
            yield return "Admin role name is invalid.";
        }
    }
}
