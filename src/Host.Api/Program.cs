using Auth.Api;
using ServiceDefaults;
using Shared.Api.Modules;
using Shared.Api.OpenApi;
using Shared.Api.Security;
using Shared.Api.Serilog;
using Shared.Caching.Infrastructure;
using Shared.Caching.Redis;
using Shared.Infrastructure;
using Shared.Logging.Serilog;
using Shared.Messaging.Infrastructure;
using Shared.Messaging.Nats.Aspire;
using Tenancy.Api;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseConfiguredSerilog();

builder.AddRedisCaching();
builder.AddCachingInfrastructure();
builder.AddSharedInfrastructure();
builder.AddMessagingInfrastructure();
builder.AddConfiguredNatsJetStreamMessaging();
builder.Services.AddGmaApiSecurityDefaults();

builder.AddModule<TenancyModule>();
builder.AddModule<AuthModule>();
// gma:new-module:public-api-modules

builder.AddServiceDefaults();
builder.AddGmaOpenApi();

WebApplication app = builder.Build();

app.UseGmaOpenApi();
app.UseGmaSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();
app.MapModules();

app.Run();
