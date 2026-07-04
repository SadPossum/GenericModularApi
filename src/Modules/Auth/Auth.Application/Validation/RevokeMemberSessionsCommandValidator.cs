namespace Auth.Application.Validation;

using Auth.Application.Commands;
using Shared.Cqrs;

internal sealed class RevokeMemberSessionsCommandValidator : ICommandValidator<RevokeMemberSessionsCommand>
{
    public IEnumerable<string> Validate(RevokeMemberSessionsCommand command)
    {
        if (command.MemberId == Guid.Empty)
        {
            yield return "Member id is required.";
        }
    }
}
