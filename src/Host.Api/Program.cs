using Auth.Api;
using Auth.Contracts;
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
using Shared.ModuleComposition;
using Shared.Notifications.Api;
using Shared.Notifications.Cqrs;
using Shared.Notifications.SignalR;
using Tenancy.Api;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseConfiguredSerilog();

builder.AddUserNotificationsCqrs();
builder.AddRedisCaching();
builder.AddCachingCqrs();
builder.AddSharedInfrastructure();
builder.AddMessagingInfrastructure();
builder.AddConfiguredNatsJetStreamMessaging();
builder.AddUserNotificationServerSentEvents();
builder.AddUserNotificationSignalR();
builder.Services.AddApiSecurityDefaults();

builder.AddModule<TenancyModule>();
builder.AddAuthModule(AuthProfile.TenantScoped());
// module-scaffold:public-api-modules

builder.AddServiceDefaults();
builder.AddSharedOpenApi();
builder.ValidateModuleComposition();

WebApplication app = builder.Build();

app.UseSharedOpenApi();
app.UseSharedSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();
app.MapModules();
app.MapUserNotificationServerSentEvents();
app.MapUserNotificationSignalR();

app.Run();
