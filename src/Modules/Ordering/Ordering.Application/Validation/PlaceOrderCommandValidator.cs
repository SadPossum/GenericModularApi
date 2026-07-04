namespace Ordering.Application.Validation;

using Ordering.Application.Commands;
using Shared.Cqrs;

internal sealed class PlaceOrderCommandValidator : ICommandValidator<PlaceOrderCommand>
{
    public IEnumerable<string> Validate(PlaceOrderCommand command)
    {
        if (command.CatalogItemId == Guid.Empty)
        {
            yield return "Catalog item id is required.";
        }

        if (command.Quantity <= 0)
        {
            yield return "Quantity must be greater than zero.";
        }
    }
}
