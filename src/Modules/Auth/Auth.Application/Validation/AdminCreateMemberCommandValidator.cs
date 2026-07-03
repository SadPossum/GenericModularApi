namespace Auth.Application.Validation;

using Auth.Application.Commands;
using Auth.Application.Security;
using Shared.Application.Cqrs;

internal sealed class AdminCreateMemberCommandValidator : ICommandValidator<AdminCreateMemberCommand>
{
    public IEnumerable<string> Validate(AdminCreateMemberCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Username))
        {
            yield return "Username is required.";
        }

        if (!AuthPasswordPolicy.IsValidPlaintextPassword(command.Password))
        {
            yield return AuthPasswordPolicy.MinimumLengthMessage;
        }
    }
}
