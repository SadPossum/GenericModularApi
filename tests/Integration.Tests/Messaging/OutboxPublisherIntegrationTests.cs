namespace Integration.Tests;

using DotNet.Testcontainers.Containers;
using Integration.Tests.Support;
using Testcontainers.PostgreSql;
using Xunit;

public sealed class OutboxPublisherIntegrationTests
{
    [DockerFact]
    [Trait("Category", "Docker")]
    [Trait("Category", "Integration")]
    public async Task Member_registered_event_is_published_and_marked_processed()
    {
        await using IContainer nats = AuthTestContainers.CreateNatsContainer();
        await using PostgreSqlContainer postgreSql = new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase("gma_auth_tests")
            .Build();
        await nats.StartAsync();
        await postgreSql.StartAsync();

        await using AuthTestApplication application = new(
            "PostgreSql",
            postgreSql.GetConnectionString(),
            AuthTestContainers.GetNatsConnectionString(nats),
            disableOutboxPublisher: false);

        await application.MigrateDatabaseAsync();
        using HttpClient client = application.CreateClient();

        await AuthApiClient.RegisterAsync(client, "tenant-events", "events@example.com");

        int processedAfterPublish = await application.WaitForProcessedOutboxMessagesAsync(1, TimeSpan.FromSeconds(20));
        int pendingAfterPublish = await application.CountPendingOutboxMessagesAsync();

        Assert.Equal(1, processedAfterPublish);
        Assert.Equal(0, pendingAfterPublish);
    }
}
