namespace Auth.Application.Validation;

using Auth.Application.Commands;
using Shared.Application.Cqrs;

internal sealed class EnableMemberCommandValidator : ICommandValidator<EnableMemberCommand>
{
    public IEnumerable<string> Validate(EnableMemberCommand command)
    {
        if (command.MemberId == Guid.Empty)
        {
            yield return "Member id is required.";
        }
    }
}
