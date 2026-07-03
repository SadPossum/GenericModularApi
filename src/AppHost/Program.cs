using Aspire.Hosting.ApplicationModel;

var builder = DistributedApplication.CreateBuilder(args);

var sqlServer = builder.AddSqlServer("sql")
    .AddDatabase("SqlServer");

var postgreSql = builder.AddPostgres("postgres")
    .AddDatabase("PostgreSql");

var nats = builder.AddNats("nats")
    .WithJetStream()
    .WithDataVolume(isReadOnly: false);

var api = builder.AddProject<Projects.Host_Api>("host-api")
    .WithReference(sqlServer)
    .WithReference(postgreSql)
    .WithReference(nats)
    .WaitFor(nats)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("NatsJetStream__Enabled", "true");

bool adminApiEnabled = bool.TryParse(
    builder.Configuration["AppHost:AdminApi:Enabled"],
    out bool configuredAdminApiEnabled) && configuredAdminApiEnabled;

IResourceBuilder<ProjectResource>? adminApi = null;
if (adminApiEnabled)
{
    adminApi = builder.AddProject<Projects.Host_AdminApi>("host-admin-api")
        .WithReference(sqlServer)
        .WithReference(postgreSql)
        .WithReference(nats)
        .WaitFor(nats)
        .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
        .WithEnvironment("NatsJetStream__Enabled", "true");
}

bool redisEnabled = bool.TryParse(
    builder.Configuration["AppHost:Redis:Enabled"],
    out bool configuredRedisEnabled) && configuredRedisEnabled;

if (redisEnabled)
{
    var redis = builder.AddRedis("redis");
    api.WithReference(redis)
        .WaitFor(redis)
        .WithEnvironment("Caching__Enabled", "true")
        .WithEnvironment("Caching__Provider", "Redis");

    if (adminApi is { } configuredAdminApi)
    {
        configuredAdminApi.WithReference(redis)
            .WaitFor(redis)
            .WithEnvironment("Caching__Enabled", "true")
            .WithEnvironment("Caching__Provider", "Redis");
    }
}

builder.Build().Run();
