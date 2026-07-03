namespace Shared.Tests;

using Shared.Infrastructure.Messaging;
using Xunit;

[Trait("Category", "Unit")]
public sealed class MessagingWorkerIdsTests
{
    [Fact]
    public void Create_bounds_machine_name_to_mapped_worker_id_length()
    {
        Guid workerId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

        string value = MessagingWorkerIds.Create(new string('x', 512), workerId);

        Assert.Equal(MessagingWorkerIds.MaxLength, value.Length);
        Assert.EndsWith($":{workerId:N}", value, StringComparison.Ordinal);
    }

    [Fact]
    public void Create_rejects_empty_worker_guid()
    {
        Assert.Throws<ArgumentException>(() => MessagingWorkerIds.Create("node-1", Guid.Empty));
    }

    [Fact]
    public void Create_normalizes_machine_name_whitespace_and_control_characters()
    {
        Guid workerId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

        string value = MessagingWorkerIds.Create($" Node\tA{char.MinValue} ", workerId);

        Assert.Equal($"node-a-:{workerId:N}", value);
    }

    [Fact]
    public void Normalize_rejects_blank_and_overlong_worker_ids()
    {
        Assert.Throws<ArgumentException>(() => MessagingWorkerIds.Normalize(" "));
        Assert.Throws<ArgumentException>(() => MessagingWorkerIds.Normalize(new string('x', MessagingWorkerIds.MaxLength + 1)));
    }

    [Theory]
    [InlineData("worker 1")]
    [InlineData("worker\t1")]
    public void Normalize_rejects_whitespace_and_control_characters(string workerId)
    {
        Assert.Throws<ArgumentException>(() => MessagingWorkerIds.Normalize(workerId));
    }
}
