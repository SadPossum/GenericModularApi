namespace Shared.Api.Models;

public class ListResponse<T>
{
    public IEnumerable<T>? Items { get; set; }
    public int ItemsCount { get; set; }
    public int TotalItemsCount { get; set; }
    public DateTimeOffset DateTime { get; set; } = DateTimeOffset.Now;
}
