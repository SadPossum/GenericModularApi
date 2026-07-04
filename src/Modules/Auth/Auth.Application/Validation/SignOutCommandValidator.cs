namespace Auth.Application.Validation;

using Auth.Application.Commands;
using Shared.Cqrs;

internal sealed class SignOutCommandValidator : ICommandValidator<SignOutCommand>
{
    public IEnumerable<string> Validate(SignOutCommand command)
    {
        if (command.MemberId == Guid.Empty)
        {
            yield return "Member id is required.";
        }

        if (string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            yield return "Refresh token is required.";
        }
    }
}
