namespace Shared.Tasks;

public sealed record TaskRunStats(IReadOnlyList<TaskRunStatusCount> StatusCounts)
{
    public int Total => this.StatusCounts.Sum(item => item.Count);
}
