namespace Ordering.Application.Validation;

using Ordering.Application.Queries;
using Shared.AccessControl;
using Shared.Cqrs;

internal sealed class ListOrdersQueryValidator : IQueryValidator<ListOrdersQuery>
{
    public IEnumerable<string> Validate(ListOrdersQuery query)
    {
        if (query.Subject is null || query.Subject.Kind != AccessSubjectKind.User)
        {
            yield return "A user access subject is required.";
        }
    }
}
