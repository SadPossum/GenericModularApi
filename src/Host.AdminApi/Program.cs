using Administration.AdminApi;
using Auth.AdminApi;
using ServiceDefaults;
using Shared.Administration.Api;
using Shared.Api.OpenApi;
using Shared.Api.Security;
using Shared.Api.Serilog;
using Shared.Caching.Redis;
using Shared.Infrastructure;
using Shared.Logging.Serilog;
using Shared.Messaging.Nats.Aspire;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseConfiguredSerilog();

builder.Services.AddSharedAdministrationApi(builder.Configuration);
builder.AddRedisCaching();
builder.AddSharedInfrastructure();
builder.AddConfiguredNatsJetStreamMessaging();
builder.Services.AddGmaApiSecurityDefaults();

builder.AddAdminApiModule<AdministrationAdminApiModule>();
builder.AddAdminApiModule<AuthAdminApiModule>();

builder.AddServiceDefaults();
builder.AddGmaOpenApi();

WebApplication app = builder.Build();

app.UseGmaOpenApi();
app.UseGmaSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();
app.MapAdminApiModules();

app.Run();
