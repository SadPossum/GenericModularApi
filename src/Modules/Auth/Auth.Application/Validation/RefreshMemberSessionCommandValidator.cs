namespace Auth.Application.Validation;

using Auth.Application.Commands;
using Shared.Cqrs;

internal sealed class RefreshMemberSessionCommandValidator : ICommandValidator<RefreshMemberSessionCommand>
{
    public IEnumerable<string> Validate(RefreshMemberSessionCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.AccessToken))
        {
            yield return "Access token is required.";
        }

        if (string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            yield return "Refresh token is required.";
        }
    }
}
