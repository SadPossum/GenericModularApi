namespace Catalog.Tests;

using Catalog.Domain.ValueObjects;
using Catalog.Persistence;
using Microsoft.EntityFrameworkCore;
using Shared.Tenancy;
using Xunit;

[Trait("Category", "Unit")]
public sealed class CatalogItemQueryTranslationTests
{
    [Fact]
    public void Projection_export_cursor_query_translates_with_sku_value_object()
    {
        DbContextOptions<CatalogDbContext> options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseSqlServer("Server=localhost;Database=query-translation;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;
        using CatalogDbContext dbContext = new(options, new TestTenantContext("tenant-a"));
        CatalogSku cursorSku = CatalogSku.Create("SKU-1").Value;

        string sql = dbContext.CatalogItems
            .FromSqlInterpolated($"""
                SELECT *
                FROM [catalog].[items]
                WHERE [Sku] > {cursorSku.Value}
                """)
            .AsNoTracking()
            .OrderBy(item => item.Sku)
            .ToQueryString();

        Assert.Contains("Sku", sql, StringComparison.Ordinal);
    }

    private sealed class TestTenantContext(string tenantId) : ITenantContext
    {
        public bool IsEnabled => true;
        public string? TenantId { get; } = tenantId;
    }
}
