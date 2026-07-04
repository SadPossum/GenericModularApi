using Administration.AdminApi;
using Auth.AdminApi;
using ServiceDefaults;
using Shared.Administration.Api;
using Shared.Api.OpenApi;
using Shared.Api.Security;
using Shared.Api.Serilog;
using Shared.Caching.Cqrs;
using Shared.Caching.Redis;
using Shared.Infrastructure;
using Shared.Logging.Serilog;
using Shared.Messaging.Infrastructure;
using Shared.Messaging.Nats.Aspire;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseConfiguredSerilog();

builder.Services.AddSharedAdministrationApi(builder.Configuration);
builder.AddRedisCaching();
builder.AddCachingCqrs();
builder.AddSharedInfrastructure();
builder.AddMessagingInfrastructure();
builder.AddConfiguredNatsJetStreamMessaging();
builder.Services.AddApiSecurityDefaults();

builder.AddAdminApiModule<AdministrationAdminApiModule>();
builder.AddAdminApiModule<AuthAdminApiModule>();

builder.AddServiceDefaults();
builder.AddSharedOpenApi();

WebApplication app = builder.Build();

app.UseSharedOpenApi();
app.UseSharedSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();
app.MapAdminApiModules();

app.Run();
