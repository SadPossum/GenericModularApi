namespace Files.Application.Validation;

using Files.Application.Commands;
using Shared.AccessControl;
using Shared.Cqrs;

internal sealed class DeleteFileCommandValidator : ICommandValidator<DeleteFileCommand>
{
    public IEnumerable<string> Validate(DeleteFileCommand command)
    {
        if (command.FileId == Guid.Empty)
        {
            yield return "File id is required.";
        }

        if (command.Subject is null || command.Subject.Kind != AccessSubjectKind.User)
        {
            yield return "A user access subject is required.";
        }
    }
}
