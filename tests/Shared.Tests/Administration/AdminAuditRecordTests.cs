namespace Shared.Tests;

using Shared.Naming;
using Shared.Administration;
using Xunit;

[Trait("Category", "Unit")]
public sealed class AdminAuditRecordTests
{
    private static readonly Guid Id = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
    private static readonly DateTimeOffset CreatedAtUtc = new(2026, 7, 2, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Constructor_normalizes_audit_metadata()
    {
        AdminAuditRecord record = new(
            Id,
            " Actor-1 ",
            " tenant-a ",
            " Auth.Members.List ",
            " Auth.Members.Read ",
            " Succeeded ",
            " Auth.Error ",
            CreatedAtUtc);

        Assert.Equal(Id, record.Id);
        Assert.Equal("Actor-1", record.ActorId);
        Assert.Equal("tenant-a", record.TenantId);
        Assert.Equal("auth.members.list", record.Operation);
        Assert.Equal("auth.members.read", record.Permission);
        Assert.Equal(AdminAuditResults.Succeeded, record.Result);
        Assert.Equal("Auth.Error", record.ErrorCode);
        Assert.Equal(CreatedAtUtc, record.CreatedAtUtc);
    }

    [Fact]
    public void Constructor_treats_blank_tenant_and_error_code_as_absent()
    {
        AdminAuditRecord record = Create(tenantId: " ", errorCode: " ");

        Assert.Null(record.TenantId);
        Assert.Null(record.ErrorCode);
    }

    [Theory]
    [InlineData("Succeeded", "succeeded")]
    [InlineData("denied", "denied")]
    [InlineData(" FAILED ", "failed")]
    public void Constructor_accepts_known_result_values(string input, string expected)
    {
        AdminAuditRecord record = Create(result: input);

        Assert.Equal(expected, record.Result);
    }

    [Fact]
    public void Constructor_rejects_empty_id()
    {
        Assert.Throws<ArgumentException>(() => Create(id: Guid.Empty));
    }

    [Fact]
    public void Constructor_rejects_invalid_actor_id()
    {
        Assert.Throws<ArgumentException>(() => Create(actorId: " "));
        Assert.Throws<ArgumentException>(() => Create(actorId: "actor 1"));
    }

    [Fact]
    public void Constructor_rejects_overlong_actor_id()
    {
        Assert.Throws<ArgumentException>(() => Create(actorId: new string('x', AdminActor.MaxLength + 1)));
    }

    [Fact]
    public void Constructor_rejects_invalid_tenant_id()
    {
        Assert.Throws<ArgumentException>(() => Create(tenantId: new string('x', TenantIds.MaxLength + 1)));
    }

    [Fact]
    public void Constructor_rejects_invalid_operation_name()
    {
        Assert.Throws<ArgumentException>(() => Create(operation: "auth"));
    }

    [Fact]
    public void Constructor_rejects_invalid_permission_code()
    {
        Assert.Throws<ArgumentException>(() => Create(permission: "auth"));
    }

    [Fact]
    public void Constructor_rejects_unknown_result()
    {
        Assert.Throws<ArgumentException>(() => Create(result: "maybe"));
    }

    [Fact]
    public void Constructor_rejects_overlong_error_code()
    {
        Assert.Throws<ArgumentException>(() => Create(errorCode: $"Auth.{new string('x', AdminAuditRecord.ErrorCodeMaxLength)}"));
    }

    [Fact]
    public void Constructor_rejects_ambiguous_error_code_characters()
    {
        Assert.Throws<ArgumentException>(() => Create(errorCode: "Auth Error"));
        Assert.Throws<ArgumentException>(() => Create(errorCode: $"Auth{char.MinValue}Error"));
        Assert.Throws<ArgumentException>(() => Create(errorCode: "Auth_Error"));
    }

    private static AdminAuditRecord Create(
        Guid? id = null,
        string actorId = "actor",
        string? tenantId = "tenant-a",
        string operation = "auth.members.list",
        string permission = "auth.members.read",
        string result = AdminAuditResults.Succeeded,
        string? errorCode = null) =>
        new(
            id ?? Id,
            actorId,
            tenantId,
            operation,
            permission,
            result,
            errorCode,
            CreatedAtUtc);
}
