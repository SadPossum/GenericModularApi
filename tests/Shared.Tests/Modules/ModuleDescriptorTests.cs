namespace Shared.Tests;

using Shared.Application.Modules;
using Xunit;

[Trait("Category", "Unit")]
public sealed class ModuleDescriptorTests
{
    [Fact]
    public void Module_metadata_descriptors_are_constructor_only()
    {
        Type[] descriptorTypes =
        [
            typeof(ModuleDescriptor),
            typeof(ModulePermissionDescriptor),
            typeof(ModuleIntegrationEventDescriptor),
            typeof(ModuleSubscriptionDescriptor),
            typeof(ModuleCacheDescriptor)
        ];

        string[] writableProperties = descriptorTypes
            .SelectMany(type => type
                .GetProperties()
                .Where(property => property.SetMethod is not null)
                .Select(property => $"{type.Name}.{property.Name}"))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Empty(writableProperties);
    }

    [Fact]
    public void Module_metadata_descriptor_constructor_parameters_use_camel_case()
    {
        Type[] descriptorTypes =
        [
            typeof(ModuleDescriptor),
            typeof(ModulePermissionDescriptor),
            typeof(ModuleIntegrationEventDescriptor),
            typeof(ModuleSubscriptionDescriptor),
            typeof(ModuleCacheDescriptor)
        ];

        string[] offenders = descriptorTypes
            .SelectMany(type => type
                .GetConstructors()
                .SelectMany(constructor => constructor
                    .GetParameters()
                    .Where(parameter => parameter.Name is { Length: > 0 } name &&
                                        char.IsUpper(name[0]))
                    .Select(parameter => $"{type.Name}.{parameter.Name}")))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Empty(offenders);
    }

    [Fact]
    public void Module_descriptor_normalizes_public_metadata()
    {
        ModuleDescriptor descriptor = new(
            " Catalog ",
            " Catalog ",
            [new ModulePermissionDescriptor(" Catalog.Items.Read ", " Read catalog items. ", tenantScoped: true)],
            [new ModuleIntegrationEventDescriptor(" Item-Created ", " GMA.Catalog.Item-Created.V1 ", 1, tenantScoped: true)],
            [
                new ModuleSubscriptionDescriptor(
                    " Catalog ",
                    " Item-Created ",
                    " GMA.Catalog.Item-Created.V1 ",
                    " Item-Created-Projection ",
                    tenantScoped: true)
            ],
            [new ModuleCacheDescriptor(" Items ", " Tenant ", tenantScoped: true, [" Products "])],
            " Catalog-Admin ");

        Assert.Equal("catalog", descriptor.Name);
        Assert.Equal("catalog", descriptor.Schema);
        Assert.Equal("catalog-admin", descriptor.AdminSurfaceName);
        Assert.Equal("catalog.items.read", Assert.Single(descriptor.Permissions).Code);
        Assert.Equal("item-created", Assert.Single(descriptor.PublishedEvents).EventType);
        Assert.Equal("item-created-projection", Assert.Single(descriptor.Subscriptions).HandlerName);
        Assert.Equal("items", Assert.Single(descriptor.CacheEntries).Name);
        Assert.Equal(["products"], Assert.Single(descriptor.CacheEntries).Tags);
    }

    [Fact]
    public void Module_descriptor_rejects_duplicate_public_metadata()
    {
        Assert.Throws<ArgumentException>(() => new ModuleDescriptor(
            "catalog",
            "catalog",
            [
                new ModulePermissionDescriptor("catalog.items.read", "Read catalog items.", tenantScoped: true),
                new ModulePermissionDescriptor("Catalog.Items.Read", "Read catalog items.", tenantScoped: true)
            ],
            [],
            [],
            []));
    }

    [Fact]
    public void Module_descriptor_rejects_published_event_subject_for_another_module()
    {
        Assert.Throws<ArgumentException>(() => new ModuleDescriptor(
            "catalog",
            "catalog",
            [],
            [new ModuleIntegrationEventDescriptor("item-created", "gma.auth.item-created.v1", 1, tenantScoped: true)],
            [],
            []));
    }

    [Fact]
    public void Module_subscription_rejects_subject_that_does_not_match_producer_event()
    {
        Assert.Throws<ArgumentException>(() => new ModuleSubscriptionDescriptor(
            "catalog",
            "item-created",
            "gma.catalog.item-updated.v1",
            "item-created-projection",
            tenantScoped: true));
    }

    [Fact]
    public void Module_cache_metadata_rejects_scope_and_tenant_mismatch()
    {
        Assert.Throws<ArgumentException>(() => new ModuleCacheDescriptor(
            "items",
            "tenant",
            tenantScoped: false,
            ["items"]));
    }

    [Theory]
    [InlineData("catalog")]
    [InlineData("catalog..items.read")]
    [InlineData("catalog_items.read")]
    public void Module_permission_descriptor_rejects_invalid_permission_codes(string code)
    {
        Assert.Throws<ArgumentException>(() =>
            new ModulePermissionDescriptor(code, "Read catalog items.", tenantScoped: true));
    }
}

