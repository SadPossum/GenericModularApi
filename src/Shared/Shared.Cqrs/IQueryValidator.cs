namespace Shared.Cqrs;

public interface IQueryValidator<in TQuery>
{
    IEnumerable<string> Validate(TQuery query);
}
