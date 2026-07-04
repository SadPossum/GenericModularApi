namespace Catalog.Application.Validation;

using Catalog.Application.Commands;
using Shared.Cqrs;

internal sealed class CreateCatalogItemCommandValidator : ICommandValidator<CreateCatalogItemCommand>
{
    public IEnumerable<string> Validate(CreateCatalogItemCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Sku))
        {
            yield return "SKU is required.";
        }

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            yield return "Name is required.";
        }

        if (command.Price <= 0)
        {
            yield return "Price must be greater than zero.";
        }

        if (string.IsNullOrWhiteSpace(command.Currency))
        {
            yield return "Currency is required.";
        }
    }
}
