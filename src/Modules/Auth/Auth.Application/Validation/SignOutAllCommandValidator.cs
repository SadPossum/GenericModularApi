namespace Auth.Application.Validation;

using Auth.Application.Commands;
using Shared.Application.Cqrs;

internal sealed class SignOutAllCommandValidator : ICommandValidator<SignOutAllCommand>
{
    public IEnumerable<string> Validate(SignOutAllCommand command)
    {
        if (command.MemberId == Guid.Empty)
        {
            yield return "Member id is required.";
        }
    }
}
