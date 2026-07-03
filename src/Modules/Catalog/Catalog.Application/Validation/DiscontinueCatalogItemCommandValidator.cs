namespace Catalog.Application.Validation;

using Catalog.Application.Commands;
using Shared.Application.Cqrs;

internal sealed class DiscontinueCatalogItemCommandValidator : ICommandValidator<DiscontinueCatalogItemCommand>
{
    public IEnumerable<string> Validate(DiscontinueCatalogItemCommand command)
    {
        if (command.ItemId == Guid.Empty)
        {
            yield return "Catalog item id is required.";
        }
    }
}
