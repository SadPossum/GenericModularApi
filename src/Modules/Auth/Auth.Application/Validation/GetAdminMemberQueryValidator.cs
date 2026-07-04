namespace Auth.Application.Validation;

using Auth.Application.Queries;
using Shared.Cqrs;

internal sealed class GetAdminMemberQueryValidator : IQueryValidator<GetAdminMemberQuery>
{
    public IEnumerable<string> Validate(GetAdminMemberQuery query)
    {
        if (query.MemberId == Guid.Empty)
        {
            yield return "Member id is required.";
        }
    }
}
