namespace Files.Application.Validation;

using Files.Application.Queries;
using Shared.AccessControl;
using Shared.Cqrs;

internal sealed class GetFileQueryValidator : IQueryValidator<GetFileQuery>
{
    public IEnumerable<string> Validate(GetFileQuery query)
    {
        if (query.FileId == Guid.Empty)
        {
            yield return "File id is required.";
        }

        if (query.Subject is null || query.Subject.Kind != AccessSubjectKind.User)
        {
            yield return "A user access subject is required.";
        }
    }
}
