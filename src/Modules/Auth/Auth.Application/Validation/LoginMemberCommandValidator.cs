namespace Auth.Application.Validation;

using Auth.Application.Commands;
using Shared.Cqrs;

internal sealed class LoginMemberCommandValidator : ICommandValidator<LoginMemberCommand>
{
    public IEnumerable<string> Validate(LoginMemberCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Username))
        {
            yield return "Username is required.";
        }

        if (string.IsNullOrWhiteSpace(command.Password))
        {
            yield return "Password is required.";
        }
    }
}
