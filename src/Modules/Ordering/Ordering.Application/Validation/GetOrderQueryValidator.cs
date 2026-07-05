namespace Ordering.Application.Validation;

using Ordering.Application.Queries;
using Shared.Cqrs;

internal sealed class GetOrderQueryValidator : IQueryValidator<GetOrderQuery>
{
    public IEnumerable<string> Validate(GetOrderQuery query)
    {
        if (query.OrderId == Guid.Empty)
        {
            yield return "Order id is required.";
        }
    }
}
