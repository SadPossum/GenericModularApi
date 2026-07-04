using Auth.Api;
using ServiceDefaults;
using Shared.Api.Modules;
using Shared.Api.OpenApi;
using Shared.Api.Security;
using Shared.Api.Serilog;
using Shared.Caching.Cqrs;
using Shared.Caching.Redis;
using Shared.Infrastructure;
using Shared.Logging.Serilog;
using Shared.Messaging.Infrastructure;
using Shared.Messaging.Nats.Aspire;
using Tenancy.Api;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseConfiguredSerilog();

builder.AddRedisCaching();
builder.AddCachingCqrs();
builder.AddSharedInfrastructure();
builder.AddMessagingInfrastructure();
builder.AddConfiguredNatsJetStreamMessaging();
builder.Services.AddApiSecurityDefaults();

builder.AddModule<TenancyModule>();
builder.AddModule<AuthModule>();
// module-scaffold:public-api-modules

builder.AddServiceDefaults();
builder.AddSharedOpenApi();

WebApplication app = builder.Build();

app.UseSharedOpenApi();
app.UseSharedSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();
app.MapModules();

app.Run();
